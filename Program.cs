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

// Perform intial check of files
GitRepoExaminer.Run();

/// Now setup event handler for checking when files are modified
FileWatching.CreateFileWatcher();

// string password = builder.Build().GetSection("docker").GetSection("password").Value;
// string uri = builder.Build().GetSection("docker").GetSection("url").Value;
// string username = builder.Build().GetSection("docker").GetSection("username").Value;

// string password = builder.Build().GetSection("cloud").GetSection("password").Value;
// string uri = builder.Build().GetSection("cloud").GetSection("url").Value;
// string username = builder.Build().GetSection("cloud").GetSection("username").Value;

