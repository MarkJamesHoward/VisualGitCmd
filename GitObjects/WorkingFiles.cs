using Neo4j.Driver;
using OpenTelemetry.Trace;
public abstract class GitWorkingFiles
{
    public static List<WorkingFile> WorkingFiles = new List<WorkingFile>();

    public static List<WorkingFile> ProcessWorkingFiles(string workingFolder)
    {
        List<string> files = FileType.GetWorkingFiles(workingFolder);
        WorkingFiles = new List<WorkingFile>();

        foreach (string file in files)
        {
            WorkingFile FileObj = new WorkingFile();
            FileObj.filename = file;
            if (GlobalVars.PerformTextExtraction)
            {
                FileObj.contents = FileType.GetFileContents(Path.Combine(workingFolder, file));
            }
            WorkingFiles.Add(FileObj);
        }
        return WorkingFiles;
    }


}
