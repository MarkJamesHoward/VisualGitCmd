using System.Reflection;

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
    public static bool SingleRun = false;
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
            // Try to get version from assembly directly
            var assembly = Assembly.GetEntryAssembly();
            var version = assembly
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            if (!string.IsNullOrEmpty(version))
            {
                // Remove git hash (everything after '+' character)
                var plusIndex = version.IndexOf('+');
                return plusIndex >= 0 ? version.Substring(0, plusIndex) : version;
            }

            // Final fallback
            return "1.0.0-unknown";
        }
        catch
        {
            // If anything fails, return default
            return "1.0.0-unknown";
        }
    }
}
