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

    public void RunRegExAgainstCommit(string commitHashCode)
    {
        commitContents = FileType.GetContents(commitHashCode, GlobalVars.workingArea);

        commitTreeDetails = Regex.Match(commitContents, "tree ([0-9a-f]{4})");
        commitParentDetails = Regex.Matches(commitContents, "parent ([0-9a-f]{4})");
        commitCommentDetails = Regex.Match(commitContents, "\n\n(.+)\n");
    }
}