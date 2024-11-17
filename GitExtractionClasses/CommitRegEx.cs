using System.ComponentModel.DataAnnotations;

public class CommitNodeExtraction
{
    public Match commitTreeDetails { get; set; }
    public MatchCollection commitParentDetails { get; set; }
    public Match commitCommentDetails { get; set; }
    public string commitContents { get; set; }

    public CommitNodeExtraction(string commitHashCode)
    {
        RunRegExAgainstCommit(commitHashCode);
    }

    public List<string> GetParentCommits(string hashCode_determinedFrom_dir_and_first2charOfFilename)
    {
        List<string> commitParentHashes = new List<string>();

        foreach (Match commitParentMatch in commitParentDetails)
        {
            string parentHash = commitParentMatch.Groups[1].Value;
            commitParentHashes.Add(parentHash);
            StandardMessages.ParentCommitHashCode(hashCode_determinedFrom_dir_and_first2charOfFilename, parentHash);
        }
        return commitParentHashes;
    }

    public void RunRegExAgainstCommit(string commitHashCode)
    {
        commitContents = FileType.GetContents(commitHashCode, GlobalVars.workingArea);

        commitTreeDetails = Regex.Match(commitContents, "tree ([0-9a-f]{4})");
        commitParentDetails = Regex.Matches(commitContents, "parent ([0-9a-f]{4})");
        commitCommentDetails = Regex.Match(commitContents, "\n\n(.+)\n");
    }
}