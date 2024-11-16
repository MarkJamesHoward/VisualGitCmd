SentryMethods.ConfigureSentry();

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



