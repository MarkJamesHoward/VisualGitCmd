using System.Text.Json;
using Xunit;

namespace VisualGit.Tests;

/// <summary>
/// Integration tests that verify Docker-generated JSON output matches expected results.
/// Tests compare files from Test/ExpectedJson with output from Docker containers at c:\dev\output.
/// </summary>
public class DockerOutputIntegrationTests
{
    private const string ActualOutputPath = @"d:\github\visualgitcmd\Test\ActualJson";

    private readonly string[] _jsonFilesToCompare =
    {
        "BlobGitInJson.json",
        "CommitGitInJson.json",
        "IndexfilesGitInJson.json",
        "TreeGitInJson.json",
    };

    private static string GetProjectRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();

        // Navigate up from bin/Debug/net9.0 to project root
        while (!File.Exists(Path.Combine(currentDir, "visual.csproj")))
        {
            var parent = Directory.GetParent(currentDir);
            if (parent == null)
                throw new InvalidOperationException(
                    "Could not find project root (visual.csproj not found)"
                );
            currentDir = parent.FullName;
        }

        return currentDir;
    }

    private static string GetExpectedJsonPath()
    {
        return Path.Combine(GetProjectRoot(), "Test", "ExpectedJson");
    }

    [Theory]
    [InlineData("BlobGitInJson.json")]
    [InlineData("CommitGitInJson.json")]
    [InlineData("IndexfilesGitInJson.json")]
    [InlineData("TreeGitInJson.json")]
    public void DockerOutput_ShouldMatchExpectedJson(string fileName)
    {
        // Arrange
        var expectedFilePath = Path.Combine(GetExpectedJsonPath(), fileName);
        var actualFilePath = Path.Combine(ActualOutputPath, fileName);

        // Assert files exist
        Assert.True(
            File.Exists(expectedFilePath),
            $"Expected JSON file not found: {expectedFilePath}"
        );
        Assert.True(
            File.Exists(actualFilePath),
            $"Actual output file not found: {actualFilePath}. Ensure Docker container has generated the output."
        );

        // Act - Read and parse JSON files
        var expectedJson = File.ReadAllText(expectedFilePath);
        var actualJson = File.ReadAllText(actualFilePath);

        // Assert - Compare JSON content (normalized)
        AssertJsonEquals(expectedJson, actualJson, fileName);
    }

    [Fact]
    public void DockerOutput_AllExpectedFilesPresent()
    {
        // Arrange
        var missingFiles = new List<string>();

        // Act
        foreach (var fileName in _jsonFilesToCompare)
        {
            var actualFilePath = Path.Combine(ActualOutputPath, fileName);
            if (!File.Exists(actualFilePath))
            {
                missingFiles.Add(fileName);
            }
        }

        // Assert
        Assert.Empty(missingFiles);
    }

    [Fact]
    public void ExpectedJson_AllFilesPresent()
    {
        // Arrange
        var missingFiles = new List<string>();

        // Act
        foreach (var fileName in _jsonFilesToCompare)
        {
            var expectedFilePath = Path.Combine(GetExpectedJsonPath(), fileName);
            if (!File.Exists(expectedFilePath))
            {
                missingFiles.Add(fileName);
            }
        }

        // Assert
        Assert.Empty(missingFiles);
    }

    /// <summary>
    /// Compares two JSON strings by parsing and normalizing them to handle formatting differences.
    /// </summary>
    private void AssertJsonEquals(string expectedJson, string actualJson, string fileName)
    {
        try
        {
            // Parse JSON to normalize formatting
            using var expectedDoc = JsonDocument.Parse(expectedJson);
            using var actualDoc = JsonDocument.Parse(actualJson);

            // Serialize back to normalized format for comparison
            var expectedNormalized = JsonSerializer.Serialize(
                expectedDoc,
                new JsonSerializerOptions { WriteIndented = false }
            );
            var actualNormalized = JsonSerializer.Serialize(
                actualDoc,
                new JsonSerializerOptions { WriteIndented = false }
            );

            Assert.Equal(expectedNormalized, actualNormalized);
        }
        catch (JsonException ex)
        {
            Assert.Fail($"Invalid JSON in {fileName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Integration test that compares all files in a single test.
    /// Use this for a quick verification that all files match.
    /// </summary>
    [Fact]
    public void DockerOutput_AllFilesShouldMatchExpectedJson()
    {
        // Arrange
        var failedComparisons = new List<string>();

        // Act
        foreach (var fileName in _jsonFilesToCompare)
        {
            var expectedFilePath = Path.Combine(GetExpectedJsonPath(), fileName);
            var actualFilePath = Path.Combine(ActualOutputPath, fileName);

            if (!File.Exists(expectedFilePath))
            {
                failedComparisons.Add($"{fileName}: Expected file missing");
                continue;
            }

            if (!File.Exists(actualFilePath))
            {
                failedComparisons.Add($"{fileName}: Actual output file missing");
                continue;
            }

            try
            {
                var expectedJson = File.ReadAllText(expectedFilePath);
                var actualJson = File.ReadAllText(actualFilePath);

                using var expectedDoc = JsonDocument.Parse(expectedJson);
                using var actualDoc = JsonDocument.Parse(actualJson);

                var expectedNormalized = JsonSerializer.Serialize(
                    expectedDoc,
                    new JsonSerializerOptions { WriteIndented = false }
                );
                var actualNormalized = JsonSerializer.Serialize(
                    actualDoc,
                    new JsonSerializerOptions { WriteIndented = false }
                );

                if (expectedNormalized != actualNormalized)
                {
                    failedComparisons.Add($"{fileName}: Content mismatch");
                }
            }
            catch (Exception ex)
            {
                failedComparisons.Add($"{fileName}: {ex.Message}");
            }
        }

        // Assert
        Assert.Empty(failedComparisons);
    }
}
