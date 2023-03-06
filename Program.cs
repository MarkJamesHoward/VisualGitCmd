using System.Diagnostics;
using System.Text.RegularExpressions;

string path = @"C:\dev\visual\.git\objects\";
List<string> HashCodeFilenames = new List<string>();

// Get all the files in the .git/objects folder
try
{

    List<string> directories = Directory.GetDirectories(path).ToList();
    List<string> files = new List<string>();

    foreach (string dir in directories)
    {
        files = Directory.GetFiles(dir).ToList();

        foreach (string file in files)
        {

            string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 4);


            HashCodeFilenames.Add(hashCode);

            string fileType = GetFileType(hashCode);


            if (fileType.Contains("blob"))
            {
                //Nothing to do here
            }
            else if (fileType.Contains("tree"))
            {
                //Nothing to do here
            }
            else if (fileType.Contains("commit"))
            {
                Console.WriteLine($"{fileType.TrimEnd('\n', '\r')} {hashCode}");
                string commitContents = GetContents(hashCode);
                var match = Regex.Match(commitContents, "tree ([0-9a-f]{4})");
                if (match.Success)
                {
                    Console.WriteLine($"\t-> tree {match.Groups[1].Value}");
                    // Get the details of the Blobs in this Tree
                    string tree = GetContents(match.Groups[1].Value);
                    var blobsInTree = Regex.Matches(tree, @"blob ([0-9a-f]{4})[0-9a-f]{36}.(\w+)");
                    foreach (Match blobMatch in blobsInTree)
                    {
                        Console.WriteLine($"\t\t-> blob {blobMatch.Groups[1]} {blobMatch.Groups[2]} ");
                    }
                }
                else
                {
                    Console.WriteLine("No Tree found in Commit");
                }
            }
        }
    }
}
catch (Exception e)
{
    Console.WriteLine($"Error while getting files in {path} {e.Message}");
}



static string GetFileType(string file)
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

static string GetContents(string file)
{
    Process p = new Process();
    p.StartInfo = new ProcessStartInfo("git.exe", $"cat-file {file} -p");
    p.StartInfo.RedirectStandardOutput = true;
    p.Start();

    while (!p.HasExited)
    {
        System.Threading.Thread.Sleep(100);
    }
    string contents = p.StandardOutput.ReadToEnd();
    return contents;
}
