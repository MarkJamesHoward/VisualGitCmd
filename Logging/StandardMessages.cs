using static MyLogging;

public abstract class StandardMessages()
{
    public static void AddingBranchObject(string name, string hash)
    {
        logger?.LogInformation($"Adding Branch {name} {hash}");
    }

    public static void DisplayVersion()
    {
        logger?.LogInformation($"Version {GlobalVars.version} - Ensure matches against website for compatibility");
    }

    public static void InvalidRepoPath(string value)
    {
        logger?.LogInformation($"Invalid Repo Path{value}");
    }
    public static void WebEmissionEnabled()
    {
        logger?.LogInformation("Web emission enabled");
    }

    public static void RepoToExamine(string value)
    {
        logger?.LogInformation($"Repo to examine: {value}");
    }

    public static void AddingBlobObject(string hash)
    {
        logger?.LogDebug($"Adding Blob Object {hash}");
    }

    public static void BlobSkippedAsSameContents(string hash)
    {
        logger?.LogDebug($"Blob with this hash already exists - combine filenames so both are displayed on single blob {hash}");
    }

    public static void SameFolderMessage()
    {
        logger?.LogInformation("VisualGit should not be run in the same folder as the Repository to be examined");
        logger?.LogInformation("Option1: Place Visual.exe into another folder and run with --p pointing to this folder");
        logger?.LogInformation("Option2: Place the Visual.exe application into a folder on your PATH. Then just run Visual from within the Repository as you just did");
    }

    public static void ParentCommitHashCode(string commit, string parent)
    {
        logger?.LogDebug($"\t-> commit {commit} linked to parent commit {parent}");
    }

    public static void ExePath(string path)
    {
        logger?.LogInformation($"Exe Path {path}");
    }

}