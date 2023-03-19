using System.Diagnostics;
using System.Text.RegularExpressions;
using Neo4j.Driver;

string head = @".git\";
string path = @".git\objects\";
string branchPath = @".git\refs\heads";

List<string> HashCodeFilenames = new List<string>();

// Get all the files in the .git/objects folder
try
{
    List<string> branchFiles = Directory.GetFiles(branchPath).ToList();
    List<string> directories = Directory.GetDirectories(path).ToList();
    List<string> files = new List<string>();

    ClearExistingNodesInNeo();

    foreach (string dir in directories)
    {
        files = Directory.GetFiles(dir).ToList();

        foreach (string file in files)
        {
            string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);

            HashCodeFilenames.Add(hashCode);

            string fileType = GetFileType(hashCode);


            if (fileType.Contains("commit"))
            {
                Console.WriteLine($"{fileType.TrimEnd('\n', '\r')} {hashCode}");

                string commitContents = GetContents(hashCode);
                var match = Regex.Match(commitContents, "tree ([0-9a-f]{4})");
                var commitParent = Regex.Match(commitContents, "parent ([0-9a-f]{4})");
                var commitComment = Regex.Match(commitContents, "\n\n(.+)\n");

                if (match.Success)
                {
                    // Get details of the tree,parent and comment in this commit
                    string treeHash = match.Groups[1].Value;
                    Console.WriteLine($"\t-> tree {treeHash}");

                    string parentHash = commitParent.Groups[1].Value;
                    Console.WriteLine($"\t-> parent commit {commitParent}");

                    string comment = commitComment.Groups[1].Value;
                    comment = comment.Trim();

                    AddCommitToNeo(comment, hashCode, commitContents);

                    if (!DoesNodeExistAlready(treeHash, "tree"))
                    {
                        AddTreeToNeo(treeHash, GetContents(treeHash));
                    }
                    CreateCommitLinkNeo(hashCode, treeHash, "", "");


                    // Get the details of the Blobs in this Tree
                    string tree = GetContents(match.Groups[1].Value);
                    var blobsInTree = Regex.Matches(tree, @"blob ([0-9a-f]{4})[0-9a-f]{36}.([\w\.]+)");

                    foreach (Match blobMatch in blobsInTree)
                    {
                        string blobHash = blobMatch.Groups[1].Value;
                        string blobContents = GetContents(blobHash);

                        Console.WriteLine($"\t\t-> blob {blobHash} {blobMatch.Groups[2]}");
                        if (!DoesNodeExistAlready(blobHash, "blob"))
                        {
                            AddBlobToNeo(blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, blobContents);
                        }
                        if (!DoesTreeToBlobLinkExist(match.Groups[1].Value, blobHash))
                        {
                            CreateLinkNeo(match.Groups[1].Value, blobMatch.Groups[1].Value, "", "");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No Tree found in Commit");
                }
            }
        }

    }
    // Add the Branches
    foreach (var file in branchFiles)
    {
        var branchHash = await File.ReadAllTextAsync(file);
        AddBranchToNeo(Path.GetFileName(file), branchHash);
        CreateBranchLinkNeo(Path.GetFileName(file), branchHash.Substring(0, 4));
    }

    AddCommitParentLinks(path);
    AddOrphanBlobs(branchPath, path);
    GetHEAD(head);
}
catch (Exception e)
{
    Console.WriteLine($"Error while getting files in {path} {e.Message}");
}

static void GetHEAD(string path) 
{
    string HeadContents = File.ReadAllText(Path.Combine(path, "HEAD"));

    // Is the HEAD detached in which case it contains a Commit Hash
    Match match = Regex.Match(HeadContents, "[0-9a-f]{40}");
    if (match.Success) {
        string HEADHash = match.Value.Substring(0, 4);
        //Create the HEAD Node
        AddHeadToNeo(HEADHash, HeadContents);
        //Create Link to Commit
        CreateHEADTOCommitLinkNeo(HEADHash);
    }

    match = Regex.Match(HeadContents, @"ref: refs/heads/(\w+)");
    if (match.Success) {
        Console.WriteLine("HEAD Branch extract: " + match.Groups[1]?.Value);
        string branch = match.Groups[1].Value;
         //Create the HEAD Node
        AddHeadToNeo(branch, HeadContents);
        //Create Link to Commit
        CreateHEADTOBranchLinkNeo(branch);
    }


}



static bool DoesTreeToBlobLinkExist(string treeHash, string blobHash)
{

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    string query = "MATCH (t:tree { hash: $treeHash })-[r:blob]->(b:blob {hash: $blobHash }) RETURN r, b";
    var result = session.Run(
            query,
            new { treeHash, blobHash });

    foreach (var record in result)
    {
        return true;
    }

    return false;
}

static void AddOrphanBlobs(string branchPath, string path)
{

    List<string> branchFiles = Directory.GetFiles(branchPath).ToList();
    List<string> directories = Directory.GetDirectories(path).ToList();
    List<string> files = new List<string>();

    foreach (string dir in directories)
    {
        files = Directory.GetFiles(dir).ToList();

        foreach (string file in files)
        {
            string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);
            string fileType = GetFileType(hashCode);

            if (fileType.Contains("blob"))
            {
                string blobContents = GetContents(hashCode);

                Console.WriteLine($"blob {hashCode}");
                if (!DoesNodeExistAlready(hashCode, "blob"))
                {
                    AddBlobToNeo(hashCode, hashCode, blobContents);
                }
            }
        }
    }
}


