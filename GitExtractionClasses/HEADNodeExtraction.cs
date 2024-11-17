public abstract class HEADNodeExtraction
{
    public static HEADNode GetHeadNodeFromPathAndDetermineWhereItPoints()
    {
        string HeadContents = File.ReadAllText(Path.Combine(GlobalVars.headPath, "HEAD"));
        //Console.WriteLine("Outputting JSON HEAD");
        string HEADHash = "";

        // Is the HEAD detached in which case it contains a Commit Hash
        Match match = Regex.Match(HeadContents, "[0-9a-f]{40}");
        if (match.Success)
        {
            HEADHash = match.Value.Substring(0, 4);
        }
        match = Regex.Match(HeadContents, @"ref: refs/heads/(\w+)");
        if (match.Success)
        {
            HEADHash = match.Groups[1].Value;
            //CreateHEADTOBranchLinkNeo(session, branch);
        }
        HEADNode h = new HEADNode();
        h.hash = HEADHash;

        DebugMessages.HeadPointingTo(h.hash);
        return h;

    }
}