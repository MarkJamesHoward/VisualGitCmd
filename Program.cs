using System.Diagnostics;
using System.Text.RegularExpressions;
using Neo4j.Driver;
using Microsoft.Extensions.Configuration;

string head = @".git\";
string path = @".git\objects\";
string branchPath = @".git\refs\heads";

string MyExePath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
string MyExeFolder = System.IO.Path.GetDirectoryName(MyExePath);
MyExeFolder = MyExeFolder.Replace(@"file:\", "");

var builder = new ConfigurationBuilder()
                               .SetBasePath(MyExeFolder)
                               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            string password  = builder.Build().GetSection("docker").GetSection("password").Value;
            string uri  = builder.Build().GetSection("docker").GetSection("url").Value;
            string username  = builder.Build().GetSection("docker").GetSection("username").Value;



List<string> HashCodeFilenames = new List<string>();

// Get all the files in the .git/objects folder
try
{
    List<string> branchFiles = Directory.GetFiles(branchPath).ToList();
    List<string> directories = Directory.GetDirectories(path).ToList();
    List<string> files = new List<string>();

    IDriver _driver = GetDriver(uri, username, password);
    var session = _driver.Session();

    ClearExistingNodesInNeo(session);

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

                    AddCommitToNeo(session, comment, hashCode, commitContents);

                    if (!DoesNodeExistAlready(session, treeHash, "tree"))
                    {
                        AddTreeToNeo(session, treeHash, GetContents(treeHash));
                    }
                    CreateCommitLinkNeo(session, hashCode, treeHash, "", "");


                    // Get the details of the Blobs in this Tree
                    string tree = GetContents(match.Groups[1].Value);
                    var blobsInTree = Regex.Matches(tree, @"blob ([0-9a-f]{4})[0-9a-f]{36}.([\w\.]+)");

                    foreach (Match blobMatch in blobsInTree)
                    {
                        string blobHash = blobMatch.Groups[1].Value;
                        string blobContents = GetContents(blobHash);

                        Console.WriteLine($"\t\t-> blob {blobHash} {blobMatch.Groups[2]}");
                        if (!DoesNodeExistAlready(session, blobHash, "blob"))
                        {
                            AddBlobToNeo(session, blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, blobContents);
                        }
                        if (!DoesTreeToBlobLinkExist(session, match.Groups[1].Value, blobHash))
                        {
                            CreateLinkNeo(session, match.Groups[1].Value, blobMatch.Groups[1].Value, "", "");
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
        AddBranchToNeo(session, Path.GetFileName(file), branchHash);
        CreateBranchLinkNeo(session, Path.GetFileName(file), branchHash.Substring(0, 4));
    }

    AddCommitParentLinks(session, path);
    AddOrphanBlobs(session, branchPath, path);
    GetHEAD(session, head);
}
catch (Exception e)
{
    Console.WriteLine($"Error while getting files in {path} {e.Message}");
}

static void GetHEAD(ISession session, string path) 
{
    string HeadContents = File.ReadAllText(Path.Combine(path, "HEAD"));

    // Is the HEAD detached in which case it contains a Commit Hash
    Match match = Regex.Match(HeadContents, "[0-9a-f]{40}");
    if (match.Success) {
        string HEADHash = match.Value.Substring(0, 4);
        //Create the HEAD Node
        AddHeadToNeo(session, HEADHash, HeadContents);
        //Create Link to Commit
        CreateHEADTOCommitLinkNeo(session, HEADHash);
    }

    match = Regex.Match(HeadContents, @"ref: refs/heads/(\w+)");
    if (match.Success) {
        Console.WriteLine("HEAD Branch extract: " + match.Groups[1]?.Value);
        string branch = match.Groups[1].Value;
         //Create the HEAD Node
        AddHeadToNeo(session, branch, HeadContents);
        //Create Link to Commit
        CreateHEADTOBranchLinkNeo(session, branch);
    }
}



static bool DoesTreeToBlobLinkExist(ISession session, string treeHash, string blobHash)
{
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

static void AddOrphanBlobs(ISession session, string branchPath, string path)
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
                if (!DoesNodeExistAlready(session, hashCode, "blob"))
                {
                    AddBlobToNeo(session, hashCode, hashCode, blobContents);
                }
            }
        }
    }
}


static void AddCommitParentLinks(ISession session, string path)
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

                    CreateCommitTOCommitLinkNeo(session, hashCode, parentHash);
                }
            }
        }
    }
}

static IDriver GetDriver(string uri, string username, string password) {
    IDriver _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
    return _driver;
}

static void ClearExistingNodesInNeo(ISession session)
{
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (n) DETACH DELETE n",
            new { });

        return result;
    });
}

static void CreateLinkNeo(ISession session, string parent, string child, string parentType, string childType)
{
    var greeting = session.ExecuteWrite(
    tx =>
    {
        var result = tx.Run(
            $"MATCH (t:tree), (b:blob) WHERE t.hash ='{parent}' AND b.hash ='{child}' CREATE (t)-[blob_link:blob]->(b) RETURN type(blob_link)",
            new { });

        return result;
    });
}

static bool CreateHEADTOBranchLinkNeo(ISession session, string branchName)
{
    
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

static bool CreateHEADTOCommitLinkNeo(ISession session, string childCommit)
{
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

static bool CreateCommitTOCommitLinkNeo(ISession session, string parent, string child)
{
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

static bool CreateCommitLinkNeo(ISession session, string parent, string child, string parentType, string childType)
{
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

static bool CreateBranchLinkNeo(ISession session, string parent, string child)
{
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

static bool DoesNodeExistAlready(ISession session, string hash, string type)
{
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

static void AddCommitToNeo(ISession session, string comment, string hash, string contents)
{
    string name = $"commit #{hash} {comment}";
    
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

static void AddBranchToNeo(ISession session, string name, string hash)
{
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

static void AddBlobToNeo(ISession session, string filename, string hash, string contents)
{
    string filenameplushash = $"{filename} #{hash}";
    
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

static void AddTreeToNeo(ISession session, string hash, string contents)
{
    string name = $"tree #{hash}";
    
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

static void AddHeadToNeo(ISession session, string hash, string contents)
{
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
