public abstract class WorkingAreaFilesJson
{

    public static void OutputWorkingFilesJsonToFile(string workingFolder, string JsonPath)
    {
        var Json = string.Empty;
        List<WorkingFile> WorkingFilesList = new List<WorkingFile>();

        List<string> files = FileType.GetWorkingFiles(workingFolder);

        foreach (string file in files)
        {
            WorkingFile FileObj = new WorkingFile();
            FileObj.filename = file;
            FileObj.contents = FileType.GetFileContents(Path.Combine(workingFolder, file));
            WorkingFilesList.Add(FileObj);
        }

        Json = JsonSerializer.Serialize(WorkingFilesList);
        File.WriteAllText(JsonPath, Json);
    }
}