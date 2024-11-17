using Neo4j.Driver;
public abstract class GitRepoExaminer
{
    #region StaticVariables
    static bool firstRun = true;
    static int dataID = 1;

    #endregion

    public static void ProcessEachFileAndExtract_Commit_Tree_Blob(string dir)
    {
        foreach (string file in Directory.GetFiles(dir).ToList())
        {
            if (file.Contains("pack-") || file.Contains(".idx"))
            {
                break;
            }

            string hashCode_determinedFrom_dir_and_first2charOfFilename = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);

            string fileType = FileType.GetFileType_UsingGitCatFileCmd_Param_T(hashCode_determinedFrom_dir_and_first2charOfFilename, GlobalVars.workingArea);

            DebugMessages.FoundFileOfType(fileType, hashCode_determinedFrom_dir_and_first2charOfFilename);

            if (fileType.Contains("commit"))
            {
                CommitNodeExtraction CommitNode = new(hashCode_determinedFrom_dir_and_first2charOfFilename);

                if (CommitNode.commitTreeDetails.Success)
                {
                    // Get details of the tree, parent and comment in this commit
                    string treeHash = CommitNode.commitTreeDetails.Groups[1].Value;
                    string commitComment = CommitNode.commitCommentDetails.Groups[1].Value.Trim();

                    List<string> commitParentHashes = new List<string>();

                    foreach (Match commitParentMatch in CommitNode.commitParentDetails)
                    {
                        string parentHash = commitParentMatch.Groups[1].Value;
                        commitParentHashes.Add(parentHash);
                        StandardMessages.ParentCommitHashCode(hashCode_determinedFrom_dir_and_first2charOfFilename, parentHash);
                    }

                    Neo4jHelper.ProcessCommitForNeo4j(commitComment, treeHash, hashCode_determinedFrom_dir_and_first2charOfFilename, CommitNode);

                    TreeNodesList.AddTreeObjectToTreeNodeList(treeHash, FileType.GetContents(treeHash, GlobalVars.workingArea));
                    CommitNodesList.AddCommitObjectToCommitNodeList(commitParentHashes, commitComment, hashCode_determinedFrom_dir_and_first2charOfFilename, treeHash, CommitNode.commitContents);

                    // Now we have a tree we can look at the blobs too and create link from the Tree to Blobs
                    BlobNodeExtraction BlobNode = new();
                    BlobNode.ProcessBlob(treeHash, CommitNode);
                }
            }
        }
    }
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
                ProcessEachFileAndExtract_Commit_Tree_Blob(dir);
            }

            GitBranches.ProcessBranches(Neo4jHelper.session);
            RemoteBranches.ProcessRemoteBranches(Neo4jHelper.session);

            HEAD HEADNodeDetails = HEADNode.GetHeadNodeFromPath();

            Neo4jHelper.ProcessNeo4jOutput();
            JSONGeneration.ProcessJSONONLYOutput(GitBranches.branches);

            if (GlobalVars.EmitWeb)
            {
                BlobCode.FindOrphanBlobs(GlobalVars.GITobjectsPath, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);

                JSONGeneration.OutputNodesJsonToAPI(firstRun, RandomName.Name, dataID++,
                    CommitNodesList.CommitNodes, BlobCode.Blobs, TreeNodesList.TreeNodes, GitBranches.branches,
                        RemoteBranches.remoteBranches, JSONGeneration.IndexFilesJsonNodes(GlobalVars.workingArea),
                             Nodes.WorkingFilesNodes(GlobalVars.workingArea), HEADNodeDetails);
            }

            // Only run this on the first run
            Browser.OpenBrowser(ref firstRun);
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

