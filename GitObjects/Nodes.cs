using Neo4j.Driver;
public class Nodes
{

    public static List<WorkingFile> WorkingFilesNodes(string workingFolder)
    {

        List<string> files = FileType.GetWorkingFiles(workingFolder);
        List<WorkingFile> WorkingFilesList = new List<WorkingFile>();


        foreach (string file in files)
        {
            WorkingFile FileObj = new WorkingFile();
            FileObj.filename = file;
            if (GlobalVars.PerformTextExtraction)
            {
                FileObj.contents = FileType.GetFileContents(Path.Combine(workingFolder, file));
            }
            WorkingFilesList.Add(FileObj);
        }
        return WorkingFilesList;
    }
    public static void GetHEAD(ISession? session, string path)
    {
        string HeadContents = File.ReadAllText(Path.Combine(GlobalVars.GITobjectsPath, "HEAD"));

        // Is the HEAD detached in which case it contains a Commit Hash
        Match match = Regex.Match(HeadContents, "[0-9a-f]{40}");
        if (match.Success)
        {
            string HEADHash = match.Value.Substring(0, 4);
            //Create the HEAD Node
            Neo4jHelper.AddHeadToNeo(session, HEADHash, HeadContents);
            //Create Link to Commit
            Neo4jHelper.CreateHEADTOCommitLinkNeo(session, HEADHash);
        }

        match = Regex.Match(HeadContents, @"ref: refs/heads/(\w+)");
        if (match.Success)
        {
            //Console.WriteLine("HEAD Branch extract: " + match.Groups[1]?.Value);
            string branch = match.Groups[1].Value;
            //Create the HEAD Node
            Neo4jHelper.AddHeadToNeo(session, branch, HeadContents);
            //Create Link to Commit
            Neo4jHelper.CreateHEADTOBranchLinkNeo(session, branch);
        }
    }
    public static HEAD HEADNodes(string path)
    {
        string HeadContents = File.ReadAllText(Path.Combine(path, "HEAD"));
        //Console.WriteLine("Outputting JSON HEAD");
        string HEADHash = "";

        // Is the HEAD detached in which case it contains a Commit Hash
        Match match = Regex.Match(HeadContents, "[0-9a-f]{40}");
        if (match.Success)
        {
            //Console.WriteLine("Outputting JSON HEAD match found 1");
            HEADHash = match.Value.Substring(0, 4);
        }
        match = Regex.Match(HeadContents, @"ref: refs/heads/(\w+)");
        if (match.Success)
        {
            //Console.WriteLine("Outputting JSON HEAD match found 2");

            //Console.WriteLine("HEAD Branch extract: " + match.Groups[1]?.Value);
            HEADHash = match.Groups[1].Value;
            //CreateHEADTOBranchLinkNeo(session, branch);
        }
        HEAD h = new HEAD();
        h.hash = HEADHash;
        return h;

    }

}
