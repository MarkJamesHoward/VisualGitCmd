public static class GlobalExceptionHandler
{
    public static void Initialize()
    {
        // Handle unhandled exceptions in the main application domain
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // Handle unhandled exceptions in tasks
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogException(ex, "Unhandled Exception");
        }
    }

    private static void OnUnobservedTaskException(
        object? sender,
        UnobservedTaskExceptionEventArgs e
    )
    {
        LogException(e.Exception, "Unobserved Task Exception");
        e.SetObserved(); // Prevent the process from terminating
    }

    private static void LogException(Exception ex, string exceptionType)
    {
        Console.WriteLine($"\n{exceptionType} occurred:");
        Console.WriteLine($"Message: {ex.Message}");

        if (GlobalVars.debug)
        {
            Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");

            // Get file and line number from the exception
            var stackTrace = new System.Diagnostics.StackTrace(ex, true);
            var frame = stackTrace.GetFrame(0);
            if (frame != null)
            {
                var fileName = frame.GetFileName();
                var lineNumber = frame.GetFileLineNumber();

                if (!string.IsNullOrEmpty(fileName) && lineNumber > 0)
                {
                    Console.WriteLine($"\nFile: {fileName}, Line: {lineNumber}");
                }
            }

            // Show inner exceptions if any
            if (ex.InnerException != null)
            {
                Console.WriteLine("\nInner Exception:");
                LogException(ex.InnerException, "Inner");
            }
        }
        else
        {
            Console.WriteLine("Run with debug mode (-d) for detailed exception information.");
        }
    }
}
