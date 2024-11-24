public abstract class GitIndexFiles
{
    public static List<IndexFile> IndexFiles = new List<IndexFile>();

    public static void ProcessIndexFiles(string workingArea)
    {
        IndexFiles = new List<IndexFile>();

        var Json = string.Empty;

        string files = FileType.GetIndexFiles(GlobalVars.workingArea);
        // Console.WriteLine(files);
        List<string> fileList = files.Split("\n").ToList();

        foreach (string file in fileList)
        {
            IndexFile FileObj = new IndexFile();
            FileObj.filename = file;
            IndexFiles.Add(FileObj);
        }
    }
}