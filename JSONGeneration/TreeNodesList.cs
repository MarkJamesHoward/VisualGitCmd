public abstract class TreeNodesList
{
    public static List<TreeNode> TreeNodes = new List<TreeNode>();

    public static void AddTreeObjectToTreeNodeList(string treeHash, string contents)
    {
        TreeNode tn = new TreeNode();
        tn.hash = treeHash;
        tn.blobs = new List<string>();

        if (!TreeNodes.Exists(i => i.hash == tn.hash))
        {
            TreeNodes.Add(tn);
        }
    }

    public static void CreateTreeToBlobLinkJson(string parent, string child)
    {
        var treeNode = TreeNodes?.Find(i => i.hash == parent);
        treeNode?.blobs?.Add(child);
    }
}