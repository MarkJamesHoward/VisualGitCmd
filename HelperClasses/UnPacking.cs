

public abstract class UnPacking
{
    public static void PerformUnpackingIfRequested()
    {
        // If Unpacking then perform this first prior to looking at the files in the repo
        if (GlobalVars.UnPackRefs)
        {
            UnPacking.UnpackRefs(GlobalVars.RepoPath);
            UnPacking.UnPackPackFile(GlobalVars.RepoPath);
        }
    }

    public static void UnPackPackFile(string RepoPath)
    {
        int tries = 0;
        bool exit = false;

        Console.WriteLine("Moving Pack Files from .git folder into base folder");
        var files = new DirectoryInfo($"{RepoPath}\\.git\\objects\\pack").GetFiles("*pack-*");
        foreach (FileInfo fi in files)
        {
            string dest = $"{RepoPath}\\{Path.GetFileName(fi.FullName)}";
            fi.MoveTo(dest, true);
        }

        Console.WriteLine("Unpacking PACK file");
        Process p = new Process();
        p.StartInfo = new ProcessStartInfo($"cmd.exe");
        p.StartInfo.Arguments = $"/C type pack-*.pack | git unpack-objects";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.WorkingDirectory = GlobalVars.workingArea;
        p.Start();

        while (!p.HasExited && !exit)
        {
            System.Threading.Thread.Sleep(100);
            if (tries++ > 10)
            {
                exit = true;
                throw new Exception("Delet Pack File did not return within a second");
            }
        }
        Console.WriteLine(p.StandardOutput.ReadToEnd());

        Console.WriteLine("Deleting PACK files from base folder");
        var MovedPackfiles = new DirectoryInfo($"{RepoPath}").GetFiles("*pack-*");
        foreach (FileInfo fi in MovedPackfiles)
        {
            fi.IsReadOnly = false;
            fi.Delete();
        }

    }

    public static void UnpackRefs(string RepoPath)
    {
        Regex regex = new Regex(@"([A-Za-z0-9]{40})\s(refs/heads/)([a-zA-Z0-9]+)");
        string pathToPackedRefsFile = $"{RepoPath}\\.git\\packed-refs";
        string pathToRefsHeadsFolder = $"{RepoPath}\\.git\\refs\\heads\\";

        Console.WriteLine(GlobalVars.GITobjectsPath);

        string packedRefsText = File.ReadAllText(pathToPackedRefsFile);

        MatchCollection matches = regex.Matches(packedRefsText);

        Console.WriteLine("Unpacking packed-refs file");
        foreach (Match match in matches)
        {

            if (match.Success)
            {
                Console.WriteLine($"Extracting Branch {match.Groups[3]}");
                string fileName = $"{pathToRefsHeadsFolder}{match.Groups[3]}";
                string contents = $"{match.Groups[1]}";
                File.WriteAllText(fileName, contents);
            }
        }

        Console.WriteLine("Deleting packed-refs file");
        var PackedRefsFile = new DirectoryInfo($"{RepoPath}\\.git\\").GetFiles("packed-refs");
        foreach (FileInfo fi in PackedRefsFile)
        {
            fi.IsReadOnly = false;
            fi.Delete();
        }
    }
}
