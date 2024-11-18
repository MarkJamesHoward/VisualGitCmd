using Neo4j.Driver;
public abstract class RemoteBranches
{
    public static List<Branch> remoteBranches = new List<Branch>();

    public static void ProcessRemoteBranches(ISession? session)
    {
        remoteBranches = new List<Branch>();
        List<string> remoteBranchFiles = GetRemoteBranchesFiles();

        // Add the Remote Branches
        foreach (var file in remoteBranchFiles)
        {
            var branchHash = File.ReadAllText(file);
            if (GlobalVars.EmitNeo)
            {
                Neo4jHelper.AddRemoteBranchToNeo(session, Path.GetFileName(file), branchHash);
                Neo4jHelper.CreateRemoteBranchLinkNeo(session, $"remote{Path.GetFileName(file)}", branchHash.Substring(0, 4));
            }
            CreateRemoteBranchObject(Path.GetFileName(file), branchHash.Substring(0, 4));
        }
    }

    public static void CreateRemoteBranchObject(string name, string hash)
    {
        Branch b = new Branch
        {
            hash = hash,
            name = name
        };

        if (!remoteBranches.Exists(i => i.name == b.name))
        {
            DebugMessages.AddingRemoteBranchObject(b.name, b.hash);
            remoteBranches.Add(b);
        }
    }

    public static List<string> GetRemoteBranchesFiles()
    {
        List<string> remoteBranchFiles = new List<string>();

        if (Directory.Exists(GlobalVars.remoteBranchPath))
        {
            List<string> RemoteDirs = Directory.GetDirectories(GlobalVars.remoteBranchPath).ToList();
            foreach (string remoteDir in RemoteDirs)
            {
                foreach (string file in Directory.GetFiles(remoteDir).ToList())
                {
                    var DirName = new DirectoryInfo(Path.GetDirectoryName(remoteDir + "\\") ?? "");
                    remoteBranchFiles.Add(file);
                }
            }
        }
        return remoteBranchFiles;
    }
}

