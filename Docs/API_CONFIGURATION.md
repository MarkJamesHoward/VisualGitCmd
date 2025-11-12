# API Configuration

This application supports multiple API endpoints through configuration files. The API URLs are no longer hardcoded in the source code and can be managed through configuration.

## Configuration Files

### appsettings.json
The primary configuration file containing API endpoints:

```json
{
  "ApiEndpoints": {
    "LocalDebug": "https://localhost:5078/api/gitinternals",
    "AzureAppService": "https://gitvisualiserapi.azurewebsites.net/api/gitinternals",
    "GoogleCloud": "https://vgit-api-729645510879.australia-southeast1.run.app/api/gitinternals",
    "AzureContainerApps": "https://visual-git-api.livelyforest-4bf24ad1.australiasoutheast.azurecontainerapps.io/api/gitinternals"
  }
}
```

### Properties/launchSettings.json
Also contains the API endpoints and launch profiles for development.

## API Endpoint Selection Logic

The application selects the API endpoint in the following order:

1. **Local Debug Mode**: If `GlobalVars.LocalDebugAPI` is true, uses the LocalDebug endpoint
2. **Command Line Override**: If `GlobalVars.Api` is provided via command line, uses that URL
3. **Default**: Uses the GoogleCloud endpoint as the default

## Usage

The `ApiConfigurationProvider` class provides access to the configured endpoints:

```csharp
// Get the appropriate base address based on current settings
Uri baseAddress = ApiConfigurationProvider.Instance.GetBaseAddress();

// Get all available endpoints
var allEndpoints = ApiConfigurationProvider.Instance.GetAllEndpoints();

// Get specific endpoint URLs
string localUrl = ApiConfigurationProvider.Instance.LocalDebugUrl;
string azureUrl = ApiConfigurationProvider.Instance.AzureAppServiceUrl;
string googleUrl = ApiConfigurationProvider.Instance.GoogleCloudUrl;
string containerUrl = ApiConfigurationProvider.Instance.AzureContainerAppsUrl;
```

## Benefits

- **Maintainability**: API URLs are centralized in configuration files
- **Flexibility**: Easy to modify endpoints without recompiling
- **Environment-specific**: Different configurations for development, staging, and production
- **Documentation**: Clear separation of concerns with documented endpoint purposes