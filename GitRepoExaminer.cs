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
            Console.WriteLine("Examining file " + file);
            if (file.Contains("pack-") || file.Contains(".idx"))
            {
                break;
            }

            string hashCode_determinedFrom_dir_and_first2charOfFilename = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);

            string fileType = FileType.GetFileType_UsingGitCatFileCmd_Param_T(hashCode_determinedFrom_dir_and_first2charOfFilename, GlobalVars.workingArea);

            DebugMessages.FoundFileOfType(fileType, hashCode_determinedFrom_dir_and_first2charOfFilename);

            if (fileType.Contains("commit"))
            {
                CommitNodeExtraction CommitNodeExtract = new();
                CommitNodeExtract.RunRegExAgainstCommit(hashCode_determinedFrom_dir_and_first2charOfFilename);

                if (CommitNodeExtract.CommitTreeDetails.Success)
                {
                    // Get details of the tree, parent and comment in this commit
                    string treeHash = CommitNodeExtract.CommitTreeDetails.Groups[1].Value;
                    string commitComment = CommitNodeExtract.CommitCommentDetails.Groups[1].Value.Trim();

                    Neo4jHelper.ProcessCommitForNeo4j(commitComment, treeHash, hashCode_determinedFrom_dir_and_first2charOfFilename, CommitNodeExtract);

                    GitTrees.Add(treeHash, hashCode_determinedFrom_dir_and_first2charOfFilename, "Root");

                    List<string> commitParentHashes = CommitNodeExtract.GetParentCommits(hashCode_determinedFrom_dir_and_first2charOfFilename);
                    GitCommits.Add(commitParentHashes, commitComment, hashCode_determinedFrom_dir_and_first2charOfFilename, treeHash);

                    // Now we have a tree we can look at the blobs too and create link from the Tree to Blobs
                    BlobNodeExtraction.ProcessBlobsForSpecifiedTree(treeHash, CommitNodeExtract);
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
            GitRemoteBranches.ProcessRemoteBranches(Neo4jHelper.session);
            GitIndexFiles.ProcessIndexFiles(GlobalVars.workingArea);
            GitWorkingFiles.ProcessWorkingFiles(GlobalVars.workingArea);
            GitBlobs.Add(GlobalVars.GITobjectsPath, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
            HEADNode HEADNodeDetails = HEADNodeExtractionRegEx.GetHeadNodeFromPathAndDetermineWhereItPoints();

            Neo4jHelper.ProcessNeo4jOutput();

            JSONGeneration.ProcessJSONONLYOutput(GitBranches.Branches);

            JSONGeneration.OutputNodesJsonToAPI(firstRun, RandomName.Name, dataID++,
                GitCommits.Commits, GitBlobs.Blobs, GitTrees.Trees, GitBranches.Branches,
                    GitRemoteBranches.RemoteBranches, GitIndexFiles.IndexFiles,
                         GitWorkingFiles.WorkingFiles, HEADNodeDetails);

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

