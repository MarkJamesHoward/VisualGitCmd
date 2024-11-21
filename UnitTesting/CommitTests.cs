using Xunit;

public class CommitTests
{
    [Fact]
    public void SingleCommit()
    {
        // GlobalVars.RepoPath = "..\\..\\..\\UnitTesting\\SingleCommit";
        // GlobalVars.branchPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/heads");
        Console.WriteLine("*********************TEST**********************");
        GlobalVars.GITobjectsPath = @"C:\github\VisualGitCmd\UnitTesting\Commits\SingleCommit\.git/objects";

        GitRepoExaminer.ProcessEachFileAndExtract_Commit_Tree_Blob(GlobalVars.GITobjectsPath);

        Assert.Collection(GitCommits.CommitNodes, n =>
        {
            Assert.IsType<CommitNode>(n);
            Assert.Equal("Initial", n.text);
        });
    }
}