public abstract class SameFolderCheck
{
    public static void Validate()
    {
        // Remeber best not to run VisualGit in the repo being examined
        if (GlobalVars.exePath == GlobalVars.RepoPath)
        {
            StandardMessages.SameFolderMessage();
            Application.End();
        }
    }
}