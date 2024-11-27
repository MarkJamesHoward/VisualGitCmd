using Neo4j.Driver;
using OpenTelemetry.Trace;
public abstract class GitWorkingFiles
{
    public static List<WorkingFile> WorkingFiles = new List<WorkingFile>();


    public static void ProcessWorkingFiles(string workingFolder)
    {
        List<WorkingFile> files = InternalProcessWorkingFiles(workingFolder);
        WorkingFiles = files;
    }

    private static List<WorkingFile> InternalProcessWorkingFiles(string workingFolder)
    {
        List<WorkingFile> LocalWorkingFiles = new List<WorkingFile>();

        string[] directories = Directory.GetDirectories(workingFolder);

        foreach (string folder in directories)
        {
            if (!folder.Contains(".git"))
            {
                List<WorkingFile> subFolderFiles = InternalProcessWorkingFiles(folder);
                foreach (var file in subFolderFiles)
                {
                    LocalWorkingFiles.Add(file);
                }
            }
        }

        List<string> files = FileType.GetWorkingFiles(workingFolder);

        foreach (string file in files)
        {
            WorkingFile FileObj = new WorkingFile();
            FileObj.filename = file;
            if (GlobalVars.PerformTextExtraction)
            {
                FileObj.contents = FileType.GetFileContents(Path.Combine(workingFolder, file));
            }
            LocalWorkingFiles.Add(FileObj);
        }
        return LocalWorkingFiles;
    }


}
