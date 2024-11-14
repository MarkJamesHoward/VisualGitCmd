public abstract class StandardMessages()
{

    public static void DisplayVersion()
    {
        Console.WriteLine($"Version {GlobalVars.version} - Ensure matches against website for compatibility");
    }

    public static void SameFolderMessage()
    {
        Console.WriteLine("VisualGit should not be run in the same folder as the Repository to be examined");
        Console.WriteLine("Option1: Place Visual.exe into another folder and run with --p pointing to this folder");
        Console.WriteLine("Option2: Place the Visual.exe application into a folder on your PATH. Then just run Visual from within the Repository as you just did");
    }

    public static void ExePath(string path)
    {
        Console.WriteLine($"Exe Path {path}");
    }

}