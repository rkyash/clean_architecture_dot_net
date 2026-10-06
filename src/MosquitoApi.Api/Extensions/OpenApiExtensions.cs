using Microsoft.OpenApi.Models;
using MosquitoApi.Api.Configuration;
using System.Reflection;

namespace MosquitoApi.Api.Extensions;

/// <summary>
/// Extensions for configuring OpenAPI documentation
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Adds and configures OpenAPI services
    /// </summary>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        var scalarSettings = configuration.GetSection(ScalarSettings.SectionName).Get<ScalarSettings>() ?? new ScalarSettings();

        services.AddOpenApi(options =>
        {
            
            // Configure OpenAPI document
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = scalarSettings.Title,
                    Description = scalarSettings.Description,
                    Version = scalarSettings.Version,
                    Contact = new OpenApiContact
                    {
                        Name = "Mosquito API Support",
                        Email = "support@mosquitoapi.com",
                        Url = new Uri("https://mosquitoapi.com/support")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    },
                    TermsOfService = new Uri("https://mosquitoapi.com/terms")
                };

                // Add servers
                if (scalarSettings.Servers.Any())
                {
                    document.Servers = scalarSettings.Servers.Select(s => new OpenApiServer
                    {
                        Url = s.Url,
                        Description = s.Description,
                        Variables = s.Variables.ToDictionary(
                            kv => kv.Key,
                            kv => new OpenApiServerVariable
                            {
                                Default = kv.Value.Default,
                                Description = kv.Value.Description,
                                Enum = kv.Value.Enum.Any() ? kv.Value.Enum : null
                            })
                    }).ToList();
                }

                // Add security schemes
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer 12345abcdef\""
                    },
                    ["ApiKey"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.ApiKey,
                        In = ParameterLocation.Header,
                        Name = "X-API-Key",
                        Description = "API Key authentication"
                    }
                };

                // Add global security requirement (optional)
                document.SecurityRequirements = new List<OpenApiSecurityRequirement>();

                // Add common response types
                AddCommonResponseTypes(document);

                // Add tags
                AddApiTags(document);

                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>
    /// Adds common response types to the OpenAPI document
    /// </summary>
    private static void AddCommonResponseTypes(OpenApiDocument document)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.Responses ??= new Dictionary<string, OpenApiResponse>();

        // Add common responses
        document.Components.Responses["BadRequest"] = new OpenApiResponse
        {
            Description = "Bad Request - Invalid input or validation errors",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["type"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("validation") },
                            ["title"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("Validation Error") },
                            ["detail"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("One or more validation errors occurred") },
                            ["errors"] = new OpenApiSchema { Type = "object", AdditionalProperties = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string" } } }
                        }
                    }
                }
            }
        };

        document.Components.Responses["NotFound"] = new OpenApiResponse
        {
            Description = "Not Found - The requested resource was not found",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["type"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("not_found") },
                            ["title"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("Resource Not Found") },
                            ["detail"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("The requested resource was not found") }
                        }
                    }
                }
            }
        };

        document.Components.Responses["InternalServerError"] = new OpenApiResponse
        {
            Description = "Internal Server Error - An unexpected error occurred",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["type"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("server_error") },
                            ["title"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("Internal Server Error") },
                            ["detail"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("An unexpected error occurred") }
                        }
                    }
                }
            }
        };
    }

    /// <summary>
    /// Adds API tags to the OpenAPI document
    /// </summary>
    private static void AddApiTags(OpenApiDocument document)
    {
        document.Tags = new List<OpenApiTag>
        {
            new OpenApiTag
            {
                Name = "Mosquitoes",
                Description = "Operations for managing mosquito data",
                ExternalDocs = new OpenApiExternalDocs
                {
                    Description = "Learn more about mosquitoes",
                    Url = new Uri("https://en.wikipedia.org/wiki/Mosquito")
                }
            },
            new OpenApiTag
            {
                Name = "Health",
                Description = "Health check endpoints for monitoring application status"
            },
            new OpenApiTag
            {
                Name = "Audit",
                Description = "Operations for accessing audit trails and system logs"
            },
            new OpenApiTag
            {
                Name = "Statistics",
                Description = "Operations for retrieving statistical data and reports"
            }
        };
    }
}