
public abstract class RemoteBranches
{

    public static void ProcessRemoteBranches(List<string> remoteBranchFiles, ISession? session, ref List<Branch> remoteBranches)
    {

        // Add the Remote Branches
        foreach (var file in remoteBranchFiles)
        {
            var branchHash = File.ReadAllText(file);
            if (GlobalVars.EmitNeo)
            {
                Neo4jHelper.AddRemoteBranchToNeo(session, Path.GetFileName(file), branchHash);
                Neo4jHelper.CreateRemoteBranchLinkNeo(session, $"remote{Path.GetFileName(file)}", branchHash.Substring(0, 4));
            }
            GitBranches.AddBranchToJson(Path.GetFileName(file), branchHash.Substring(0, 4), ref remoteBranches);

        }
    }
    public static void GetRemoteBranches(ref List<string> remoteBranchFiles)
    {


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
    }
}

