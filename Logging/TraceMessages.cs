using static MyLogging;
public abstract class TraceMessages
{
    public static void RunningCatFile(string file)
    {
        logger?.LogTrace($"Running Cat File: cmd='git cat-file {file} -t'");
    }
    public static void AddingOrphanBlobsToJson()
    {
        logger?.LogTrace($"Adding orphan blobs to json");
    }
}