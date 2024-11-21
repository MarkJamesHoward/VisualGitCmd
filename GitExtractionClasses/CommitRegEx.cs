using System.ComponentModel.DataAnnotations;

public class CommitNodeExtraction
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Match CommitTreeDetails { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public MatchCollection CommitParentDetails { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Match CommitCommentDetails { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string CommitContents { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public List<string> GetParentCommits(string hashCode_determinedFrom_dir_and_first2charOfFilename)
    {
        List<string> commitParentHashes = new List<string>();

        if (CommitParentDetails != null)
        {
            foreach (Match commitParentMatch in CommitParentDetails)
            {
                string parentHash = commitParentMatch.Groups[1].Value;
                commitParentHashes.Add(parentHash);
                StandardMessages.ParentCommitHashCode(hashCode_determinedFrom_dir_and_first2charOfFilename, parentHash);
            }
        }
        return commitParentHashes;
    }

    public void RunRegExAgainstCommit(string commitHashCode)
    {
        CommitContents = FileType.GetContents(commitHashCode, GlobalVars.workingArea);

        CommitTreeDetails = Regex.Match(CommitContents, "tree ([0-9a-f]{4})");
        CommitParentDetails = Regex.Matches(CommitContents, "parent ([0-9a-f]{4})");
        CommitCommentDetails = Regex.Match(CommitContents, "\n\n(.+)\n");
    }
}