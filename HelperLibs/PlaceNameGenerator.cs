using RandomNameGeneratorLibrary;


public abstract class RandomName()
{
    public static string Name { get; set; }

    public static void GenerateRandomName()
    {
        PlaceNameGenerator randomNameGenerator = new PlaceNameGenerator();
        Name = randomNameGenerator.GenerateRandomPlaceName();
    }
}
