using Xunit;

public class BranchTests
{
    [Fact]
    public void TwoBranches()
    {
        // GlobalVars.RepoPath = "..\\..\\..\\UnitTesting\\SingleCommit";
        // GlobalVars.branchPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/heads");
        Console.WriteLine("*********************TEST**********************");
        GlobalVars.branchPath = @"C:\github\VisualGitCmd\UnitTesting\Branches\TwoBranches\.git/refs/heads";

        GitBranches.ProcessBranches(null);

        Assert.Collection(GitBranches.branches, b =>
        {
            Assert.IsType<Branch>(b);
            Assert.Equal("BranchTwo", b.name);
        },
        b =>
        {
            Assert.IsType<Branch>(b);
            Assert.Equal("master", b.name);
        });
    }

    [Fact]
    public void OneBranch()
    {
        // GlobalVars.RepoPath = "..\\..\\..\\UnitTesting\\SingleCommit";
        // GlobalVars.branchPath = Path.Combine(GlobalVars.RepoPath, @".git/refs/heads");
        Console.WriteLine("*********************TEST**********************");
        GlobalVars.branchPath = @"C:\github\VisualGitCmd\UnitTesting\Branches\OneBranch\.git/refs/heads";

        GitBranches.ProcessBranches(null);

        Assert.Collection(GitBranches.branches, b =>
        {
            Assert.IsType<Branch>(b);
            Assert.Equal("master", b.name);
        });
    }
}