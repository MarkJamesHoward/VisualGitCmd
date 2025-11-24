public abstract class FilePath
{
    public static void GetExeFilePath()
    {
        GlobalVars.exePath =
            Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName) ?? "";
        StandardMessages.ExePath(GlobalVars.exePath);
    }

    /// <summary>
    /// Expands the tilde (~) path to the user's home directory on Linux/macOS
    /// </summary>
    /// <param name="path">The path potentially starting with ~</param>
    /// <returns>The expanded path</returns>
    public static string ExpandTildePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        // Only expand if path starts with ~
        if (path.StartsWith("~"))
        {
            // Get the home directory
            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (string.IsNullOrEmpty(homeDirectory))
            {
                // Fallback to HOME environment variable on Linux/macOS
                homeDirectory = Environment.GetEnvironmentVariable("HOME") ?? "";
            }

            if (!string.IsNullOrEmpty(homeDirectory))
            {
                // Replace ~ with home directory
                if (path.Length == 1)
                {
                    // Path is just "~"
                    return homeDirectory;
                }
                else if (path.Length > 1 && (path[1] == '/' || path[1] == '\\'))
                {
                    // Path is "~/something"
                    return Path.Combine(homeDirectory, path.Substring(2));
                }
            }
        }

        return path;
    }
}
