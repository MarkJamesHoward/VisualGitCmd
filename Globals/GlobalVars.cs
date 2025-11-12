public abstract class GlobalVars
{
    public static bool LocalDebugWebsite = false;
    public static string? Api { get; set; }
    public static string? exePath { get; set; }

    // Version will be loaded from configuration or assembly
    public static string version = GetVersion();
    public static bool EmitJsonOnly = false;
    public static bool EmitWeb = false;
    public static bool Bare = false;
    public static bool LocalDebugAPI = false;
    public static bool UnPackRefs = false;
    public static bool EmitNeo = false;
    public static bool PerformTextExtraction = false;

    public static string RepoPath = "";

    public static string workingArea = "";
    public static string headPath = "";
    public static string GITobjectsPath = "";
    public static string branchPath = "";
    public static string tagPath = "";

    public static string remoteBranchPath = "";

    public static string CommitNodesJsonFile = "";
    public static string TreeNodesJsonFile = "";
    public static string BlobNodesJsonFile = "";
    public static string HeadNodesJsonFile = "";
    public static string BranchNodesJsonFile = "";
    public static string IndexFilesJsonFile = "";
    public static string WorkingFilesJsonFile = "";

    public static bool debug = false;

    /// <summary>
    /// Gets the version from configuration, assembly, or fallback to default
    /// </summary>
    /// <returns>The application version string</returns>
    private static string GetVersion()
    {
        try
        {
            // First try to get from assembly version
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            if (version != null)
            {
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }

            // Fallback to configuration if available
            var config = ApiConfigurationProvider.Instance;
            // This would need to be implemented in ApiConfiguration if we want config-based version

            // Final fallback
            return "1.3.0";
        }
        catch
        {
            // If anything fails, return default
            return "1.3.0";
        }
    }
}
