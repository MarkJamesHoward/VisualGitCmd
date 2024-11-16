using Neo4j.Driver;
public class Nodes
{

    public static List<WorkingFile> WorkingFilesNodes(string workingFolder)
    {

        List<string> files = FileType.GetWorkingFiles(workingFolder);
        List<WorkingFile> WorkingFilesList = new List<WorkingFile>();


        foreach (string file in files)
        {
            WorkingFile FileObj = new WorkingFile();
            FileObj.filename = file;
            if (GlobalVars.PerformTextExtraction)
            {
                FileObj.contents = FileType.GetFileContents(Path.Combine(workingFolder, file));
            }
            WorkingFilesList.Add(FileObj);
        }
        return WorkingFilesList;
    }


}
