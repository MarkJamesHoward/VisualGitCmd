public abstract class GitRepoExaminer
{
    #region StaticVariables
    static bool firstRun = true;
    static int dataID = 1;

    #endregion

    public static void ProcessEachFileAndExtract_Commit_Tree_Blob(string dir)
    {
        // Console.WriteLine("ProcessEachFileAndExtract_Commit_Tree_Blob Starting " + dir);
        foreach (string file in Directory.GetFiles(dir).ToList())
        {
            DebugMessages.GenericMessage("Examining file " + file);
            if (file.Contains("pack-") || file.Contains(".idx"))
            {
                continue;
            }

            string hashCode_determinedFrom_dir_and_first2charOfFilename =
                Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);

            string fileType = FileType.GetFileType_UsingGitCatFileCmd_Param_T(
                hashCode_determinedFrom_dir_and_first2charOfFilename,
                GlobalVars.workingArea
            );

            DebugMessages.FoundFileOfType(
                fileType,
                hashCode_determinedFrom_dir_and_first2charOfFilename
            );

            if (fileType.Contains("commit"))
            {
                CommitNodeExtraction CommitNodeExtract = new();
                CommitNodeExtract.RunRegExAgainstCommit(
                    hashCode_determinedFrom_dir_and_first2charOfFilename
                );

                if (CommitNodeExtract.CommitTreeDetails.Success)
                {
                    // Get details of the tree, parent and comment in this commit
                    string treeHash = CommitNodeExtract.CommitTreeDetails.Groups[1].Value;
                    string commitComment = CommitNodeExtract
                        .CommitCommentDetails.Groups[1]
                        .Value.Trim();

                    Neo4jHelper.ProcessCommitForNeo4j(
                        commitComment,
                        treeHash,
                        hashCode_determinedFrom_dir_and_first2charOfFilename,
                        CommitNodeExtract
                    );

                    GitTrees.Add(
                        treeHash,
                        hashCode_determinedFrom_dir_and_first2charOfFilename,
                        "Root"
                    );

                    List<string> commitParentHashes = CommitNodeExtract.GetParentCommits(
                        hashCode_determinedFrom_dir_and_first2charOfFilename
                    );
                    GitCommits.Add(
                        commitParentHashes,
                        commitComment,
                        hashCode_determinedFrom_dir_and_first2charOfFilename,
                        treeHash
                    );

                    // Now we have a tree we can look at the blobs too and create link from the Tree to Blobs
                    BlobNodeExtraction.ProcessBlobsForSpecifiedTree(treeHash, CommitNodeExtract);
                }
            }
        }
        // Console.WriteLine("ProcessEachFileAndExtract_Commit_Tree_Blob Completed " + dir);
    }

    public static void Run()
    {
        // Get all the files in the .git/objects folder
        try
        {
            Neo4jHelper.CheckIfNeoj4EmissionEnabled();

            // Console.WriteLine("RUN Starting " + GlobalVars.GITobjectsPath);
            //
            GitCommits.Commits.Clear();
            GitBlobs.Blobs.Clear();
            GitBranches.Branches.Clear();
            GitTrees.Trees.Clear();

            foreach (
                string dir in Directory.GetDirectories(GlobalVars.GITobjectsPath.Trim()).ToList()
            )
            {
                if (dir.Contains("pack") || dir.Contains("info"))
                {
                    DebugMessages.IgnoreDirectory(dir);
                    continue;
                }
                // Console.WriteLine("top level call ProcessEachFileAndExtract_Commit_Tree_Blob " + GlobalVars.GITobjectsPath);
                ProcessEachFileAndExtract_Commit_Tree_Blob(dir);
            }
            // Console.WriteLine("RUN Completed " + GlobalVars.GITobjectsPath);

            GitBranches.ProcessBranches(Neo4jHelper.session);
            GitTags.ProcessTags(Neo4jHelper.session);
            GitRemoteBranches.ProcessRemoteBranches(Neo4jHelper.session);
            GitIndexFiles.ProcessIndexFiles(GlobalVars.workingArea);

            // If this is a Bare Repo then we'll not have a working area
            if (!GlobalVars.Bare)
            {
                GitWorkingFiles.ProcessWorkingFiles(GlobalVars.workingArea);
            }

            GitBlobs.Add(
                GlobalVars.GITobjectsPath,
                GlobalVars.workingArea,
                GlobalVars.PerformTextExtraction
            );
            HEADNode HEADNodeDetails =
                HEADNodeExtractionRegEx.GetHeadNodeFromPathAndDetermineWhereItPoints();

            Neo4jHelper.ProcessNeo4jOutput();

            JSONGeneration.ProcessJSONONLYOutput(GitBranches.Branches);

            JSONGeneration.OutputNodesJsonToAPI(
                firstRun,
                RandomName.Name,
                dataID++,
                GitCommits.Commits,
                GitBlobs.Blobs,
                GitTrees.Trees,
                GitBranches.Branches,
                GitRemoteBranches.RemoteBranches,
                GitTags.Tags,
                GitIndexFiles.IndexFiles,
                GitWorkingFiles.WorkingFiles,
                HEADNodeDetails
            );

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
                    var stackTrace = new System.Diagnostics.StackTrace(e, true);
                    var frame = stackTrace.GetFrame(0);
                    if (frame != null)
                    {
                        Console.WriteLine(
                            $"File: {frame.GetFileName()}, Line: {frame.GetFileLineNumber()}"
                        );
                    }
                }
                else
                {
                    Console.WriteLine($"Details: {e.Message}");
                }
            }
            else
            {
                if (GlobalVars.debug)
                {
                    Console.WriteLine(
                        $"Error while getting files in {GlobalVars.GITobjectsPath} {e.Message} {e}"
                    );
                    var stackTrace = new System.Diagnostics.StackTrace(e, true);
                    var frame = stackTrace.GetFrame(0);
                    if (frame != null)
                    {
                        Console.WriteLine(
                            $"File: {frame.GetFileName()}, Line: {frame.GetFileLineNumber()}"
                        );
                    }
                }
                else
                {
                    Console.WriteLine(
                        $"Error while getting files in {GlobalVars.GITobjectsPath} {e.Message} {e}"
                    );
                }
            }
        }
    }
}
