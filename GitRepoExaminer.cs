using Neo4j.Driver;
public abstract class GitRepoExaminer
{
    #region StaticVariables
    static bool firstRun = true;
    static int dataID = 1;

    #endregion

    public static void Run()
    {
        // Get all the files in the .git/objects folder
        try
        {
            Neo4jHelper.CheckIfNeoj4EmissionEnabled();

            foreach (string dir in Directory.GetDirectories(GlobalVars.GITobjectsPath).ToList())
            {
                if (dir.Contains("pack") || dir.Contains("info"))
                {
                    break;
                }

                foreach (string file in Directory.GetFiles(dir).ToList())
                {
                    if (file.Contains("pack-") || file.Contains(".idx"))
                    {
                        break;
                    }

                    string hashCode_determinedFrom_dir_and_first2charOfFilename = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);

                    string fileType = FileType.GetFileType(hashCode_determinedFrom_dir_and_first2charOfFilename, GlobalVars.workingArea);

                    //Console.WriteLine($"{fileType.TrimEnd('\n', '\r')} {hashCode}");

                    if (fileType.Contains("commit"))
                    {
                        CommitNodeExtraction CommitNode = new(hashCode_determinedFrom_dir_and_first2charOfFilename);

                        if (CommitNode.commitTreeDetails.Success)
                        {
                            // Get details of the tree,parent and comment in this commit
                            string treeHash = CommitNode.commitTreeDetails.Groups[1].Value;

                            List<string> commitParentHashes = new List<string>();

                            foreach (Match commitParentMatch in CommitNode.commitParentDetails)
                            {
                                string parentHash = commitParentMatch.Groups[1].Value;
                                commitParentHashes.Add(parentHash);
                                StandardMessages.ParentCommitHashCode(hashCode_determinedFrom_dir_and_first2charOfFilename, parentHash);
                            }

                            string comment = CommitNode.commitCommentDetails.Groups[1].Value;
                            comment = comment.Trim();

                            if (GlobalVars.EmitNeo)
                            {
                                Neo4jHelper.AddCommitToNeo(Neo4jHelper.session, comment, hashCode_determinedFrom_dir_and_first2charOfFilename, CommitNode.commitContents);
                            }

                            if (GlobalVars.EmitNeo && !FileType.DoesNodeExistAlready(Neo4jHelper.session, treeHash, "tree"))
                            {
                                if (GlobalVars.EmitNeo)
                                    Neo4jHelper.AddTreeToNeo(Neo4jHelper.session, treeHash, FileType.GetContents(treeHash, GlobalVars.workingArea));
                            }

                            TreeNodesList.AddTreeObjectToTreeNodeList(treeHash, FileType.GetContents(treeHash, GlobalVars.workingArea));
                            CommitNodesList.AddCommitObjectToCommitNodeList(commitParentHashes, comment, hashCode_determinedFrom_dir_and_first2charOfFilename, treeHash, CommitNode.commitContents);

                            if (GlobalVars.EmitNeo)
                            {
                                Neo4jHelper.CreateCommitLinkNeo(Neo4jHelper.session, hashCode_determinedFrom_dir_and_first2charOfFilename, treeHash, "", "");
                            }

                            BlobNodeExtraction BlobNode = new();
                            BlobNode.ProcessBlob(treeHash, CommitNode);
                        }
                        else
                        {
                            //Console.WriteLine("No Tree found in Commit");
                        }
                    }
                }

            }

            GitBranches.ProcessBranches(Neo4jHelper.session);
            RemoteBranches.ProcessRemoteBranches(Neo4jHelper.session);

            Neo4jHelper.ProcessNeo4jOutput();
            JSONGeneration.ProcessJSONONLYOutput(GitBranches.branches);

            if (GlobalVars.EmitWeb)
            {
                BlobCode.FindBlobs(GlobalVars.GITobjectsPath, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);

                JSONGeneration.OutputNodesJsonToAPI(firstRun, RandomName.Name, dataID++,
                    CommitNodesList.CommitNodes, BlobCode.Blobs, TreeNodesList.TreeNodes, GitBranches.branches,
                        RemoteBranches.remoteBranches, JSONGeneration.IndexFilesJsonNodes(GlobalVars.workingArea),
                             Nodes.WorkingFilesNodes(GlobalVars.workingArea), HEADNode.GetHeadNodeFromPath());
            }

            // Only run this on the first run
            if (firstRun)
            {
                firstRun = false;
                Process.Start(new ProcessStartInfo($"https://visualgit.net/visualize?data={RandomName.Name.Replace(' ', 'x')}/1") { UseShellExecute = true });
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