static void AddCommitParentLinks(string path)
{
    List<string> directories = Directory.GetDirectories(path).ToList();

    foreach (string dir in directories)
    {
        var files = Directory.GetFiles(dir).ToList();

        foreach (string file in files)
        {

            string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);
            string fileType = GetFileType(hashCode);

            if (fileType.Contains("commit"))
            {
                string commitContents = GetContents(hashCode);
                var commitParent = Regex.Match(commitContents, "parent ([0-9a-f]{4})");

                if (commitParent.Success)
                {
                    string parentHash = commitParent.Groups[1].Value;
                    Console.WriteLine($"\t-> parent commit {commitParent}");

                    CreateCommitTOCommitLinkNeo(hashCode, parentHash);
                }
            }
        }
    }
}

static void ClearExistingNodesInNeo()
{
    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (n) DETACH DELETE n",
            new { });

        return result;
    });
}

static void CreateLinkNeo(string parent, string child, string parentType, string childType)
{

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (t:tree), (b:blob) WHERE t.hash ='{parent}' AND b.hash ='{child}' CREATE (t)-[blob_link:blob]->(b) RETURN type(blob_link)",
            new { });

        return result;
    });
}

static bool CreateHEADTOBranchLinkNeo(string branchName)
{

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    Console.WriteLine("HEAD -> " + branchName);
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (t:HEAD), (b:branch) WHERE t.name ='HEAD' AND b.name ='{branchName}' CREATE (t)-[head_link:HEAD]->(b) RETURN type(head_link)",
            new { });

        return result.Count();
    });

    return greeting > 0 ? true : false;
}

static bool CreateHEADTOCommitLinkNeo(string childCommit)
{

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    Console.WriteLine("HEAD -> " + childCommit);
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (t:HEAD), (b:commit) WHERE t.name ='HEAD' AND b.hash ='{childCommit}' CREATE (t)-[head_link:HEAD]->(b) RETURN type(head_link)",
            new { });

        return result.Count();
    });

    return greeting > 0 ? true : false;
}

static bool CreateCommitTOCommitLinkNeo(string parent, string child)
{

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (t:commit), (b:commit) WHERE t.hash ='{parent}' AND b.hash ='{child}' CREATE (t)-[parent_link:parent]->(b) RETURN type(parent_link)",
            new { });

        return result.Count();
    });

    return greeting > 0 ? true : false;
}

static bool CreateCommitLinkNeo(string parent, string child, string parentType, string childType)
{
    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (t:commit), (b:tree) WHERE t.hash ='{parent}' AND b.hash ='{child}' CREATE (t)-[tree_link:tree]->(b) RETURN type(tree_link)",
            new { });

        return result.Count();
    });

    return greeting > 0 ? true : false;
}

static bool CreateBranchLinkNeo(string parent, string child)
{
    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (t:branch), (b:commit) WHERE t.name ='{parent}' AND b.hash ='{child}' CREATE (t)-[branch_link:branch]->(b) RETURN type(branch_link)",
            new { });

        return result.Count();
    });

    return greeting > 0 ? true : false;
}

static bool DoesNodeExistAlready(string hash, string type)
{
    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (a:{type}) WHERE a.hash = '{hash}' RETURN a.name + ', from node ' + id(a)",
            new { });

        return result.Count() > 0 ? true : false;
    });

    return greeting;
}

static void AddCommitToNeo(string comment, string hash, string contents)
{

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));
    string name = $"commit #{hash} {comment}";

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            "CREATE (a:commit) " +
            "SET a.name = $name " +
            "SET a.comment = $comment " +
            "SET a.contents = $contents " +
            "SET a.hash = $hash " +
            "RETURN a.name + ', from node ' + id(a)",
            new { comment, hash, name, contents });

        return "created node";
    });
}

static void AddBranchToNeo(string name, string hash)
{

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            "CREATE (a:branch) " +
            "SET a.name = $name " +
            "SET a.hash = $hash " +
            "RETURN a.name + ', from node ' + id(a)",
            new { name, hash });

        return "created node";
    });
}

static void AddBlobToNeo(string filename, string hash, string contents)
{

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));
    string filenameplushash = $"{filename} #{hash}";

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            "CREATE (a:blob) " +
            "SET a.filenameplushash = $filenameplushash " +
            "SET a.hash = $hash " +
            "SET a.filename = $filename " +
            "SET a.contents = $contents " +
            "RETURN a.name + ', from node ' + id(a)",
            new { filenameplushash, hash, filename, contents });

        return "created node";
    });
}

static void AddTreeToNeo(string hash, string contents)
{

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    string name = $"tree #{hash}";
    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            "CREATE (a:tree) " +
            "SET a.name = $name " +
            "SET a.hash = $hash " +
            "SET a.contents = $contents " +
            "RETURN a.name + ', from node ' + id(a)",
            new { hash, contents, name });

        return "created node";
    });
}

static void AddHeadToNeo(string hash, string contents)
{

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            "CREATE (a:HEAD) " +
            "SET a.name = 'HEAD' " +
            "SET a.hash = $hash " +
            "SET a.contents = $contents " +
            "RETURN a.name + ', from node ' + id(a)",
            new { hash, contents });

        return "created node";
    });
}


static string GetFileType(string file)
{
    //run the git cat-file command to determine the file type
    Process p = new Process();
    p.StartInfo = new ProcessStartInfo("git.exe", $"cat-file {file} -t");
    p.StartInfo.RedirectStandardOutput = true;
    p.Start();

    while (!p.HasExited)
    {
        System.Threading.Thread.Sleep(100);
    }
    return p.StandardOutput.ReadToEnd();
}

static string GetContents(string file)
{
    Process p = new Process();
    p.StartInfo = new ProcessStartInfo("git.exe", $"cat-file {file} -p");
    p.StartInfo.RedirectStandardOutput = true;
    p.Start();

    while (!p.HasExited)
    {
        System.Threading.Thread.Sleep(100);
    }
    string contents = p.StandardOutput.ReadToEnd();
    return contents;
}
