using Neo4j.Driver;
public abstract class BlobCode
{
    public static List<Blob> Blobs = new List<Blob>();

    public static void AddBlobToNeo(ISession? session, string filename, string hash, string contents)
    {
        string filenameplushash = $"{filename} #{hash}";

        var greeting = session?.ExecuteWrite(
        tx =>
        {
            var result = tx.Run(
                "CREATE (a:blob) " +
                "SET a.filenameplushash = $filenameplushash " +
                "SET a.hash = $hash " +
                "SET a.filename = $filename " +
                "SET a.contents = $contents " +
                "RETURN a.name + ', from node ' + id(a)",
                new { filenameplushash, hash, filename, contents });

            return "created node";
        });
    }
    public static void FindBlobs(string path, string workingArea, bool PerformTextExtraction)
    {
        TraceMessages.AddingOrphanBlobsToJson();

        foreach (string dir in Directory.GetDirectories(path).ToList())
        {
            if (dir.Contains("info") || dir.Contains("pack"))
            {
                break;
            }

            List<string> files = Directory.GetFiles(dir).ToList();

            foreach (string file in files)
            {
                string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);
                string fileType = FileType.GetFileType_UsingGitCatFileCmd_Param_T(hashCode, workingArea);

                if (fileType.Contains("blob"))
                {
                    string blobContents = String.Empty;

                    if (PerformTextExtraction)
                    {
                        blobContents = FileType.GetContents(hashCode, workingArea);
                    }

                    AddToBlobObjectCollection("", "", hashCode, blobContents);
                }
            }
        }
    }

    public static void AddOrphanBlobs(ISession? session, string branchPath, string path, string workingArea, bool PerformTextExtraction)
    {

        List<string> branchFiles = Directory.GetFiles(branchPath).ToList();
        List<string> directories = Directory.GetDirectories(path).ToList();
        List<string> files = new List<string>();

        foreach (string dir in directories)
        {
            files = Directory.GetFiles(dir).ToList();

            foreach (string file in files)
            {
                string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);
                string fileType = FileType.GetFileType_UsingGitCatFileCmd_Param_T(hashCode, workingArea);

                if (fileType.Contains("blob"))
                {
                    string blobContents = string.Empty;

                    if (PerformTextExtraction)
                    {
                        FileType.GetContents(hashCode, workingArea);
                    }

                    Console.WriteLine($"blob {hashCode}");
                    if (!FileType.DoesNodeExistAlready(session, hashCode, "blob"))
                    {
                        AddBlobToNeo(session, hashCode, hashCode, blobContents);
                    }
                }
            }
        }
    }
    public static void AddToBlobObjectCollection(string treeHash, string filename, string hash, string contents)
    {
        Blob b = new Blob();
        b.filename = filename;
        b.hash = hash;
        b.tree = treeHash;
        b.contents = contents;

        if (!Blobs.Exists(i => i.hash == b.hash))
        {
            DebugMessages.AddingBlobObject(b.hash, b.filename);
            Blobs.Add(b);
        }
        else
        {
            // If filename is different then it has the same contents 
            // Combine the names so they are both displayed
            var existingBlob = Blobs.Find(i => i.hash == b.hash);
            if (existingBlob?.filename?.Contains(b.filename) == false)
            {
                StandardMessages.BlobSkippedAsSameContents(b.hash);
                existingBlob.filename = existingBlob?.filename + " " + b.filename;
            }

        }
    }
}