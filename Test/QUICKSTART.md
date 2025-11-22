# Quick Start: Docker Integration Tests

## Step-by-Step Execution

### 1. Generate Docker Output
```powershell
# Build the Docker image (if not already built)
docker build -t visualgit -f Docker/DockerFileLinuxTest .

# Start container to generate output
docker-compose up -d

# Execute the application inside the container to generate JSON files
# (Modify based on your actual execution method)
docker exec -it <container-name> dotnet run --no-launch-profile -- -p /App/docker-test-folder -j /App/docker-test-folder/output
```

### 2. Run Tests
```powershell
# Run all integration tests
dotnet test --filter "DockerOutputIntegrationTests"

# Or run the aggregate test for quick validation
dotnet test --filter "DockerOutput_AllFilesShouldMatchExpectedJson"
```

### 3. Review Results
- ✅ **All Pass**: Docker output matches expected baseline
- ❌ **Failures**: Review differences and update either code or expected files

## Expected Output Location
- **Expected Files**: `Test/ExpectedJson/*.json`
- **Docker Output**: `c:\dev\output/*.json` (mapped via docker-compose volume)

## Files Being Compared
1. BlobGitInJson.json
2. CommitGitInJson.json
3. IndexfilesGitInJson.json
4. TreeGitInJson.json
