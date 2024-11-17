public class BlobNodeExtraction
{
    public void ProcessBlobsForSpecifiedTree(string treeHash, CommitNodeExtraction CommitNode)
    {
        // Get the details of the Blobs in this Tree
        string tree = FileType.GetContents(CommitNode.commitTreeDetails.Groups[1].Value, GlobalVars.workingArea);

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
            if (GlobalVars.EmitNeo && !FileType.DoesNodeExistAlready(Neo4jHelper.session, blobHash, "blob"))
            {
                if (GlobalVars.EmitNeo)
                    BlobCode.AddBlobToNeo(Neo4jHelper.session, blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, blobContents);
            }
            //Console.WriteLine($"Adding non orphan blob {blobMatch.Groups[1].Value}");

            BlobCode.AddToBlobObjectCollection(treeHash, blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, blobContents);

            if (GlobalVars.EmitNeo && !Links.DoesTreeToBlobLinkExist(Neo4jHelper.session, CommitNode.commitTreeDetails.Groups[1].Value, blobHash))
            {
                if (GlobalVars.EmitNeo)
                    Neo4jHelper.CreateLinkNeo(Neo4jHelper.session, CommitNode.commitTreeDetails.Groups[1].Value, blobMatch.Groups[1].Value, "", "");
            }

            TreeNodesList.CreateTreeToBlobLink(CommitNode.commitTreeDetails.Groups[1].Value, blobMatch.Groups[1].Value);
        }
    }
}