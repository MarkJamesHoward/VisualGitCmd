public abstract class MyLogging
{
    public static ILogger? logger;
    public static void CreateLogger()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder =>
        builder.AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddFilter("Program", LogLevel.Debug)
        .AddConsole());
        logger = factory.CreateLogger("Program");
    }
}