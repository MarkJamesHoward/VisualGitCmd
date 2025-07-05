using Neo4j.Driver;

public abstract class GitTags
{
    public static List<Tag> Tags = new List<Tag>();

    public static void CreateTagsObject(string name, string hash)
    {
        Tag b = new Tag
        {
            hash = hash,
            name = name
        };

        if (!Tags.Exists(i => i.name == b.name))
        {
            DebugMessages.AddingTagsObject(b.name, b.hash);
            Tags.Add(b);
        }
    }

    public static void ProcessTags(ISession? session)
    {
        Tags = new List<Tag>();

        List<string> TagsFiles = Directory.GetFiles(GlobalVars.tagPath).ToList();

        // Add the Tags
        foreach (var file in TagsFiles)
        {
            var TagsHash = File.ReadAllText(file);
            if (GlobalVars.EmitNeo)
            {
                //Neo4jHelper.AddTagsToNeo(session, Path.GetFileName(file), TagsHash); TODO
                //Neo4jHelper.CreateTagsLinkNeo(session, Path.GetFileName(file), TagsHash.Substring(0, 4)); TODO
            }
            CreateTagsObject(Path.GetFileName(file), TagsHash.Substring(0, 4));
        }
    }
}
