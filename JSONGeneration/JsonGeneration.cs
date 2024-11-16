public abstract class JSONGeneration
{
    public static void ProcessJSONONLYOutput(List<Branch> branches)
    {
        if (GlobalVars.EmitJsonOnly)
        {
            BlobCode.FindBlobs(GlobalVars.GITobjectsPath, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
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
        var Json = string.Empty;

        Json = JsonSerializer.Serialize(Nodes);

        //Console.WriteLine(Json);
        //Console.WriteLine(JsonPath);
        File.WriteAllText(JsonPath, Json);
    }

    public static void OutputBranchJson<T>(List<T> Nodes, List<TreeNode> TreeNodes, List<Blob> blobs, string JsonPath)
    {
        var Json = string.Empty;

        Json = JsonSerializer.Serialize(Nodes);

        //Console.WriteLine(Json);
        File.WriteAllText(JsonPath, Json);
    }




    public static async Task PostAsync(bool firstrun, string name, int dataID, HttpClient httpClient, string commitjson, string blobjson, string treejson, string branchjson, string remotebranchjson, string indexfilesjson, string workingfilesjson, string HEADjson)
    {
        if (firstrun)
        {
            StandardMessages.VisualGitID(name);
            // Console.WriteLine($"Visual Git ID:  {name}"); //Outputs some random first and last name combination in the format "{first} {last}" example: "Mark Rogers"
        }

        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                userId = $"{name.Replace(' ', 'x')}",
                id = $"{dataID++}",
                commitNodes = commitjson ?? "",
                blobNodes = blobjson ?? "",
                treeNodes = treejson ?? "",
                branchNodes = branchjson ?? "",
                remoteBranchNodes = remotebranchjson ?? "",
                headNodes = HEADjson ?? "",
                indexFilesNodes = indexfilesjson ?? "",
                workingFilesNodes = workingfilesjson ?? ""
            }),
                Encoding.UTF8,
                "application/json");

        // var resilienace =  new ResiliencePipelineBuilder()
        // .AddRetry(new RetryStrategyOptions {
        //         ShouldHandle = new PredicateBuilder().Handle<Exception>(),
        //         Delay = TimeSpan.FromSeconds(2),
        //         MaxRetryAttempts = 2,
        //         BackoffType = DelayBackoffType.Exponential
        // })
        // .AddTimeout(TimeSpan.FromSeconds(30))
        // .Build();

        HttpResponseMessage response = await Resiliance._resilienace.ExecuteAsync(async ct => await httpClient.PostAsync("GitInternals", jsonContent, ct));

        try
        {
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("Please restart VisualGit...");
        }
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
        var RemoteBranchJson = JsonSerializer.Serialize(RemoteBranchNodes);
        var IndexFilesJson = JsonSerializer.Serialize(IndexFilesNodes);
        var WorkingFilesJson = JsonSerializer.Serialize(WorkingFilesNodes);
        var HEADJson = JsonSerializer.Serialize(HEADNodes);

        HttpClient sharedClient = new()
        {
            BaseAddress = new Uri("https://gitvisualiserapi.azurewebsites.net/api/gitinternals"),
        };
        await PostAsync(firstrun, name, dataID, sharedClient, CommitJson, BlobJson, TreeJson, BranchJson, RemoteBranchJson, IndexFilesJson, WorkingFilesJson, HEADJson);
    }


}

