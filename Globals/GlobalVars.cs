
public abstract class GlobalVars
{
    public static string? exePath { get; set; }
    public static string version = "0.0.14";
    public static bool EmitJsonOnly = false;
    public static bool EmitWeb = false;
    public static bool UnPackRefs = false;
    public static bool EmitNeo = false;
    public static bool PerformTextExtraction = false;

    public static string RepoPath = "";

    public static string workingArea = "";
    public static string head = "";
    public static string path = "";
    public static string branchPath = "";
    public static string remoteBranchPath = "";

    public static string CommitNodesJsonFile = "";
    public static string TreeNodesJsonFile = "";
    public static string BlobNodesJsonFile = "";
    public static string HeadNodesJsonFile = "";
    public static string BranchNodesJsonFile = "";
    public static string IndexFilesJsonFile = "";
    public static string WorkingFilesJsonFile = "";

    public static bool debug = false;


}
