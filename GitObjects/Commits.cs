public abstract class GitCommits
{
    public static List<Commit> Commits = new List<Commit>();

    public static void AddCommitObjectToCommitNodeList(List<string> parentCommitHash, string comment, string hash, string treeHash)
    {
        Commit n = new Commit();
        n.text = comment;
        n.hash = hash;
        n.parent = parentCommitHash;
        n.tree = treeHash;

        if (!Commits.Exists(i => i.hash == n.hash))
        {
            DebugMessages.AddedNewCommitObjectToCommitNodesList(n.hash, n.text);
            Commits.Add(n);
        }
    }
}