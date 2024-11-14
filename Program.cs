
using System.Diagnostics;

string version = "0.0.14";

SentryMethods.ConfigureSentry();

string exePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName) ?? "";
Console.WriteLine($"Exe Path {exePath}");

CmdLineArguments.ProcessCmdLineArguments(args);

//Display version so can compare with Website
Console.WriteLine($"Version {version} - Ensure matches against website for compatibility");

if (GlobalVars.UnPackRefs)
{
    UnPacking.UnpackRefs(GlobalVars.RepoPath);
    UnPacking.UnPackPackFile(GlobalVars.RepoPath);
}

if (exePath == GlobalVars.RepoPath)
{
    Console.WriteLine("VisualGit cannot be run in the same folder as the Repository to be examined");
    Console.WriteLine("Option1: Place Visual.exe into another folder and run with --p pointing to this folder");
    Console.WriteLine("Option2: Place the Visual.exe application into a folder on your PATH. Then just run Visual from within the Repository as you just did");
    return;
}

VisualGit.Run();

FileWatching.OnChangedDelegate handler = FileWatching.OnChanged;
FileWatching.CreateFileWatcher(handler);



// string password = builder.Build().GetSection("docker").GetSection("password").Value;
// string uri = builder.Build().GetSection("docker").GetSection("url").Value;
// string username = builder.Build().GetSection("docker").GetSection("username").Value;

// string password = builder.Build().GetSection("cloud").GetSection("password").Value;
// string uri = builder.Build().GetSection("cloud").GetSection("url").Value;
// string username = builder.Build().GetSection("cloud").GetSection("username").Value;

// Initial Run to check for files without detecting any file changes
//VisualGit _visualGit = new VisualGit();
