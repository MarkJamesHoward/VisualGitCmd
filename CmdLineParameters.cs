using Yargs;
using CommandLine;
using System.Security.Cryptography.X509Certificates;
using MyProjectl;

namespace MyProject;

public class CmdLineArguments {



public static void ProcessCmdLineArguments(string[] args)
{
        string RepoPath = Environment.CurrentDirectory;
   

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
                        GlobalVars.head = Path.Combine(RepoPath, @".\");
                        GlobalVars.path = Path.Combine(RepoPath, @".\objects\");
                        GlobalVars.branchPath = Path.Combine(RepoPath, @".\refs\heads");
                        GlobalVars.remoteBranchPath = Path.Combine(RepoPath, @".\refs\remotes");
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
                        RepoPath = Environment.CurrentDirectory;
                        Console.WriteLine($"Default RepoPath {RepoPath}");

                        // Check if the path to examine the repo of is provided on the command line
                        if (o.RepoPath != null)
                        {
                           RepoPath = Path.Combine(RepoPath.Trim(), o.RepoPath.Trim());
                            Console.WriteLine($"Combined RepoPath {RepoPath}");
                                
                            // Check if path exists
                            if (!Directory.Exists(RepoPath)) {
                                Console.WriteLine($"Invalid RepoPath-{RepoPath}");
                                throw new Exception("Invalid RepoPath");
                            }
                            else 
                            {
                                Console.WriteLine($"Repo to examine: {RepoPath}");
                            }
                        }
                    }
                    else
                    {
                        RepoPath = @"C:\dev\test";
                        Console.WriteLine($"Debug: Using {RepoPath}");
                    }


                    if (debug)
                    {
                        workingArea = RepoPath;
                        head = Path.Combine(RepoPath, @".git/");
                        path = Path.Combine(RepoPath, @".git/objects/");
                        branchPath = Path.Combine(RepoPath, @".git/refs/heads");
                        remoteBranchPath = Path.Combine(RepoPath, @".git/refs/remotes");
                    }
                    else
                    {
                        workingArea = RepoPath;
                        head = Path.Combine(RepoPath, @".git/");
                        path = Path.Combine(RepoPath, @".git/objects/");
                        branchPath = Path.Combine(RepoPath, @".git/refs/heads");
                        remoteBranchPath = Path.Combine(RepoPath, @".git/refs/remotes");
                    }

                    if (o.UnpackRefs)
                    {
                        UnPackRefs = true;
                        Console.WriteLine($"PACK files will be UnPacked");
                    }

                    if (EmitJsonOnly)
                    {
                        Console.WriteLine($"Json emission enabled");
                    }

                    if (EmitWeb)
                    {
                        Console.WriteLine($"Web emission enabled");
                    }

                });
            }
        }
        catch(Exception ex) {
            Console.WriteLine("Warning! Error reading CommandLine arguments");
            if (debug) {
                Console.WriteLine(ex.Message);
            }
        }
}