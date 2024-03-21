using System.Diagnostics;
using System.Text.RegularExpressions;
using Neo4j.Driver;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class BlobCode
{
     public static void AddBlobToNeo(ISession session, string filename, string hash, string contents)
    {
        string filenameplushash = $"{filename} #{hash}";

        var greeting = session.ExecuteWrite(
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
     public static void AddOrphanBlobsToJson(string branchPath, string path,List<Blob> blobs, string workingArea, bool PerformTextExtraction)
    {
        //Console.WriteLine("Adding orphan blobs to json");

        List<string> branchFiles = Directory.GetFiles(branchPath).ToList();
        List<string> directories = Directory.GetDirectories(path).ToList();
        List<string> files = new List<string>();

        foreach (string dir in directories)
        {
            if (dir.Contains("info") || dir.Contains("pack"))
            {
                break;
            }

            files = Directory.GetFiles(dir).ToList();

            foreach (string file in files)
            {
                string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);
                string fileType = FileType.GetFileType(hashCode, workingArea);

                if (fileType.Contains("blob"))
                {;

                    string blobContents = String.Empty;

                    if (PerformTextExtraction)
                    {
                        blobContents = FileType.GetContents(hashCode, workingArea);
                    }

                    //Console.WriteLine($"blob {hashCode}");
                    AddBlobToJson("", "", hashCode, blobContents, blobs);
                    
                }
            }
        }
    }

    public static void AddOrphanBlobs(ISession session, string branchPath, string path,List<Blob> blobs, string workingArea, bool PerformTextExtraction)
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
                string fileType = FileType.GetFileType(hashCode, workingArea);

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
      public static void AddBlobToJson(string treeHash, string filename, string hash, string contents, List<Blob> Blobs)
    {
        Blob b = new Blob();
        b.filename = filename;
        b.hash = hash;
        b.tree = treeHash;
        b.contents = contents;

        if (!Blobs.Exists(i => i.hash == b.hash))
        {
            //Console.WriteLine($"Adding blob {b.hash}");
            Blobs.Add(b);
        }
        else
        {
            //Console.WriteLine($"Skipping blob {b.hash}");
            // If filename is different then it has the same contents 
            // Combine the names so they are both displayed
            var existingBlob = Blobs.Find(i => i.hash == b.hash);
            if (!existingBlob.filename.Contains(b.filename))
            {
                existingBlob.filename = existingBlob.filename + " " + b.filename;
            }

        }
    }
}