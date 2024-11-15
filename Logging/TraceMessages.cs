using static MyLogging;
public abstract class TraceMessages
{
    public static void AddingOrphanBlobsToJson()
    {
        logger?.LogTrace($"Adding orphan blobs to json");
    }
}