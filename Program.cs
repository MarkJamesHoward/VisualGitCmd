SentryMethods.ConfigureSentry();

// Determine where we are running
FilePath.GetExeFilePath();

// Check which folder the user would like to examine
CmdLineArguments.ProcessCmdLineArguments(args);

//Display version so can compare with Website
StandardMessages.DisplayVersion();

// If Unpacking then perform this first prior to looking at the files in the repo
if (GlobalVars.UnPackRefs)
{
    UnPacking.UnpackRefs(GlobalVars.RepoPath);
    UnPacking.UnPackPackFile(GlobalVars.RepoPath);
}

// Remeber best not to run VisualGit in the repo being examined
if (GlobalVars.exePath == GlobalVars.RepoPath)
{
    StandardMessages.SameFolderMessage();
    return;
}

// Perform intial check of files
VisualGit.Run();

/// Now setup event handler for checking when files are modified
FileWatching.CreateFileWatcher();

// string password = builder.Build().GetSection("docker").GetSection("password").Value;
// string uri = builder.Build().GetSection("docker").GetSection("url").Value;
// string username = builder.Build().GetSection("docker").GetSection("username").Value;

// string password = builder.Build().GetSection("cloud").GetSection("password").Value;
// string uri = builder.Build().GetSection("cloud").GetSection("url").Value;
// string username = builder.Build().GetSection("cloud").GetSection("username").Value;

