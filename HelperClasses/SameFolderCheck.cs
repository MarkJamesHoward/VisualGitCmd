public abstract class SameFolderCheck
{
    public static void Validate()
    {
        // Remeber best not to run VisualGit in the repo being examined
        // This is only a real issue when content extraction is enabled
        if (GlobalVars.exePath == GlobalVars.RepoPath && GlobalVars.PerformTextExtraction)
        {
            StandardMessages.SameFolderMessage();
            Application.End();
        }
    }
}