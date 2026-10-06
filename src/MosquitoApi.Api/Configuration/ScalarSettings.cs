namespace MosquitoApi.Api.Configuration;

/// <summary>
/// Configuration settings for Scalar API documentation
/// </summary>
public class ScalarSettings
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Scalar";

    /// <summary>
    /// API documentation title
    /// </summary>
    public string Title { get; set; } = "API Documentation";

    /// <summary>
    /// API description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// API version
    /// </summary>
    public string Version { get; set; } = "v1.0.0";

    /// <summary>
    /// Scalar theme (light, dark, purple, etc.)
    /// </summary>
    public string Theme { get; set; } = "purple";

    /// <summary>
    /// Whether to show sidebar
    /// </summary>
    public bool ShowSidebar { get; set; } = true;

    /// <summary>
    /// Whether to hide download button
    /// </summary>
    public bool HideDownloadButton { get; set; } = false;

    /// <summary>
    /// Whether to use dark mode
    /// </summary>
    public bool DarkMode { get; set; } = true;

    /// <summary>
    /// Layout style (modern, classic)
    /// </summary>
    public string Layout { get; set; } = "modern";

    /// <summary>
    /// Custom CSS file path
    /// </summary>
    public string? CustomCss { get; set; }

    /// <summary>
    /// Server configurations
    /// </summary>
    public List<ScalarServer> Servers { get; set; } = new();

    /// <summary>
    /// Whether Scalar UI is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Route path for Scalar UI
    /// </summary>
    public string Route { get; set; } = "/scalar/v1";

    /// <summary>
    /// OpenAPI specification path
    /// </summary>
    public string OpenApiRoute { get; set; } = "/openapi/v1.json";

    /// <summary>
    /// Whether to show request/response examples
    /// </summary>
    public bool ShowExamples { get; set; } = true;

    /// <summary>
    /// Whether to enable interactive mode
    /// </summary>
    public bool InteractiveMode { get; set; } = true;

    /// <summary>
    /// Default expand level for operations
    /// </summary>
    public int DefaultExpandLevel { get; set; } = 1;

    /// <summary>
    /// Custom favicon URL
    /// </summary>
    public string? FaviconUrl { get; set; }

    /// <summary>
    /// Custom logo URL
    /// </summary>
    public string? LogoUrl { get; set; }
}

/// <summary>
/// Server configuration for Scalar
/// </summary>
public class ScalarServer
{
    /// <summary>
    /// Server URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Server description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Server variables
    /// </summary>
    public Dictionary<string, ScalarServerVariable> Variables { get; set; } = new();
}

/// <summary>
/// Server variable configuration for Scalar
/// </summary>
public class ScalarServerVariable
{
    /// <summary>
    /// Default value
    /// </summary>
    public string Default { get; set; } = string.Empty;

    /// <summary>
    /// Variable description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Enum values for the variable
    /// </summary>
    public List<string> Enum { get; set; } = new();
}