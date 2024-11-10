using MyProjectl;

namespace MyProject {

public abstract class RemoteBranches {

    public static void GetRemoteBranches(ref List<string> remoteBranchFiles, ref List<string> branchFiles ) {

                // List<string> remoteBranchFiles = new List<string>();

                // List<string> branchFiles = Directory.GetFiles(GlobalVars.branchPath).ToList();
                branchFiles = Directory.GetFiles(GlobalVars.branchPath).ToList();

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
                // return remoteBranchFiles;
            }
        }
}
