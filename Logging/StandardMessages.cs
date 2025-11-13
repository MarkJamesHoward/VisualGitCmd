using static MyLogging;

public abstract class StandardMessages()
{
    public static void DisplayVersion()
    {
        logger?.LogInformation(
            $"Version {GlobalVars.version} - Ensure matches against website for compatibility"
        );
    }

    public static void UsingAPIUrl(string url)
    {
        logger?.LogInformation($"Using API URL: {url}");
    }

    public static void InvalidRepoPath(string value)
    {
        logger?.LogInformation($"Invalid Repo Path{value}");
    }

    public static void UserSuppliedAPIURL(string url)
    {
        logger?.LogInformation($"User suplied API URL: {url}");
    }

    public static void WebEmissionEnabled()
    {
        logger?.LogInformation("Web emission enabled");
    }

    public static void VisualGitID(string name)
    {
        logger?.LogInformation($"Visual Git Website ID: {name}");
    }

    public static void DebugSelectedAndAlsoRepoPathProvided(string path)
    {
        logger?.LogInformation($"Using supplied repo path with debug enabled {path}");
    }

    public static void UsingDebugHardCodedPath(string path)
    {
        logger?.LogInformation(
            $"Using hard coded path as non supplied on cmd line and debug enabled {path}"
        );
    }

    public static void DebugModeEnabled()
    {
        logger?.LogInformation($"Debug mode enabled");
    }

    public static void RepoToExamine(string value)
    {
        logger?.LogInformation($"Git Repo Selected: {value}");
    }

    public static void BlobSkippedAsSameContents(string hash)
    {
        logger?.LogDebug(
            $"Blob with this hash already exists - combine filenames so both are displayed on single blob {hash}"
        );
    }

    public static void SameFolderMessage()
    {
        logger?.LogInformation(
            "VisualGit should not be run in the same folder as the Repository to be examined when text extraction is enabled"
        );
        logger?.LogInformation(
            "Option1: Place visual.exe into another folder and run with -p pointing to this folder"
        );
        logger?.LogInformation(
            "Option2: Place the visual.exe application into a folder on your PATH. Then just run Visual from within the Repository as you just did"
        );
    }

    public static void ParentCommitHashCode(string commit, string parent)
    {
        logger?.LogDebug($"Commit {commit} linked to parent commit {parent}");
    }

    public static void ExePath(string path)
    {
        logger?.LogInformation($"Exe Path {path}");
    }
}
