public abstract class GitTrees
{
    public static List<Tree> Trees = new List<Tree>();

    public static void Add(string treeHash, string parentHash, string folderName)
    {
        Tree tn = new Tree();
        tn.hash = treeHash;
        tn.blobs = new List<string>();
        tn.text = folderName;
        tn.parents = new List<string>();
        tn.parents.Add(parentHash);

        if (!Trees.Exists(i => i.hash == tn.hash))
        {
            Trees.Add(tn);
        }
        else
        {
            var existingTree = Trees.Find(t => t.hash == tn.hash);

            if (existingTree?.parents?.Contains(parentHash) == false)
            {
                existingTree.parents.Add(parentHash);
            }

        }
    }

    public static void CreateTreeToBlobLink(string parent, string child)
    {
        var treeNode = Trees?.Find(i => i.hash == parent);
        treeNode?.blobs?.Add(child);
    }

}