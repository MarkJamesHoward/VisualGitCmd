SentryMethods.ConfigureSentry();

// Initialize configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Properties/launchSettings.json", optional: true, reloadOnChange: true)
    .Build();

ApiConfigurationProvider.Initialize(configuration);

// Determine where we are running
FilePath.GetExeFilePath();

MyLogging.CreateLogger();

// Check which folder the user would like to examine
CmdLineArguments.ProcessCmdLineArguments(args);

//Display version so can compare with Website
StandardMessages.DisplayVersion();

UnPacking.PerformUnpackingIfRequested();

SameFolderCheck.Validate();

RandomName.GenerateRandomName();

// Perform intial check of files
GitRepoExaminer.Run();

/// Now setup event handler for checking when files are modified
FileWatching.CreateFileWatcher();
