

using MyProjectl;

namespace MyProject
{

    public abstract class FileWatching {

        public delegate void OnChangedDelegate(object sender, FileSystemEventArgs e);

        public static void CreateFileWatcher(OnChangedDelegate OnChanged) {
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

                watcher.Changed += OnChanged.Invoke;
                watcher.Created += OnChanged.Invoke;
                watcher.Deleted += OnChanged.Invoke;
                watcher.Renamed += OnChanged.Invoke;
                //watcher.Error += OnError;

                watcher.Filter = "*.*";
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;

                Console.ReadLine();
            }
        }
    }
}