public abstract class GitTrees
{
    public static List<Tree> Trees = new List<Tree>();

    public static void Add(string treeHash)
    {
        Tree tn = new Tree();
        tn.hash = treeHash;
        tn.blobs = new List<string>();

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

    public static void CreateSubTreeToTreeLink(string parent, string child)
    {
        var treeNode = Trees?.Find(i => i.hash == parent);
        treeNode?.subTrees?.Add(child);
    }
}