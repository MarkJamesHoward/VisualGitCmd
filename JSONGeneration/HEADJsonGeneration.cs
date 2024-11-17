public abstract class HEADJsonGeneration
{



    public static void OutputHEADJson(string JsonPath, string path)
    {
        string HeadContents = File.ReadAllText(Path.Combine(GlobalVars.GITobjectsPath, "HEAD"));
        //Console.WriteLine("Outputting JSON HEAD");
        string HEADHash = "";

        // Is the HEAD detached in which case it contains a Commit Hash
        Match match = Regex.Match(HeadContents, "[0-9a-f]{40}");
        if (match.Success)
        {
            //Console.WriteLine("Outputting JSON HEAD match found 1");
            HEADHash = match.Value.Substring(0, 4);
        }
        match = Regex.Match(HeadContents, @"ref: refs/heads/(\w+)");
        if (match.Success)
        {
            //Console.WriteLine("Outputting JSON HEAD match found 2");

            //Console.WriteLine("HEAD Branch extract: " + match.Groups[1]?.Value);
            HEADHash = match.Groups[1].Value;
            //CreateHEADTOBranchLinkNeo(session, branch);
        }
        HEADNode h = new HEADNode();
        h.hash = HEADHash;

        var Json = string.Empty;
        Json = JsonSerializer.Serialize(h);

        //Console.WriteLine(Json);
        File.WriteAllText(JsonPath, Json);
    }
}