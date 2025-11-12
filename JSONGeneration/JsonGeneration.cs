using System.Text.Encodings.Web;

public abstract class JSONGeneration
{
    public static void ProcessJSONONLYOutput(List<Branch> branches)
    {
        if (GlobalVars.EmitJsonOnly)
        {
            GitBlobs.Add(
                GlobalVars.GITobjectsPath,
                GlobalVars.workingArea,
                GlobalVars.PerformTextExtraction
            );
            JSONGeneration.OutputNodesJson(GitCommits.Commits, GlobalVars.CommitNodesJsonFile);
            JSONGeneration.OutputNodesJson(GitTrees.Trees, GlobalVars.TreeNodesJsonFile);
            JSONGeneration.OutputNodesJson(GitBlobs.Blobs, GlobalVars.BlobNodesJsonFile);
            HEADJsonGeneration.OutputHEADJsonToFile(
                GlobalVars.HeadNodesJsonFile,
                GlobalVars.headPath
            );
            JSONGeneration.OutputBranchJson(
                branches,
                GitTrees.Trees,
                GitBlobs.Blobs,
                GlobalVars.BranchNodesJsonFile
            );
            IndexFilesJson.OutputIndexFilesJson(GlobalVars.IndexFilesJsonFile);
            WorkingAreaFilesJson.OutputWorkingFilesJsonToFile(
                GlobalVars.workingArea,
                GlobalVars.WorkingFilesJsonFile
            );
        }
    }

    public static void OutputNodesJson<T>(List<T> Nodes, string JsonPath)
    {
        var Json = JsonSerializer.Serialize(Nodes);
        File.WriteAllText(JsonPath, Json);
    }

    public static void OutputBranchJson<T>(
        List<T> Nodes,
        List<Tree> TreeNodes,
        List<Blob> blobs,
        string JsonPath
    )
    {
        var Json = JsonSerializer.Serialize(Nodes);
        File.WriteAllText(JsonPath, Json);
    }

    public static async void OutputNodesJsonToAPI(
        bool firstrun,
        string name,
        int dataID,
        List<Commit> CommitNodes,
        List<Blob> BlobNodes,
        List<Tree> TreeNodes,
        List<Branch> BranchNodes,
        List<Branch> RemoteBranchNodes,
        List<Tag> TagNodes,
        List<IndexFile> IndexFilesNodes,
        List<WorkingFile> WorkingFilesNodes,
        HEADNode HEADNodes
    )
    {
        if (GlobalVars.EmitWeb)
        {
            var Json = string.Empty;

            var CommitJson = JsonSerializer.Serialize(CommitNodes);
            var BlobJson = JsonSerializer.Serialize(BlobNodes);
            var TreeJson = JsonSerializer.Serialize(TreeNodes);
            var BranchJson = JsonSerializer.Serialize(BranchNodes);
            var TagJson = JsonSerializer.Serialize(TagNodes);

            DebugMessages.OutputBranchJson(BranchJson);
            DebugMessages.OutputTagJson(TagJson);
            DebugMessages.OutputCommitJson(CommitJson);
            DebugMessages.OutputTreeJson(TreeJson);
            DebugMessages.OutputBlobJson(BlobJson);

            var RemoteBranchJson = JsonSerializer.Serialize(RemoteBranchNodes);
            var IndexFilesJson = JsonSerializer.Serialize(IndexFilesNodes);
            var WorkingFilesJson = JsonSerializer.Serialize(WorkingFilesNodes);
            var HEADJson = JsonSerializer.Serialize(HEADNodes);

            DebugMessages.OutputHEADJson(HEADJson);
            DebugMessages.OutputIndexFilesJson(IndexFilesJson);
            DebugMessages.OutputWorkingFilesJson(WorkingFilesJson);

            System.Uri BaseAddress = ApiConfigurationProvider.Instance.GetBaseAddress();

            HttpClient sharedClient = new()
            {
                // Local Debug
                BaseAddress = BaseAddress,
            };
            await Browser.PostAsync(
                firstrun,
                name,
                dataID,
                sharedClient,
                CommitJson,
                BlobJson,
                TreeJson,
                BranchJson,
                TagJson,
                RemoteBranchJson,
                IndexFilesJson,
                WorkingFilesJson,
                HEADJson
            );
        }
    }
}
