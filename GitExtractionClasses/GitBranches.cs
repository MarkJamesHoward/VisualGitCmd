using MyProjectl;
using Neo4j.Driver;

namespace MyProject {

    public abstract class GitBranches {

        public static void AddBranchToJson(string name, string hash, ref List<Branch> branches)
        {
            Branch b = new Branch();
            b.hash = hash;
            b.name = name;

            if (!branches.Exists(i => i.name == b.name))
            {
                //Console.WriteLine($"Adding branch {b.name} {b.hash}");
                branches.Add(b);
            }
        }

        public static void ProcessBranches(List<string> branchFiles, ISession? session, ref List<Branch> branches) {
                    
                    // Add the Branches
                    foreach (var file in branchFiles)
                    {
                        var branchHash = File.ReadAllText(file);
                        if (GlobalVars.EmitNeo)
                        {
                            Neo4j.AddBranchToNeo(session, Path.GetFileName(file), branchHash);
                            Neo4j.CreateBranchLinkNeo(session, Path.GetFileName(file), branchHash.Substring(0, 4));
                        }
                        AddBranchToJson(Path.GetFileName(file), branchHash.Substring(0, 4), ref branches);
                    }

        }
    }
}