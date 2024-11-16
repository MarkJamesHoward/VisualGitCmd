public abstract class Commits
{
    public static List<CommitNode> CommitNodesList = new List<CommitNode>();

    public static void AddCommitObjectToCommitNodeList(List<string> parentCommitHash, string comment, string hash, string treeHash, string contents)
    {
        CommitNode n = new CommitNode();
        n.text = comment;
        n.hash = hash;
        n.parent = parentCommitHash;
        n.tree = treeHash;

        if (!CommitNodesList.Exists(i => i.hash == n.hash))
        {
            DebugMessages.AddedNewCommitObjectToCommitNodesList(n.hash, n.text);
            CommitNodesList.Add(n);
        }
    }
}