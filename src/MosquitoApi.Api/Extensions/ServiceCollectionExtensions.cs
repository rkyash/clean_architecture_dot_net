using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MosquitoApi.Application.Configuration;
using MosquitoApi.Api.Middleware;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace MosquitoApi.Api.Extensions;

/// <summary>
/// Extension methods for configuring API services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds API-specific services to the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add JWT authentication
        services.AddJwtAuthenticationServices(configuration);

        // Add OpenAPI/Swagger services
        services.AddOpenApiServices(configuration);

        // Add CORS
        services.AddCorsServices(configuration);

        // Add rate limiting
        services.AddRateLimitingServices(configuration);

        // Add middleware services
        services.AddMiddlewareServices();

        // Add HTTP context accessor
        services.AddHttpContextAccessor();

        // Add problem details
        services.AddProblemDetails();

        // Add endpoint API explorer
        services.AddEndpointsApiExplorer();

        return services;
    }

    /// <summary>
    /// Adds JWT authentication and authorization services
    /// </summary>
    private static IServiceCollection AddJwtAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT settings
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() 
            ?? throw new InvalidOperationException("JWT settings are not configured");
        
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Add authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = !Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = jwtSettings.ValidateIssuer,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = jwtSettings.ValidateAudience,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = jwtSettings.ValidateLifetime,
                ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes),
                RequireExpirationTime = true,
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role
            };

            // JWT Bearer events
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    // Additional token validation logic can be added here
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var response = new { message = "You are not authorized to access this resource." };
                    return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    var response = new { message = "You do not have permission to access this resource." };
                    return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
                }
            };
        });

        // Add authorization with policies
        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy("AdminOnly", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("role", "Admin", "SuperAdmin"))
            .AddPolicy("ModeratorOrAdmin", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("role", "Moderator", "Admin", "SuperAdmin"))
            .AddPolicy("EmailVerified", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("email_verified", "True"))
            .AddPolicy("ActiveUser", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_active", "True"));

        return services;
    }

    /// <summary>
    /// Adds OpenAPI and Scalar documentation services
    /// </summary>
    private static IServiceCollection AddOpenApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        // services.AddSwaggerGen(options =>
        // {
        //     options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        //     {
        //         Title = "Mosquito API",
        //         Version = "v1.0.0",
        //         Description = "A comprehensive API for managing mosquito data with Clean Architecture",
        //         Contact = new Microsoft.OpenApi.Models.OpenApiContact
        //         {
        //             Name = "Mosquito API Team",
        //             Email = "support@mosquitoapi.com"
        //         }
        //     });
        //
        //     // Include XML comments if available
        //     var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        //     var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        //     if (File.Exists(xmlPath))
        //     {
        //         options.IncludeXmlComments(xmlPath);
        //     }
        // });

        return services;
    }

    /// <summary>
    /// Adds CORS services with configuration
    /// </summary>
    private static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }
                else
                {
                    policy.AllowAnyOrigin();
                }

                policy.AllowAnyMethod()
                      .AllowAnyHeader();

                // Only allow credentials if not using AllowAnyOrigin
                if (allowedOrigins.Length > 0)
                {
                    policy.AllowCredentials();
                }
            });

            // Add a specific policy for development
            options.AddPolicy("Development", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    /// <summary>
    /// Adds rate limiting services
    /// </summary>
    private static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limiter
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // API-specific rate limiter
            options.AddFixedWindowLimiter("ApiLimiter", limiterOptions =>
            {
                limiterOptions.PermitLimit = 50;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 10;
            });

            // Search rate limiter (more restrictive)
            options.AddFixedWindowLimiter("SearchLimiter", limiterOptions =>
            {
                limiterOptions.PermitLimit = 20;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 5;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }

    /// <summary>
    /// Adds custom middleware services
    /// </summary>
    private static IServiceCollection AddMiddlewareServices(this IServiceCollection services)
    {
        // Middleware is added to pipeline in WebApplicationExtensions, not registered as services
        // No need to register middleware classes as services since they're instantiated by the pipeline
        
        return services;
    }
}