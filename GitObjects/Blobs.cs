using System.Data;
using Neo4j.Driver;
public abstract class GitBlobs
{
    public static List<Blob> Blobs = new List<Blob>();


    public static void Add(string path, string workingArea, bool PerformTextExtraction)
    {
        TraceMessages.AddingOrphanBlobsToJson();

        foreach (string dir in Directory.GetDirectories(path).ToList())
        {
            if (dir.Contains("info") || dir.Contains("pack"))
            {
                continue;
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
                    DebugMessages.GenericMessage("Adding BLOB from AddingOrphanBlobsToJson");
                    Add("", "", hashCode, blobContents);
                }
            }
        }
    }


    public static void Add(string treeHash, string filename, string hash, string contents)
    {
        Blob b = new Blob();
        b.filename = filename;
        b.hash = hash;
        b.contents = contents;

        if (b.trees == null)
        {
            b.trees = new List<string>();
        }

        if (treeHash != "")
        {
            DebugMessages.GenericMessage("Tree for BLOB added " + treeHash);
            b.trees.Add(treeHash);
        }

        if (!Blobs.Exists(i => i.hash == b.hash))
        {
            DebugMessages.AddingBlobObject(b.hash, b.filename);
            Blobs.Add(b);
        }
        else
        {
            DebugMessages.ExistingBlobObjectUpdate(b.hash, b.filename, treeHash);

            // If filename is different then it has the same contents 
            // Combine the names so they are both displayed
            var existingBlob = Blobs.Find(i => i.hash == b.hash);
            if (existingBlob?.trees == null)
            {
                existingBlob.trees = new List<string>();
                existingBlob?.trees.Add(treeHash);
            }
            else if (existingBlob?.trees.Contains(treeHash) == false)
            {
                existingBlob.trees.Add(treeHash);
            }
            if (existingBlob?.filename?.Contains(b.filename) == false)
            {
                StandardMessages.BlobSkippedAsSameContents(b.hash);
                existingBlob.filename = existingBlob?.filename + " " + b.filename;
            }

        }
    }
}