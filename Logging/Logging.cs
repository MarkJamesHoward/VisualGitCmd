public abstract class MyLogging
{
    public static ILogger? logger;
    public static void CreateLogger()
    {
        if (GlobalVars.debug)
        {
            using ILoggerFactory factory = LoggerFactory.Create(builder =>
            builder.AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)

            .AddFilter("Program", LogLevel.Debug)

            .AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }));
            logger = factory.CreateLogger("Program");

        }
        else
        {
            using ILoggerFactory factory = LoggerFactory.Create(builder =>
            builder.AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddFilter("Program", LogLevel.Information)
            .AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            }));
            logger = factory.CreateLogger("Program");
        }
    }
}