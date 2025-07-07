using static MyLogging;

public abstract class DebugMessages()
{
    public static void GenericMessage(string message)
    {
        logger?.LogDebug(message);
    }
    public static void IgnoreDirectory(string? dir)
    {
        logger?.LogDebug($"Ignoring directory: {dir}");
    }

    public static void IgnoringFile(string? file)
    {
        logger?.LogDebug($"Ignoring file: {file}");
    }

    public static void FileChanged(string? filename)
    {
        logger?.LogDebug($"File changed: {filename}");
    }

    public static void FileCreated(string? filename)
    {
        logger?.LogDebug($"File changed: {filename}");
    }
    public static void FileDeleted(string? filename)
    {
        logger?.LogDebug($"File deleted: {filename}");
    }
    public static void FileRenamed(string? filename, string? oldName)
    {
        logger?.LogDebug($"File renamed: {filename} oldName={oldName}");
    }


    public static void FoundFileOfType(string type, string hash)
    {
        logger?.LogDebug($"Processing file of type {type.Replace('\n', ' ')} hashCode={hash}");
    }
    public static void OutputBranchJson(string data)
    {
        logger?.LogDebug($"Branch JSON: {data}");
    }
    public static void OutputTagJson(string data)
    {
        logger?.LogDebug($"Tag JSON: {data}");
    }
    public static void OutputCommitJson(string data)
    {
        logger?.LogDebug($"Commit JSON: {data}");
    }
    public static void OutputHEADJson(string data)
    {
        logger?.LogDebug($"HEAD JSON: {data}");
    }
    public static void OutputIndexFilesJson(string data)
    {
        logger?.LogDebug($"Index Files JSON: {data}");
    }
    public static void OutputWorkingFilesJson(string data)
    {
        logger?.LogDebug($"Working Files JSON: {data}");
    }

    public static void OutputBlobJson(string data)
    {
        logger?.LogDebug($"Blob JSON: {data}");
    }

    public static void OutputTreeJson(string data)
    {
        logger?.LogDebug($"Tree JSON: {data}");
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

     public static void AddingTagsObject(string name, string hash)
    {
        logger?.LogDebug($"Adding Tag object to our list of Tag Nodes: Name={name} PointingTO={hash}");
    }

    


    public static void ExistingBlobObjectUpdate(string hash, string filename, string parentTree)
    {
        logger?.LogDebug($"Updating Blob object to our list of BLOB Nodes: hash={hash} name={filename} parentTree={parentTree}");

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