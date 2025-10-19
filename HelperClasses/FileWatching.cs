

public abstract class FileWatching
{
    static bool BatchingUpFileChanges = false;
    static object MainLockObj = new Object();
    static int batch = 1;

     private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }

public static bool IgnoreFiles(string filename) {
     if (
            (filename?.Contains(".lock", StringComparison.CurrentCultureIgnoreCase) ?? false) ||
            (filename?.Contains("tmp", StringComparison.CurrentCultureIgnoreCase) ?? false) 
        )
        {
            return true;
        }
        else
        {
            return false;
        }
}
public static void ProcessUpdates(object sender, FileSystemEventArgs? e)
{
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
       

}
  public static void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (IgnoreFiles(e?.Name ?? "") == false)
        {
            DebugMessages.FileCreated(e?.Name);
            ProcessUpdates(sender, e);
        }
        else
        {
            DebugMessages.IgnoringFile(e?.Name);
        }               
    }

    public static void OnChanged(object sender, FileSystemEventArgs e)
    {       
        if (IgnoreFiles(e?.Name ?? "") == false)
        {
            DebugMessages.FileChanged(e?.Name);
            ProcessUpdates(sender, e);  
        }
        else
        {
            DebugMessages.IgnoringFile(e?.Name);
        }

    }

    public static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (IgnoreFiles(e?.Name ?? "") == false)
        {
        DebugMessages.FileDeleted(e?.Name);
        ProcessUpdates(sender, e);  
        }
            else
        {
            DebugMessages.IgnoringFile(e?.Name);
        }
    }
    public static void OnRenamed(object sender, RenamedEventArgs  e)
    {        
        if (IgnoreFiles(e?.Name ?? "") == false)
        {
        DebugMessages.FileRenamed(e?.Name, e?.OldName);
        ProcessUpdates(sender, e);  
        }
            else
        {
            DebugMessages.IgnoringFile(e?.Name);
        }
    }
    public delegate void OnChangedDelegate(object sender, FileSystemEventArgs e);
    public delegate void OnCreatedDelegate(object sender, FileSystemEventArgs e);
    public delegate void OnDeletedDelegate(object sender, FileSystemEventArgs e);
    public delegate void OnRenamedDelegate(object sender, RenamedEventArgs e);
    public delegate void OnErrorDelegate(object sender, ErrorEventArgs e);

    private static OnChangedDelegate ChangedHandler = OnChanged;
    private static OnCreatedDelegate CreatedHandler = OnCreated;
    private static OnDeletedDelegate DeletedHandler = OnDeleted;
    private static OnRenamedDelegate RenamedHandler = OnRenamed;
    private static OnErrorDelegate ErrorHandler = OnError;


    public static void CreateFileWatcher()
    {
        using var watcher = new FileSystemWatcher(GlobalVars.RepoPath);
        {
            // watcher.NotifyFilter = NotifyFilters.Attributes
            //                         | NotifyFilters.CreationTime
            //                         | NotifyFilters.DirectoryName
            //                         | NotifyFilters.FileName
            //                         | NotifyFilters.LastAccess
            //                         | NotifyFilters.LastWrite
            //                         | NotifyFilters.Security
            //                         | NotifyFilters.Size;

             watcher.NotifyFilter = NotifyFilters.CreationTime |
                                    NotifyFilters.FileName |
                                     NotifyFilters.LastWrite;


            watcher.Changed += ChangedHandler.Invoke;
            watcher.Created += CreatedHandler.Invoke;
            watcher.Deleted += DeletedHandler.Invoke;
            watcher.Renamed += RenamedHandler.Invoke;
            watcher.Error += ErrorHandler.Invoke;

            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.ReadLine();
        }
    }
}
