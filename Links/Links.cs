
public class Links
{
    public static bool DoesTreeToBlobLinkExist(ISession? session, string treeHash, string blobHash)
    {
        string query = "MATCH (t:tree { hash: $treeHash })-[r:blob]->(b:blob {hash: $blobHash }) RETURN r, b";
        var result = session?.Run(
                query,
                new { treeHash, blobHash });

        if (result != null)
        {
            foreach (var record in result)
            {
                return true;
            }
        }

        return false;
    }

    public static void AddCommitParentLinks(ISession? session, string path, string workingArea)
    {
        List<string> directories = Directory.GetDirectories(GlobalVars.path).ToList();

        foreach (string dir in directories)
        {
            var files = Directory.GetFiles(dir).ToList();

            foreach (string file in files)
            {

                string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);
                string fileType = FileType.GetFileType(hashCode, GlobalVars.workingArea);

                if (fileType.Contains("commit"))
                {
                    string commitContents = FileType.GetContents(hashCode, GlobalVars.workingArea);
                    var commitParent = Regex.Match(commitContents, "parent ([0-9a-f]{4})");

                    if (commitParent.Success)
                    {
                        foreach (var item in commitParent.Groups.Values)
                        {
                            // string parentHash = commitParent.Groups[1].Value;
                            string parentHash = item.Value;
                            //Console.WriteLine($"\t-> parent commit {commitParent}");

                            Neo4jHelper.CreateCommitTOCommitLinkNeo(session, hashCode, parentHash);
                        }

                    }
                }
            }
        }
    }


}
