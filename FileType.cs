using System.Diagnostics;
using System.Text.RegularExpressions;
using Neo4j.Driver;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class FileType {

     public static bool DoesNodeExistAlready(ISession session, string hash, string type)
    {
        var greeting = session.ExecuteWrite(
        tx =>
        {
            var result = tx.Run(
                $"MATCH (a:{type}) WHERE a.hash = '{hash}' RETURN a.name + ', from node ' + id(a)",
                new { });

            return result.Count() > 0 ? true : false;
        });

        return greeting;
    }
    public static string GetContents(string file)
    {
        int count = 0;
        Process p = new Process();
        p.StartInfo = new ProcessStartInfo("git.exe", $"cat-file {file} -p");
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();

        while (!p.HasExited)
        {
            System.Threading.Thread.Sleep(100);
            count++;
            if (count > 10)
            {
                throw new Exception("Cat File did not return withing a second");
            }
        }
        string contents = p.StandardOutput.ReadToEnd();
        return contents;
    }

    public static string GetFileType(string file)
    {
        //run the git cat-file command to determine the file type
        Process p = new Process();
        p.StartInfo = new ProcessStartInfo("git.exe", $"cat-file {file} -t");
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();

        while (!p.HasExited)
        {
            System.Threading.Thread.Sleep(100);
        }
        return p.StandardOutput.ReadToEnd();
    }
    public static string GetIndexFiles()
    {
        //run the git cat-file command to determine the file type
        Process p = new Process();
        p.StartInfo = new ProcessStartInfo("git.exe", $"ls-files");
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();

        while (!p.HasExited)
        {
            System.Threading.Thread.Sleep(100);
        }
        return p.StandardOutput.ReadToEnd();
    }

     public static List<string> GetWorkingFiles(string dir)
    {
        //run the git cat-file command to determine the file type
        List<string> StrippedFiles = new List<string>();

        List<string> files = Directory.GetFiles(dir).ToList();
        files.ForEach(i =>
        {
            StrippedFiles.Add(Path.GetFileName(i));
        });
        //Console.WriteLine($"Folder we checking {dir}");
        return StrippedFiles;
    }
}