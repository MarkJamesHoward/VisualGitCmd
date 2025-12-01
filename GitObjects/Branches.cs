using Neo4j.Driver;

public abstract class GitBranches
{
    public static List<Branch> Branches = new List<Branch>();

    /// <summary>
    /// Creates a new branch object and adds it to the Branches collection if it doesn't already exist.
    /// </summary>
    /// <param name="name">The name of the branch to create.</param>
    /// <param name="hash">The commit hash associated with the branch.</param>
    /// <remarks>
    /// This method checks if a branch with the specified name already exists in the Branches collection.
    /// If the branch does not exist, it creates a new Branch object and adds it to the collection.
    /// A debug message is logged when a new branch object is added.
    /// </remarks>
    public static void CreateBranchObject(string name, string hash)
    {
        Branch b = new Branch { hash = hash, name = name };

        if (!Branches.Exists(i => i.name == b.name))
        {
            DebugMessages.AddingBranchObject(b.name, b.hash);
            Branches.Add(b);
        }
    }

    public static void ProcessBranches(ISession? session)
    {
        Branches = new List<Branch>();

        List<string> branchFiles = Directory.GetFiles(GlobalVars.branchPath).ToList();

        // Add the Branches
        foreach (var file in branchFiles)
        {
            var branchHash = File.ReadAllText(file);
            if (GlobalVars.EmitNeo)
            {
                Neo4jHelper.AddBranchToNeo(session, Path.GetFileName(file), branchHash);
                Neo4jHelper.CreateBranchLinkNeo(
                    session,
                    Path.GetFileName(file),
                    branchHash.Substring(0, 4)
                );
            }
            CreateBranchObject(Path.GetFileName(file), branchHash.Substring(0, 4));
        }
    }
}
