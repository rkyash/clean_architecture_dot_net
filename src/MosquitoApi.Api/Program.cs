using FluentValidation;
using MosquitoApi.Api.Extensions;
using MosquitoApi.Application.DependencyInjection;
using MosquitoApi.Domain.Configuration;
using MosquitoApi.Infrastructure.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure configuration sources
builder.Configuration.AddAppConfiguration(
    builder.Environment.EnvironmentName, 
    builder.Environment.ContentRootPath);

// Validate configuration
builder.Configuration.ValidateConfiguration();



// Add AppSettings to services - supports both patterns
builder.Services.AddAppSettings(builder.Configuration);

// Alternative: Your preferred singleton pattern (simpler approach)
// AppSettings.Instance.Bind(builder.Configuration);  // Your exact pattern
// OR even simpler:
// builder.Configuration.BindAppSettings();  // Extension method approach

// Configure Serilog with AppSettings
builder.Host.UseSerilog((context, configuration) =>
{
    var appName = AppSettingsManager.ApplicationSettings.Name;
    
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ApplicationName", appName)
        .Enrich.WithProperty("Environment", AppSettingsManager.Environment)
        .Enrich.WithProperty("Version", AppSettingsManager.ApplicationSettings.Version)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
});

// Add services to the container
builder.Services.AddControllers();

// Add custom service registrations
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add API-specific services
builder.Services.AddApiServices(builder.Configuration);

// Add health checks
builder.Services.AddInfrastructureHealthChecks(builder.Configuration);

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Log configuration summary
var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.Configuration.LogConfigurationSummary(logger);

// Configure the HTTP request pipeline
await app.ConfigurePipelineAsync();

// Ensure databases are created and migrated
if (app.Environment.IsDevelopment())
{
    await InfrastructureServiceRegistration.EnsureDatabasesCreatedAsync(app.Services);
}

// Configure endpoints
app.ConfigureEndpoints();

// Start the application
try
{
    Log.Information("Starting Mosquito API");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}