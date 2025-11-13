using System.Reflection;
using Microsoft.Extensions.Configuration;

public class ApiConfiguration
{
    private readonly IConfiguration _configuration;

    public ApiConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Local debug API endpoint for development
    /// </summary>
    public string LocalDebugUrl =>
        _configuration["ApiEndpoints:LocalDebug"] ?? "https://localhost:5078/api/gitinternals";

    /// <summary>
    /// Azure App Service hosted API endpoint
    /// </summary>
    public string AzureAppServiceUrl =>
        _configuration["ApiEndpoints:AzureAppService"]
        ?? "https://gitvisualiserapi.azurewebsites.net/api/gitinternals";

    /// <summary>
    /// Google Cloud hosted API endpoint (default)
    /// </summary>
    public string GoogleCloudUrl =>
        _configuration["ApiEndpoints:GoogleCloud"]
        ?? "https://vgit-api-729645510879.australia-southeast1.run.app/api/gitinternals";

    /// <summary>
    /// Azure Container Apps hosted API endpoint
    /// </summary>
    public string AzureContainerAppsUrl =>
        _configuration["ApiEndpoints:AzureContainerApps"]
        ?? "https://visual-git-api.livelyforest-4bf24ad1.australiasoutheast.azurecontainerapps.io/api/gitinternals";

    /// <summary>
    /// Application version from configuration
    /// </summary>
    public string Version
    {
        get
        {
            var fullVersion =
                Assembly
                    .GetEntryAssembly()
                    ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion ?? "1.0.0-unknown";

            // Remove git hash (everything after '+' character)
            var plusIndex = fullVersion.IndexOf('+');
            return plusIndex >= 0 ? fullVersion.Substring(0, plusIndex) : fullVersion;
        }
    }

    /// <summary>
    /// Application name from configuration
    /// </summary>
    public string ApplicationName =>
        _configuration["ApplicationSettings:ApplicationName"] ?? "Visual Git Command";

    /// <summary>
    /// Gets the appropriate base address URI based on current configuration
    /// </summary>
    /// <returns>The base address URI to use for API calls</returns>
    public Uri GetAPIURL()
    {
        if (GlobalVars.LocalDebugAPI)
        {
            return new Uri(LocalDebugUrl);
        }
        else if (GlobalVars.Api != null && GlobalVars.Api != "")
        {
            return new Uri(GlobalVars.Api);
        }
        else
        {
            // Default to Azure Cloud version
            return new Uri(AzureContainerAppsUrl);
        }
    }

    public string GetAPIRLUrlEncoded()
    {
        if (GlobalVars.LocalDebugAPI)
        {
            return System.Net.WebUtility.UrlEncode(LocalDebugUrl);
        }
        else if (GlobalVars.Api != null && GlobalVars.Api != "")
        {
            return System.Net.WebUtility.UrlEncode(GlobalVars.Api);
        }
        else
        {
            // Default to Azure Container App version
            return System.Net.WebUtility.UrlEncode(AzureContainerAppsUrl);
        }
    }

    /// <summary>
    /// Gets all configured API endpoints
    /// </summary>
    /// <returns>Dictionary of endpoint names and URLs</returns>
    public Dictionary<string, string> GetAllEndpoints()
    {
        return new Dictionary<string, string>
        {
            { "LocalDebug", LocalDebugUrl },
            { "AzureAppService", AzureAppServiceUrl },
            { "GoogleCloud", GoogleCloudUrl },
            { "AzureContainerApps", AzureContainerAppsUrl },
        };
    }
}

public static class ApiConfigurationProvider
{
    private static ApiConfiguration? _instance;

    public static ApiConfiguration Instance
    {
        get
        {
            if (_instance == null)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile(
                        "Properties/launchSettings.json",
                        optional: true,
                        reloadOnChange: true
                    )
                    .Build();

                _instance = new ApiConfiguration(configuration);
            }
            return _instance;
        }
    }

    public static void Initialize(IConfiguration configuration)
    {
        _instance = new ApiConfiguration(configuration);
    }
}
