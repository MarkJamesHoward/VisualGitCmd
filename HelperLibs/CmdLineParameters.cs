using Yargs;
using CommandLine;

public class CmdLineArguments
{
    public static void ProcessCmdLineArguments(string[] args)
    {
        GlobalVars.RepoPath = Environment.CurrentDirectory;
        GlobalVars.workingArea = Path.Combine(GlobalVars.RepoPath, @"./");
        GlobalVars.head = Path.Combine(GlobalVars.RepoPath, @".git/");
        GlobalVars.path = Path.Combine(GlobalVars.RepoPath, @".git/objects\");
        GlobalVars.branchPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/heads");
        GlobalVars.remoteBranchPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/remotes");

        try
        {
            if (args != null)
            {
                Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {

                    if (o.Web)
                    {
                        GlobalVars.EmitWeb = true;
                    }

                    if (o.Bare)
                    {
                        GlobalVars.head = Path.Combine(GlobalVars.RepoPath, @".\");
                        GlobalVars.path = Path.Combine(GlobalVars.RepoPath, @".\objects\");
                        GlobalVars.branchPath = Path.Combine(GlobalVars.RepoPath, @".\refs\heads");
                        GlobalVars.remoteBranchPath = Path.Combine(GlobalVars.RepoPath, @".\refs\remotes");
                    }

                    if (o.Json != null)
                    {
                        GlobalVars.EmitJsonOnly = true;
                        GlobalVars.EmitWeb = false;

                        GlobalVars.CommitNodesJsonFile = Path.Combine(o.Json, "CommitGitInJson.json");
                        GlobalVars.TreeNodesJsonFile = Path.Combine(o.Json, "TreeGitInJson.json");
                        GlobalVars.BlobNodesJsonFile = Path.Combine(o.Json, "BlobGitInJson.json");
                        GlobalVars.HeadNodesJsonFile = Path.Combine(o.Json, "HeadGitInJson.json");
                        GlobalVars.BranchNodesJsonFile = Path.Combine(o.Json, "BranchGitInJson.json");
                        GlobalVars.IndexFilesJsonFile = Path.Combine(o.Json, "IndexfilesGitInJson.json");
                        GlobalVars.WorkingFilesJsonFile = Path.Combine(o.Json, "WorkingfilesGitInJson.json");
                    }

                    if (o.Neo)
                    {
                        GlobalVars.EmitNeo = true;
                        GlobalVars.EmitJsonOnly = false;
                        GlobalVars.EmitWeb = false;
                        Console.WriteLine($"Neo4J emission enabled");
                    }

                    if (o.Extract)
                    {
                        GlobalVars.PerformTextExtraction = true;
                        Console.WriteLine($"Extraction of file contents will take place");
                    }

                    if (o.Debug)
                    {
                        GlobalVars.debug = true;
                        StandardMessages.DebugModeEnabled();
                    }

                    if (!GlobalVars.debug)
                    {
                        GlobalVars.RepoPath = Environment.CurrentDirectory;
                        DebugMessages.DisplayCurrentDirectory(GlobalVars.RepoPath);

                        // Check if the path to examine the repo of is provided on the command line
                        if (o.RepoPath != null)
                        {
                            GlobalVars.RepoPath = Path.Combine(GlobalVars.RepoPath.Trim(), o.RepoPath.Trim());

                            // Check if path exists
                            if (!Directory.Exists(GlobalVars.RepoPath))
                            {
                                StandardMessages.InvalidRepoPath(GlobalVars.RepoPath);
                                throw new Exception("Invalid RepoPath");
                            }
                            else
                            {
                                StandardMessages.RepoToExamine(GlobalVars.RepoPath);
                            }
                        }
                    }
                    else
                    {
                        if (o.RepoPath == null)
                        {
                            GlobalVars.RepoPath = @"C:\dev\test";
                            StandardMessages.UsingDebugHardCodedPath(GlobalVars.RepoPath);
                        }
                        else
                        {
                            GlobalVars.RepoPath = o.RepoPath;
                            StandardMessages.DebugSelectedAndAlsoRepoPathProvided(GlobalVars.RepoPath);
                        }
                    }


                    if (GlobalVars.debug)
                    {
                        GlobalVars.workingArea = GlobalVars.RepoPath;
                        GlobalVars.head = Path.Combine(GlobalVars.RepoPath, @".git/");
                        GlobalVars.path = Path.Combine(GlobalVars.RepoPath, @".git/objects/");
                        GlobalVars.branchPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/heads");
                        GlobalVars.remoteBranchPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/remotes");
                    }
                    else
                    {
                        GlobalVars.workingArea = GlobalVars.RepoPath;
                        GlobalVars.head = Path.Combine(GlobalVars.RepoPath, @".git/");
                        GlobalVars.path = Path.Combine(GlobalVars.RepoPath, @".git/objects/");
                        GlobalVars.branchPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/heads");
                        GlobalVars.remoteBranchPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/remotes");
                    }

                    if (o.UnpackRefs)
                    {
                        GlobalVars.UnPackRefs = true;
                        Console.WriteLine($"PACK files will be UnPacked");
                    }

                    if (GlobalVars.EmitJsonOnly)
                    {
                        Console.WriteLine($"Json emission enabled");
                    }

                    if (GlobalVars.EmitWeb)
                    {
                        StandardMessages.WebEmissionEnabled();
                    }

                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Warning! Error reading CommandLine arguments");
            if (GlobalVars.debug)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
