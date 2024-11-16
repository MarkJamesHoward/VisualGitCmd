public abstract class Application
{
    public static void End()
    {
        throw new Exception("Visual Git should not be run in the same folder as the Repo to examine");
    }
}