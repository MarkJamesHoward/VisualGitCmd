using Neo4j.Driver;

public abstract class GitBranches
{
    public static List<Branch> branches = new List<Branch>();

    public static void CreateBranchObject(string name, string hash)
    {
        Branch b = new Branch
        {
            hash = hash,
            name = name
        };

        if (!branches.Exists(i => i.name == b.name))
        {
            DebugMessages.AddingBranchObject(b.name, b.hash);
            branches.Add(b);
        }
    }

    public static void ProcessBranches(List<string> branchFiles, ISession? session)
    {
        branches = new List<Branch>();

        // Add the Branches
        foreach (var file in branchFiles)
        {
            var branchHash = File.ReadAllText(file);
            if (GlobalVars.EmitNeo)
            {
                Neo4jHelper.AddBranchToNeo(session, Path.GetFileName(file), branchHash);
                Neo4jHelper.CreateBranchLinkNeo(session, Path.GetFileName(file), branchHash.Substring(0, 4));
            }
            CreateBranchObject(Path.GetFileName(file), branchHash.Substring(0, 4));
        }
    }
}
