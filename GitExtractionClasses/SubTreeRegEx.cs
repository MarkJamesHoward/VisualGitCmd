public abstract class SubTreeExtraction
{
    public static void ProcessSubTreesForSpecifiedTree(string treeHash, CommitNodeExtraction CommitNode)
    {
        // Get the details of the Trees in this Tree
        string tree = FileType.GetContents(CommitNode.CommitTreeDetails.Groups[1].Value, GlobalVars.workingArea);

        var TreesInTree = Regex.Matches(tree, @"blob ([0-9a-f]{4})[0-9a-f]{36}.([\w\.]+)");

        foreach (Match treeMatch in TreesInTree)
        {
            string blobHash = treeMatch.Groups[1].Value;
            string blobContents = string.Empty;

            FileType.GetContents(blobHash, GlobalVars.workingArea);

            GitTrees.CreateTreeToBlobLink(CommitNode.CommitTreeDetails.Groups[1].Value, treeMatch.Groups[1].Value);
        }
    }
}