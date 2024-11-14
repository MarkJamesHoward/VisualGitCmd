public abstract class FilePath
{
    public static void GetExeFilePath()
    {
        GlobalVars.exePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName) ?? "";
        StandardMessages.ExePath(GlobalVars.exePath);
    }
}