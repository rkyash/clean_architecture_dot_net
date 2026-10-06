using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MosquitoApi.Application.DTOs;
using MosquitoApi.Application.DTOs.User;
using MosquitoApi.Application.Interfaces;
using MosquitoApi.Application.Mappings;
using MosquitoApi.Application.Services;
using MosquitoApi.Application.Validators;

namespace MosquitoApi.Application.DependencyInjection;

/// <summary>
/// Extension methods for registering application layer services
/// </summary>
public static class ApplicationServiceRegistration
{
    /// <summary>
    /// Registers application layer services in the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper
        var mapperConfig = new MapperConfiguration(config =>
        {
            config.AddProfile<MosquitoMappingProfile>();
            config.AddProfile<UserMappingProfile>();
        });
        services.AddSingleton(mapperConfig.CreateMapper());

        // Register FluentValidation validators manually
        services.AddScoped<IValidator<CreateMosquitoRequest>, CreateMosquitoRequestValidator>();
        services.AddScoped<IValidator<UpdateMosquitoRequest>, UpdateMosquitoRequestValidator>();
        services.AddScoped<IValidator<MosquitoSearchRequest>, MosquitoSearchRequestValidator>();
        
        // User management validators
        services.AddScoped<IValidator<DTOs.User.CreateUserRequest>, CreateUserRequestValidator>();
        services.AddScoped<IValidator<DTOs.User.UpdateUserRequest>, UpdateUserRequestValidator>();
        services.AddScoped<IValidator<DTOs.User.UserSearchRequest>, UserSearchRequestValidator>();

        // Configure FluentValidation options
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

        // Register application services
        services.AddScoped<IMosquitoService, MosquitoService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();

        return services;
    }
}