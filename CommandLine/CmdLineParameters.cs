using CommandLine;
using Yargs;

public class CmdLineArguments
{
    public static void ProcessCmdLineArguments(string[] args)
    {
        GlobalVars.RepoPath = Environment.CurrentDirectory.Trim();
        GlobalVars.workingArea = Path.Combine(GlobalVars.RepoPath, @"./").Trim();
        GlobalVars.headPath = Path.Combine(GlobalVars.RepoPath, @".git/").Trim();
        GlobalVars.GITobjectsPath = Path.Combine(GlobalVars.RepoPath, @".git/objects\").Trim();
        GlobalVars.branchPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/heads").Trim();
        GlobalVars.tagPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/tags");
        GlobalVars.remoteBranchPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/remotes")
            .Trim();

        try
        {
            if (args != null)
            {
                Parser
                    .Default.ParseArguments<Options>(args)
                    .WithParsed<Options>(o =>
                    {
                        if (o.LocalDebugWebsite)
                        {
                            GlobalVars.LocalDebugWebsite = true;
                        }

                        if (o.IsWSL)
                        {
                            GlobalVars.IsWSL = true;
                            StandardMessages.RunningInWSL();
                        }

                        if (o.Api != null)
                        {
                            GlobalVars.Api = o.Api.Trim();
                            StandardMessages.UserSuppliedAPIURL(GlobalVars.Api);
                        }

                        if (o.Web)
                        {
                            GlobalVars.EmitWeb = true;
                        }

                        if (o.SingleRun)
                        {
                            GlobalVars.SingleRun = true;
                        }

                        if (o.LocalDebugAPI)
                        {
                            GlobalVars.LocalDebugAPI = true;
                        }

                        if (o.Bare)
                        {
                            GlobalVars.Bare = true;
                        }

                        if (o.Json != null)
                        {
                            GlobalVars.EmitJsonOnly = true;
                            GlobalVars.EmitWeb = false;

                            GlobalVars.CommitNodesJsonFile = Path.Combine(
                                    o.Json,
                                    "CommitGitInJson.json"
                                )
                                .Trim();
                            GlobalVars.TreeNodesJsonFile = Path.Combine(
                                    o.Json,
                                    "TreeGitInJson.json"
                                )
                                .Trim();
                            GlobalVars.BlobNodesJsonFile = Path.Combine(
                                    o.Json,
                                    "BlobGitInJson.json"
                                )
                                .Trim();
                            GlobalVars.HeadNodesJsonFile = Path.Combine(
                                    o.Json,
                                    "HeadGitInJson.json"
                                )
                                .Trim();
                            GlobalVars.BranchNodesJsonFile = Path.Combine(
                                    o.Json,
                                    "BranchGitInJson.json"
                                )
                                .Trim();
                            GlobalVars.IndexFilesJsonFile = Path.Combine(
                                    o.Json,
                                    "IndexfilesGitInJson.json"
                                )
                                .Trim();
                            GlobalVars.WorkingFilesJsonFile = Path.Combine(
                                    o.Json,
                                    "WorkingfilesGitInJson.json"
                                )
                                .Trim();
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
                                // Expand tilde path for Linux/macOS compatibility
                                string expandedPath = FilePath.ExpandTildePath(o.RepoPath.Trim());

                                GlobalVars.RepoPath = Path.Combine(
                                        GlobalVars.RepoPath.Trim(),
                                        expandedPath
                                    )
                                    .Trim();

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
                                StandardMessages.UsingDebugHardCodedPath(
                                    GlobalVars.RepoPath.Trim()
                                );
                            }
                            else
                            {
                                // Expand tilde path for Linux/macOS compatibility
                                GlobalVars.RepoPath = FilePath.ExpandTildePath(o.RepoPath.Trim());
                                StandardMessages.DebugSelectedAndAlsoRepoPathProvided(
                                    GlobalVars.RepoPath.Trim()
                                );
                            }
                        }

                        if (GlobalVars.Bare)
                        {
                            GlobalVars.workingArea = GlobalVars.RepoPath.Trim();
                            GlobalVars.headPath = Path.Combine(GlobalVars.RepoPath, @".\").Trim();
                            GlobalVars.GITobjectsPath = Path.Combine(
                                    GlobalVars.RepoPath,
                                    @".\objects\"
                                )
                                .Trim();
                            GlobalVars.branchPath = Path.Combine(
                                    GlobalVars.RepoPath,
                                    @".\refs\heads"
                                )
                                .Trim();
                            GlobalVars.tagPath = Path.Combine(
                                GlobalVars.RepoPath,
                                @".git/refs/tags"
                            );
                            GlobalVars.remoteBranchPath = Path.Combine(
                                    GlobalVars.RepoPath,
                                    @".\refs\remotes"
                                )
                                .Trim();
                        }
                        else
                        {
                            GlobalVars.workingArea = GlobalVars.RepoPath.Trim();
                            GlobalVars.headPath = Path.Combine(GlobalVars.RepoPath, @".git/")
                                .Trim();
                            GlobalVars.GITobjectsPath = Path.Combine(
                                    GlobalVars.RepoPath,
                                    @".git/objects/"
                                )
                                .Trim();
                            GlobalVars.branchPath = Path.Combine(
                                    GlobalVars.RepoPath,
                                    @".git/refs/heads"
                                )
                                .Trim();
                            GlobalVars.tagPath = Path.Combine(
                                GlobalVars.RepoPath,
                                @".git/refs/tags"
                            );
                            GlobalVars.remoteBranchPath = Path.Combine(
                                    GlobalVars.RepoPath,
                                    @".git/refs/remotes"
                                )
                                .Trim();
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

            // Update logger with Debug settings if specified in the cmd line arguments
            MyLogging.CreateLogger();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Warning! Error reading CommandLine arguments");
            if (GlobalVars.debug)
            {
                Console.WriteLine(ex.Message);
                var stackTrace = new System.Diagnostics.StackTrace(ex, true);
                var frame = stackTrace.GetFrame(0);
                if (frame != null)
                {
                    Console.WriteLine(
                        $"File: {frame.GetFileName()}, Line: {frame.GetFileLineNumber()}"
                    );
                }
            }
        }
    }
}
