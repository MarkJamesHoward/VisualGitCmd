using Neo4j.Driver;

public abstract class FileType
{
    public static bool DoesNodeExistAlready(ISession? session, string hash, string type)
    {
        var greeting = session?.ExecuteWrite(
        tx =>
        {
            var result = tx.Run(
                $"MATCH (a:{type}) WHERE a.hash = '{hash}' RETURN a.name + ', from node ' + id(a)",
                new { });

            return result.Count() > 0 ? true : false;
        });

        return greeting ?? false;
    }
    public static string GetContents(string file, string workingArea)
    {
        int count = 0;
        Process p = new Process();
        p.StartInfo = new ProcessStartInfo("git", $"cat-file {file} -p");
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.WorkingDirectory = workingArea;
        p.StartInfo.UseShellExecute = false; //Import in Linux environments
        p.Start();

        while (!p.HasExited)
        {
            System.Threading.Thread.Sleep(100);
            count++;
            if (count > 10)
            {
                throw new Exception("Cat File did not return within a second");
            }
        }
        string contents = p.StandardOutput.ReadToEnd();
        return contents;
    }


    public static string GetFileType(string file, string workingArea)
    {
        int tries = 0;
        bool exit = false;

        //run the git cat-file command to determine the file type
        Process p = new Process();
        //Console.WriteLine("git " + $"cat-file {file} -t");
        p.StartInfo = new ProcessStartInfo("git", $"cat-file {file} -t");
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.WorkingDirectory = workingArea;
        p.Start();

        while (!p.HasExited && !exit)
        {
            System.Threading.Thread.Sleep(100);
            // Console.WriteLine($"Running git cat-file {file} -t");
            if (tries++ > 10)
            {
                exit = true;
                throw new Exception("Cat File did not return within a second");
            }
        }
        return p.StandardOutput.ReadToEnd();
    }


    public static string GetIndexFiles(string workingArea)
    {
        int count = 0;
        //run the git cat-file command to determine the file type
        Process p = new Process();
        p.StartInfo = new ProcessStartInfo("git", $"ls-files -s");
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.WorkingDirectory = workingArea;
        p.Start();

        while (!p.HasExited)
        {
            System.Threading.Thread.Sleep(100);
            if (count++ > 10)
            {
                throw new Exception("git ls-files failed to return..");
            }
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

    public static string GetFileContents(string filename)
    {

        string contents = File.ReadAllText(filename);
        //Console.WriteLine("Reading File contents " + filename);
        return contents;
    }
}