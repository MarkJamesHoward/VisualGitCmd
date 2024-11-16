using Neo4j.Driver;
public abstract class GitRepoExaminer
{
    #region StaticVariables
    static string name = RandomName.randomNameGenerator.GenerateRandomPlaceName();
    static bool firstRun = true;
    static int dataID = 1;
    static string password = "";
    static string uri = "";
    static string username = "";
    static List<string> HashCodeFilenames = new List<string>();
    #endregion

    public static void Run()
    {

        List<TreeNode> TreeNodes = new List<TreeNode>();
        List<Branch> branches = new List<Branch>();
        List<Branch> remoteBranches = new List<Branch>();

        HEAD HEAD = new HEAD();

        // Get all the files in the .git/objects folder
        try
        {
            List<string> remoteBranchFiles = new List<string>();
            List<string> branchFiles = new List<string>();

            branchFiles = Directory.GetFiles(GlobalVars.branchPath).ToList();
            RemoteBranches.GetRemoteBranches(ref remoteBranchFiles);

            List<string> directories = Directory.GetDirectories(GlobalVars.GITobjectsPath).ToList();
            List<string> files = new List<string>();

            IDriver _driver;
            ISession? session = null;

            if (GlobalVars.EmitNeo)
            {
                _driver = Neo4jHelper.GetDriver(uri, username, password);
                session = _driver.Session();
                Neo4jHelper.ClearExistingNodesInNeo(session);
            }


            foreach (string dir in directories)
            {
                if (dir.Contains("pack") || dir.Contains("info"))
                {
                    break;
                }

                files = Directory.GetFiles(dir).ToList();

                foreach (string file in files)
                {
                    if (file.Contains("pack-") || file.Contains(".idx"))
                    {
                        break;
                    }

                    string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);

                    HashCodeFilenames.Add(hashCode);

                    string fileType = FileType.GetFileType(hashCode, GlobalVars.workingArea);

                    //Console.WriteLine($"{fileType.TrimEnd('\n', '\r')} {hashCode}");

                    if (fileType.Contains("commit"))
                    {
                        string commitContents;
                        commitContents = FileType.GetContents(hashCode, GlobalVars.workingArea);

                        var match = Regex.Match(commitContents, "tree ([0-9a-f]{4})");
                        var commitParents = Regex.Matches(commitContents, "parent ([0-9a-f]{4})");
                        var commitComment = Regex.Match(commitContents, "\n\n(.+)\n");

                        if (match.Success)
                        {
                            // Get details of the tree,parent and comment in this commit
                            string treeHash = match.Groups[1].Value;
                            //Console.WriteLine($"\t-> tree {treeHash}");

                            List<string> commitParentHashes = new List<string>();

                            foreach (Match commitParentMatch in commitParents)
                            {
                                string parentHash = commitParentMatch.Groups[1].Value;
                                commitParentHashes.Add(parentHash);
                                StandardMessages.ParentCommitHashCode(hashCode, parentHash);
                            }

                            string comment = commitComment.Groups[1].Value;
                            comment = comment.Trim();

                            if (GlobalVars.EmitNeo)
                            {
                                Neo4jHelper.AddCommitToNeo(session, comment, hashCode, commitContents);
                            }

                            if (GlobalVars.EmitNeo && !FileType.DoesNodeExistAlready(session, treeHash, "tree"))
                            {
                                if (GlobalVars.EmitNeo)
                                    Neo4jHelper.AddTreeToNeo(session, treeHash, FileType.GetContents(treeHash, GlobalVars.workingArea));
                            }

                            JSONGeneration.CreateTreeJson(treeHash, FileType.GetContents(treeHash, GlobalVars.workingArea), TreeNodes);
                            Commits.AddCommitObjectToCommitNodeList(commitParentHashes, comment, hashCode, treeHash, commitContents);

                            if (GlobalVars.EmitNeo)
                            {
                                Neo4jHelper.CreateCommitLinkNeo(session, hashCode, treeHash, "", "");
                            }

                            // Get the details of the BlobCode.Blobs in this Tree
                            string tree = FileType.GetContents(match.Groups[1].Value, GlobalVars.workingArea);
                            var blobsInTree = Regex.Matches(tree, @"blob ([0-9a-f]{4})[0-9a-f]{36}.([\w\.]+)");

                            foreach (Match blobMatch in blobsInTree)
                            {
                                string blobHash = blobMatch.Groups[1].Value;
                                string blobContents = string.Empty;

                                if (GlobalVars.PerformTextExtraction)
                                {
                                    FileType.GetContents(blobHash, GlobalVars.workingArea);
                                }

                                //Console.WriteLine($"\t\t-> blob {blobHash} {blobMatch.Groups[2]}");
                                if (GlobalVars.EmitNeo && !FileType.DoesNodeExistAlready(session, blobHash, "blob"))
                                {
                                    if (GlobalVars.EmitNeo)
                                        BlobCode.AddBlobToNeo(session, blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, blobContents);
                                }
                                //Console.WriteLine($"Adding non orphan blob {blobMatch.Groups[1].Value}");

                                BlobCode.AddToBlobObjectCollection(treeHash, blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, blobContents);

                                if (GlobalVars.EmitNeo && !Links.DoesTreeToBlobLinkExist(session, match.Groups[1].Value, blobHash))
                                {
                                    if (GlobalVars.EmitNeo)
                                        Neo4jHelper.CreateLinkNeo(session, match.Groups[1].Value, blobMatch.Groups[1].Value, "", "");
                                }

                                JSONGeneration.CreateTreeToBlobLinkJson(match.Groups[1].Value, blobMatch.Groups[1].Value, TreeNodes);
                            }
                        }
                        else
                        {
                            //Console.WriteLine("No Tree found in Commit");
                        }
                    }
                }

            }

            GitBranches.ProcessBranches(branchFiles, session, ref branches);
            RemoteBranches.ProcessRemoteBranches(remoteBranchFiles, session, ref remoteBranches);

            if (GlobalVars.EmitNeo)
            {
                Links.AddCommitParentLinks(session, GlobalVars.GITobjectsPath, GlobalVars.workingArea);
                BlobCode.AddOrphanBlobs(session, GlobalVars.branchPath, GlobalVars.GITobjectsPath, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
                Nodes.GetHEAD(session, GlobalVars.head);
            }


            if (GlobalVars.EmitJsonOnly)
            {
                BlobCode.FindBlobs(GlobalVars.GITobjectsPath, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
                JSONGeneration.OutputNodesJson(Commits.CommitNodesList, GlobalVars.CommitNodesJsonFile);
                JSONGeneration.OutputNodesJson(TreeNodes, GlobalVars.TreeNodesJsonFile);
                JSONGeneration.OutputNodesJson(BlobCode.Blobs, GlobalVars.BlobNodesJsonFile);
                JSONGeneration.OutputHEADJson(HEAD, GlobalVars.HeadNodesJsonFile, GlobalVars.head);
                JSONGeneration.OutputBranchJson(branches, TreeNodes, BlobCode.Blobs, GlobalVars.BranchNodesJsonFile);
                JSONGeneration.OutputIndexFilesJson(GlobalVars.IndexFilesJsonFile);
                JSONGeneration.OutputWorkingFilesJson(GlobalVars.workingArea, GlobalVars.WorkingFilesJsonFile);
            }

            if (GlobalVars.EmitWeb)
            {
                BlobCode.FindBlobs(GlobalVars.GITobjectsPath, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
                JSONGeneration.OutputNodesJsonToAPI(firstRun, name, dataID++, Commits.CommitNodesList, BlobCode.Blobs, TreeNodes, branches, remoteBranches, JSONGeneration.IndexFilesJsonNodes(GlobalVars.workingArea), Nodes.WorkingFilesNodes(GlobalVars.workingArea), Nodes.HEADNodes(GlobalVars.head));
            }

            // Only run this on the first run
            if (firstRun)
            {
                firstRun = false;
                Process.Start(new ProcessStartInfo($"https://visualgit.net/visualize?data={name.Replace(' ', 'x')}/1") { UseShellExecute = true });
            }
        }
        catch (Exception e)
        {
            if (e.Message.Contains($"Could not find a part of the GlobalVars.path"))
            {
                Console.WriteLine("Waiting for Git to be initiased in this folder...");

                if (GlobalVars.debug)
                {
                    Console.WriteLine($"Details: {e.Message}");
                }
                else
                {
                    Console.WriteLine($"Details: {e.Message}");

                }
            }
            else
            {
                Console.WriteLine($"Error while getting files in {GlobalVars.GITobjectsPath} {e.Message} {e}");
            }
        }
    }
}

