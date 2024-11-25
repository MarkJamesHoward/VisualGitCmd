public abstract class GitTrees
{
    public static List<Tree> Trees = new List<Tree>();

    public static void Add(string treeHash, string parentHash, string folderName)
    {
        Tree tn = new Tree();
        tn.hash = treeHash;
        tn.blobs = new List<string>();
        tn.parent = parentHash;
        tn.text = folderName;

        if (!Trees.Exists(i => i.hash == tn.hash))
        {
            Trees.Add(tn);
        }
    }

    public static void CreateTreeToBlobLink(string parent, string child)
    {
        var treeNode = Trees?.Find(i => i.hash == parent);
        treeNode?.blobs?.Add(child);
    }

    public static void CreateSubTreeToParentTreeLink(string parent, string child)
    {
        var treeNode = Trees?.Find(i => i.hash == child);
        if (treeNode != null)
        {
            treeNode.parent = parent;
        }
    }
}