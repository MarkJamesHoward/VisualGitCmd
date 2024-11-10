using Yargs;
using CommandLine;
using System.Security.Cryptography.X509Certificates;
using MyProjectl;

namespace MyProject
{
    public class CmdLineArguments {

    public static void ProcessCmdLineArguments(string[] args)
    {
            GlobalVars.RepoPath = Environment.CurrentDirectory;
          //Console.WriteLine($"Current folder is {GlobalVars.RepoPath}");
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
                            Console.WriteLine($"Debug mode enabled");
                        }

                        if (!GlobalVars.debug)
                        {
                            GlobalVars.RepoPath = Environment.CurrentDirectory;
                            Console.WriteLine($"Default GlobalVars.RepoPath {GlobalVars.RepoPath}");

                            // Check if the path to examine the repo of is provided on the command line
                            if (o.RepoPath != null)
                            {
                               GlobalVars.RepoPath = Path.Combine(GlobalVars.RepoPath.Trim(), o.RepoPath.Trim());
                                Console.WriteLine($"Combined GlobalVars.RepoPath {GlobalVars.RepoPath}");
                                
                                // Check if path exists
                                if (!Directory.Exists(GlobalVars.RepoPath)) {
                                    Console.WriteLine($"Invalid GlobalVars.RepoPath-{GlobalVars.RepoPath}");
                                    throw new Exception("Invalid GlobalVars.RepoPath");
                                }
                                else 
                                {
                                    Console.WriteLine($"Repo to examine: {GlobalVars.RepoPath}");
                                }
                            }
                        }
                        else
                        {
                            GlobalVars.RepoPath = @"C:\dev\test";
                            Console.WriteLine($"Debug: Using {GlobalVars.RepoPath}");
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
                            Console.WriteLine($"Web emission enabled");
                        }

                    });
                }
            }
            catch(Exception ex) {
                Console.WriteLine("Warning! Error reading CommandLine arguments");
                if (GlobalVars.debug) {
                    Console.WriteLine(ex.Message);
                }
            }
    }
    }
}