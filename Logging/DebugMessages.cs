using static MyLogging;

public abstract class DebugMessages()
{
    public static void FoundFileOfType(string type, string hash)
    {
        logger?.LogDebug($"Processing file of type {type.Replace('\n', ' ')} hashCode={hash}");
    }
    public static void OutputBranchJson(string data)
    {
        logger?.LogDebug($"Branch JSON: {data}");
    }
    public static void AddingRemoteBranchObject(string name, string hash)
    {
        logger?.LogDebug($"Creating Remote branch: name={name} hash={hash}");
    }
    public static void AddedNewCommitObjectToCommitNodesList(string value, string comment)
    {
        logger?.LogDebug($"Added Commit object to our list of COMMIT Nodes: hash={value} comment={comment}");
    }
    public static void AddingBranchObject(string name, string hash)
    {
        logger?.LogDebug($"Adding Branch object to our list of BRANCH Nodes: Name={name} PointingTO={hash}");
    }
    public static void AddingBlobObject(string hash, string name)
    {
        logger?.LogDebug($"Adding Blob object to our list of BLOB Nodes: hash={hash} name={name}");
    }

    public static void HeadPointingTo(string hash)
    {
        logger?.LogDebug($"HEAD is now pointing to {hash}");
    }

    public static void DisplayCurrentDirectory(string path)
    {
        logger?.LogDebug($"Current Directory = {path}");
    }

}