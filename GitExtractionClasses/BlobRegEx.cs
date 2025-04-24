public abstract class BlobNodeExtraction
{
    public static void ProcessBlobsForSpecifiedTree(string treeHash, CommitNodeExtraction CommitNode)
    {
        // Get the details of the Blobs in this Tree
        string tree = FileType.GetContents(CommitNode.CommitTreeDetails.Groups[1].Value, GlobalVars.workingArea);

        var blobsInTree = Regex.Matches(tree, @"blob ([0-9a-f]{4})[0-9a-f]{36}.([\w\.]+)");

        foreach (Match blobMatch in blobsInTree)
        {
            string blobHash = blobMatch.Groups[1].Value;
            string blobContents = string.Empty;

            FileType.GetContents(blobHash, GlobalVars.workingArea);

            //Console.WriteLine($"\t\t-> blob {blobHash} {blobMatch.Groups[2]}");
            if (GlobalVars.EmitNeo && !FileType.DoesNeo4jNodeExistAlready(Neo4jHelper.session, blobHash, "blob"))
            {
                Neo4jHelper.AddBlobToNeo(Neo4jHelper.session, blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, blobContents);
            }

            DebugMessages.GenericMessage("Adding BLOB from ProcessBlobsForSpecifiedTree treeHash:" + treeHash);
            GitBlobs.Add(treeHash, blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, blobContents);

            if (GlobalVars.EmitNeo && !Neo4jHelper.DoesTreeToBlobLinkExist(Neo4jHelper.session, CommitNode.CommitTreeDetails.Groups[1].Value, blobHash))
            {
                if (GlobalVars.EmitNeo)
                    Neo4jHelper.CreateLinkNeo(Neo4jHelper.session, CommitNode.CommitTreeDetails.Groups[1].Value, blobMatch.Groups[1].Value, "", "");
            }

            GitTrees.CreateTreeToBlobLink(CommitNode.CommitTreeDetails.Groups[1].Value, blobMatch.Groups[1].Value);
        }
        // Any SubTrees in this Tree?
        var TreesInTree = Regex.Matches(tree, @"tree ([0-9a-f]{4})[0-9a-f]{36}.([\w\.]+)");
        foreach (Match treeMatch in TreesInTree)
        {
            string subtreeHash = treeMatch.Groups[1].Value;
            string subTreeFolderName = treeMatch.Groups[2].Value;
            BlobNodeExtraction.ProcessSubTeeesForSpecifiedTree(treeHash, subtreeHash, subTreeFolderName);
        }
    }


    public static void ProcessSubTeeesForSpecifiedTree(string parentTreeHash, string subTreeHash, string folderName)
    {
        GitTrees.Add(subTreeHash, parentTreeHash, folderName);

        // Get the details of the Blobs in this Tree
        string tree = FileType.GetContents(subTreeHash, GlobalVars.workingArea);

        var blobsInTree = Regex.Matches(tree, @"blob ([0-9a-f]{4})[0-9a-f]{36}.([\w\.]+)");

        foreach (Match blobMatch in blobsInTree)
        {
            string blobHash = blobMatch.Groups[1].Value;
            string blobContents = string.Empty;

            FileType.GetContents(blobHash, GlobalVars.workingArea);

            DebugMessages.GenericMessage("Adding BLOB from ProcessSubTeeesForSpecifiedTree");
            GitBlobs.Add(subTreeHash, blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, blobContents);

            GitTrees.CreateTreeToBlobLink(subTreeHash, blobMatch.Groups[1].Value);
        }
        // Any SubTrees in this Tree?
        var TreesInTree = Regex.Matches(tree, @"tree ([0-9a-f]{4})[0-9a-f]{36}.([\w\.]+)");
        foreach (Match treeMatch in TreesInTree)
        {
            string nextSubTreeHash = treeMatch.Groups[1].Value;
            string subTreeFolderName = treeMatch.Groups[2].Value;
            ProcessSubTeeesForSpecifiedTree(subTreeHash, nextSubTreeHash, subTreeFolderName);
        }
    }
}