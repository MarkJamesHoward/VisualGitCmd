using static MyLogging;

public abstract class DebugMessages()
{
    public static void AddingBranchObject(string name, string hash)
    {
        logger?.LogDebug($"Adding Branch Object: Name={name} hash={hash}");
    }

    public static void DisplayCurrentDirectory(string path)
    {
        logger?.LogDebug($"Current Directory = {path}");
    }

}