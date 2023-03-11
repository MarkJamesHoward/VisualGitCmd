using System.Diagnostics;
using System.Text.RegularExpressions;
using Neo4j.Driver;

string path = @"C:\dev\visual\.git\objects\";
List<string> HashCodeFilenames = new List<string>();

// Get all the files in the .git/objects folder
try
{
    List<string> branchFiles = Directory.GetFiles(@".git\refs\heads").ToList();
    List<string> directories = Directory.GetDirectories(@".git\objects\").ToList();
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


            if (fileType.Contains("blob"))
            {
                //Nothing to do here
            }
            else if (fileType.Contains("tree"))
            {
                //Nothing to do here
            }
            else if (fileType.Contains("commit"))
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

                    AddCommitToNeo(comment, hashCode);

                    if (!DoesNodeExistAlready(treeHash, "tree"))
                    {
                        AddToNeo("tree", treeHash, "tree");
                        CreateCommitLinkNeo(hashCode, treeHash, "", "");
                    }

                    // Get the details of the Blobs in this Tree
                    string tree = GetContents(match.Groups[1].Value);
                    var blobsInTree = Regex.Matches(tree, @"blob ([0-9a-f]{4})[0-9a-f]{36}.(\w+)");

                    foreach (Match blobMatch in blobsInTree)
                    {
                        string blobHash = blobMatch.Groups[1].Value;

                        Console.WriteLine($"\t\t-> blob {blobHash} {blobMatch.Groups[2]}");
                        if (!DoesNodeExistAlready(blobHash, "blob"))
                        {
                            AddToNeo(blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, "blob");
                        }
                        CreateLinkNeo(match.Groups[1].Value, blobMatch.Groups[1].Value, "", "");
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
     foreach(var file in branchFiles) {
        var branchHash = await File.ReadAllTextAsync(file);
        AddBranchToNeo(Path.GetFileName(file), branchHash);
        CreateBranchLinkNeo(Path.GetFileName(file), branchHash.Substring(0,4));
    }

    AddCommitParentLinks();
}
catch (Exception e)
{
    Console.WriteLine($"Error while getting files in {path} {e.Message}");
}

static void AddCommitParentLinks()
{

    List<string> directories = Directory.GetDirectories(@".git\objects\").ToList();

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

static void ClearExistingNodesInNeo() {
     IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (n) DETACH DELETE n",
            new {});

        return result;
    });
}

static void CreateLinkNeo(string parent, string child, string parentType, string childType) {

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (t:tree), (b:blob) WHERE t.hash ='{parent}' AND b.hash ='{child}' CREATE (t)-[r:blob]->(b) RETURN type(r)",
            new {});

        return result;
    });
}

static bool CreateCommitTOCommitLinkNeo(string parent, string child) {

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (t:commit), (b:commit) WHERE t.hash ='{parent}' AND b.hash ='{child}' CREATE (t)-[r:parent]->(b) RETURN type(r)",
            new {});

        return result.Count();
    });

    return greeting > 0 ? true : false;
}

static bool CreateCommitLinkNeo(string parent, string child, string parentType, string childType) {

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (t:commit), (b:tree) WHERE t.hash ='{parent}' AND b.hash ='{child}' CREATE (t)-[r:tree]->(b) RETURN type(r)",
            new {});

        return result.Count();
    });

    return greeting > 0 ? true : false;
}

static bool CreateBranchLinkNeo(string parent, string child) {

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (t:branch), (b:commit) WHERE t.name ='{parent}' AND b.hash ='{child}' CREATE (t)-[r:branch]->(b) RETURN type(r)",
            new {});

        return result.Count();
    });

    return greeting > 0 ? true : false;
}

static bool DoesNodeExistAlready(string hash, string type) {

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (a:{type}) WHERE a.hash = '{hash}' RETURN a.name + ', from node ' + id(a)",
            new {});

        return result.Count() > 0 ? true : false;
    });

    return greeting;
}

static void AddCommitToNeo(string comment, string hash) {

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));
    string name = "commit " + hash + " " + comment;

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            "CREATE (a:commit) " +
            "SET a.name = $name " +
            "SET a.comment = $comment " +
            "SET a.hash = $hash " +
            "RETURN a.name + ', from node ' + id(a)",
            new {comment, hash, name});

        return "created node";
    });
}

static void AddBranchToNeo(string name, string hash) {

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
            new {name, hash});

        return "created node";
    });
}

static void AddToNeo(string filename, string hash, string type) {

    IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));
    string filenameplushash =  filename + " " + hash;

    using var session = _driver.Session();
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            "CREATE (a:" + type + ") " +
            "SET a.filenameplushash = $filenameplushash " +
            "SET a.hash = $hash " +
            "SET a.filename = $filename " +
            "RETURN a.name + ', from node ' + id(a)",
            new {filenameplushash, hash, filename});

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
