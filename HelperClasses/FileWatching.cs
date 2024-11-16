

public abstract class FileWatching
{
    static bool BatchingUpFileChanges = false;
    static object MainLockObj = new Object();
    static int batch = 1;


    public static void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (
            (e?.Name?.Contains(".lock", StringComparison.CurrentCultureIgnoreCase) ?? false) ||
            (e?.Name?.Contains("tmp", StringComparison.CurrentCultureIgnoreCase) ?? false)
        )
        {
            return;
        }

        if (!BatchingUpFileChanges)
        {
            BatchingUpFileChanges = true;

            var t = Task.Run(delegate
            {
                lock (MainLockObj)
                {
                    batch++;

                    Console.WriteLine($"Batch {batch} Waiting for file changes to complete.....");
                    Thread.Sleep(2000);
                    BatchingUpFileChanges = false;

                    Console.WriteLine($"Batch {batch} Processing.....");
                    GitRepoExaminer.Run();
                    Console.WriteLine($"Batch {batch} Completed.....");
                }

            });

        }
        else
        {
            //Console.WriteLine($"Batch {batch} batching " + e.Name);
        }
    }
    public delegate void OnChangedDelegate(object sender, FileSystemEventArgs e);

    private static OnChangedDelegate handler = OnChanged;

    public static void CreateFileWatcher()
    {
        using var watcher = new FileSystemWatcher(GlobalVars.RepoPath);
        {
            watcher.NotifyFilter = NotifyFilters.Attributes
                                    | NotifyFilters.CreationTime
                                    | NotifyFilters.DirectoryName
                                    | NotifyFilters.FileName
                                    | NotifyFilters.LastAccess
                                    | NotifyFilters.LastWrite
                                    | NotifyFilters.Security
                                    | NotifyFilters.Size;

            watcher.Changed += handler.Invoke;
            watcher.Created += handler.Invoke;
            watcher.Deleted += handler.Invoke;
            watcher.Renamed += handler.Invoke;

            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.ReadLine();
        }
    }
}
