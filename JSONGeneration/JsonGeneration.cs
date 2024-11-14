public abstract class JSONGeneration
{

    public static void CreateTreeToBlobLinkJson(string parent, string child, List<TreeNode> treeNodes)
    {
        var treeNode = treeNodes?.Find(i => i.hash == parent);
        treeNode?.blobs?.Add(child);
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

    public static void CreateCommitJson(List<string> parentCommitHash, string comment, string hash, string treeHash, string contents, List<CommitNode> CommitNodes)
    {
        CommitNode n = new CommitNode();
        n.text = comment;
        n.hash = hash;
        n.parent = parentCommitHash;
        n.tree = treeHash;

        if (!CommitNodes.Exists(i => i.hash == n.hash))
            CommitNodes.Add(n);
    }

    public static void CreateTreeJson(string treeHash, string contents, List<TreeNode> TreeNodes)
    {
        TreeNode tn = new TreeNode();
        tn.hash = treeHash;
        tn.blobs = new List<string>();

        if (!TreeNodes.Exists(i => i.hash == tn.hash))
        {
            TreeNodes.Add(tn);
        }
    }
    public static async Task PostAsync(bool firstrun, string name, int dataID, HttpClient httpClient, string commitjson, string blobjson, string treejson, string branchjson, string remotebranchjson, string indexfilesjson, string workingfilesjson, string HEADjson)
    {
        if (firstrun)
            Console.WriteLine($"Visual Git ID:  {name}"); //Outputs some random first and last name combination in the format "{first} {last}" example: "Mark Rogers"

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
    public static void OutputHEADJson(HEAD head, string JsonPath, string path)
    {
        string HeadContents = File.ReadAllText(Path.Combine(GlobalVars.path, "HEAD"));
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

        var Json = string.Empty;
        Json = JsonSerializer.Serialize(h);

        //Console.WriteLine(Json);
        File.WriteAllText(JsonPath, Json);
    }

}

