public abstract class JSONGeneration
{
    public static void ProcessJSONONLYOutput(List<Branch> branches)
    {
        if (GlobalVars.EmitJsonOnly)
        {
            GitBlobs.Add(GlobalVars.GITobjectsPath, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
            JSONGeneration.OutputNodesJson(GitCommits.Commits, GlobalVars.CommitNodesJsonFile);
            JSONGeneration.OutputNodesJson(GitTrees.Trees, GlobalVars.TreeNodesJsonFile);
            JSONGeneration.OutputNodesJson(GitBlobs.Blobs, GlobalVars.BlobNodesJsonFile);
            HEADJsonGeneration.OutputHEADJsonToFile(GlobalVars.HeadNodesJsonFile, GlobalVars.headPath);
            JSONGeneration.OutputBranchJson(branches, GitTrees.Trees, GitBlobs.Blobs, GlobalVars.BranchNodesJsonFile);
            IndexFilesJson.OutputIndexFilesJson(GlobalVars.IndexFilesJsonFile);
            WorkingAreaFilesJson.OutputWorkingFilesJsonToFile(GlobalVars.workingArea, GlobalVars.WorkingFilesJsonFile);
        }
    }







    public static void OutputNodesJson<T>(List<T> Nodes, string JsonPath)
    {
        var Json = JsonSerializer.Serialize(Nodes);
        File.WriteAllText(JsonPath, Json);
    }

    public static void OutputBranchJson<T>(List<T> Nodes, List<Tree> TreeNodes, List<Blob> blobs, string JsonPath)
    {
        var Json = JsonSerializer.Serialize(Nodes);
        File.WriteAllText(JsonPath, Json);
    }

    public static async void OutputNodesJsonToAPI(bool firstrun, string name, int dataID, List<Commit> CommitNodes,
     List<Blob> BlobNodes, List<Tree> TreeNodes, List<Branch> BranchNodes, List<Branch> RemoteBranchNodes,
     List<IndexFile> IndexFilesNodes, List<WorkingFile> WorkingFilesNodes, HEADNode HEADNodes)
    {
        if (GlobalVars.EmitWeb)
        {
            var Json = string.Empty;

            var CommitJson = JsonSerializer.Serialize(CommitNodes);
            var BlobJson = JsonSerializer.Serialize(BlobNodes);
            var TreeJson = JsonSerializer.Serialize(TreeNodes);
            var BranchJson = JsonSerializer.Serialize(BranchNodes);
            DebugMessages.OutputBranchJson(BranchJson);
            DebugMessages.OutputBlobJson(BlobJson);
            DebugMessages.OutputTreeJson(TreeJson);



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


}

