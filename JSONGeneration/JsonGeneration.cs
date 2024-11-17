public abstract class JSONGeneration
{
    public static void ProcessJSONONLYOutput(List<Branch> branches)
    {
        if (GlobalVars.EmitJsonOnly)
        {
            BlobCode.FindOrphanBlobs(GlobalVars.GITobjectsPath, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
            JSONGeneration.OutputNodesJson(CommitNodesList.CommitNodes, GlobalVars.CommitNodesJsonFile);
            JSONGeneration.OutputNodesJson(TreeNodesList.TreeNodes, GlobalVars.TreeNodesJsonFile);
            JSONGeneration.OutputNodesJson(BlobCode.Blobs, GlobalVars.BlobNodesJsonFile);
            HEADJsonGeneration.OutputHEADJson(GlobalVars.HeadNodesJsonFile, GlobalVars.headPath);
            JSONGeneration.OutputBranchJson(branches, TreeNodesList.TreeNodes, BlobCode.Blobs, GlobalVars.BranchNodesJsonFile);
            JSONGeneration.OutputIndexFilesJson(GlobalVars.IndexFilesJsonFile);
            JSONGeneration.OutputWorkingFilesJson(GlobalVars.workingArea, GlobalVars.WorkingFilesJsonFile);
        }
    }

    public static void OutputWorkingFilesJson(string workingFolder, string JsonPath)
    {
        var Json = string.Empty;
        List<WorkingFile> WorkingFilesList = new List<WorkingFile>();

        List<string> files = FileType.GetWorkingFiles(workingFolder);

        foreach (string file in files)
        {
            WorkingFile FileObj = new WorkingFile();
            FileObj.filename = file;
            FileObj.contents = FileType.GetFileContents(Path.Combine(workingFolder, file));
            WorkingFilesList.Add(FileObj);
        }

        Json = JsonSerializer.Serialize(WorkingFilesList);
        File.WriteAllText(JsonPath, Json);
    }

    public static List<IndexFile> IndexFilesJsonNodes(string workingArea)
    {
        var Json = string.Empty;
        List<IndexFile> IndexFilesList = new List<IndexFile>();

        string files = FileType.GetIndexFiles(GlobalVars.workingArea);
        // Console.WriteLine(files);
        List<string> fileList = files.Split("\n").ToList();

        foreach (string file in fileList)
        {
            IndexFile FileObj = new IndexFile();
            FileObj.filename = file;
            IndexFilesList.Add(FileObj);
        }

        return IndexFilesList;

    }
    public static void OutputIndexFilesJson(string JsonPath)
    {
        var Json = string.Empty;
        List<IndexFile> IndexFilesList = new List<IndexFile>();

        string files = FileType.GetIndexFiles("");
        //Console.WriteLine(files);
        List<string> fileList = files.Split("\n").ToList();

        foreach (string file in fileList)
        {
            IndexFile FileObj = new IndexFile();
            FileObj.filename = file;
            IndexFilesList.Add(FileObj);
        }

        Json = JsonSerializer.Serialize(IndexFilesList);

        Console.WriteLine(JsonPath);
        File.WriteAllText(JsonPath, Json);
    }


    public static void OutputNodesJson<T>(List<T> Nodes, string JsonPath)
    {
        var Json = JsonSerializer.Serialize(Nodes);
        File.WriteAllText(JsonPath, Json);
    }

    public static void OutputBranchJson<T>(List<T> Nodes, List<TreeNode> TreeNodes, List<Blob> blobs, string JsonPath)
    {
        var Json = JsonSerializer.Serialize(Nodes);
        File.WriteAllText(JsonPath, Json);
    }

    public static async void OutputNodesJsonToAPI(bool firstrun, string name, int dataID, List<CommitNode> CommitNodes,
     List<Blob> BlobNodes, List<TreeNode> TreeNodes, List<Branch> BranchNodes, List<Branch> RemoteBranchNodes,
     List<IndexFile> IndexFilesNodes, List<WorkingFile> WorkingFilesNodes, HEAD HEADNodes)
    {
        var Json = string.Empty;

        var CommitJson = JsonSerializer.Serialize(CommitNodes);
        var BlobJson = JsonSerializer.Serialize(BlobNodes);
        var TreeJson = JsonSerializer.Serialize(TreeNodes);
        var BranchJson = JsonSerializer.Serialize(BranchNodes);
        DebugMessages.OutputBranchJson(BranchJson);

        var RemoteBranchJson = JsonSerializer.Serialize(RemoteBranchNodes);
        var IndexFilesJson = JsonSerializer.Serialize(IndexFilesNodes);
        var WorkingFilesJson = JsonSerializer.Serialize(WorkingFilesNodes);
        var HEADJson = JsonSerializer.Serialize(HEADNodes);

        HttpClient sharedClient = new()
        {
            BaseAddress = new Uri("https://gitvisualiserapi.azurewebsites.net/api/gitinternals"),
        };
        await Browser.PostAsync(firstrun, name, dataID, sharedClient, CommitJson, BlobJson, TreeJson, BranchJson, RemoteBranchJson, IndexFilesJson, WorkingFilesJson, HEADJson);
    }


}

