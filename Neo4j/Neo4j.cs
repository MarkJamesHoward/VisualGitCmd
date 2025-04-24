using Neo4j.Driver;

public abstract class Neo4jHelper
{
    public static IDriver? _driver = null;
    public static ISession? session = null;
    static string password = "";
    static string uri = "";
    static string username = "";

    public static void AddCommitParentLinks(ISession? session, string path, string workingArea)
    {
        List<string> directories = Directory.GetDirectories(GlobalVars.GITobjectsPath).ToList();

        foreach (string dir in directories)
        {
            var files = Directory.GetFiles(dir).ToList();

            foreach (string file in files)
            {

                string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);
                string fileType = FileType.GetFileType_UsingGitCatFileCmd_Param_T(hashCode, GlobalVars.workingArea);

                if (fileType.Contains("commit"))
                {
                    string commitContents = FileType.GetContents(hashCode, GlobalVars.workingArea);
                    var commitParent = Regex.Match(commitContents, "parent ([0-9a-f]{4})");

                    if (commitParent.Success)
                    {
                        foreach (var item in commitParent.Groups.Values)
                        {
                            // string parentHash = commitParent.Groups[1].Value;
                            string parentHash = item.Value;
                            //Console.WriteLine($"\t-> parent commit {commitParent}");

                            Neo4jHelper.CreateCommitTOCommitLinkNeo(session, hashCode, parentHash);
                        }

                    }
                }
            }
        }
    }

    public static void AddOrphanBlobs(ISession? session, string branchPath, string path, string workingArea, bool PerformTextExtraction)
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
                string fileType = FileType.GetFileType_UsingGitCatFileCmd_Param_T(hashCode, workingArea);

                if (fileType.Contains("blob"))
                {
                    string blobContents = string.Empty;

                    if (PerformTextExtraction)
                    {
                        FileType.GetContents(hashCode, workingArea);
                    }

                    DebugMessages.GenericMessage($"blob {hashCode}");
                    if (!FileType.DoesNeo4jNodeExistAlready(session, hashCode, "blob"))
                    {
                        Neo4jHelper.AddBlobToNeo(session, hashCode, hashCode, blobContents);
                    }
                }
            }
        }
    }


    public static void AddBlobToNeo(ISession? session, string filename, string hash, string contents)
    {
        string filenameplushash = $"{filename} #{hash}";

        var greeting = session?.ExecuteWrite(
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

    public static void ProcessCommitForNeo4j(string commitComment, string treeHash, string hashCode_determinedFrom_dir_and_first2charOfFilename, CommitNodeExtraction CommitNode)
    {
        Neo4jHelper.AddCommitToNeo(Neo4jHelper.session, commitComment, hashCode_determinedFrom_dir_and_first2charOfFilename, CommitNode.CommitContents);
        Neo4jHelper.AddTreeToNeo(Neo4jHelper.session, treeHash, FileType.GetContents(treeHash, GlobalVars.workingArea));
        Neo4jHelper.CreateCommitLinkNeo(Neo4jHelper.session, hashCode_determinedFrom_dir_and_first2charOfFilename, treeHash, "", "");
    }

    public static bool DoesTreeToBlobLinkExist(ISession? session, string treeHash, string blobHash)
    {
        string query = "MATCH (t:tree { hash: $treeHash })-[r:blob]->(b:blob {hash: $blobHash }) RETURN r, b";
        var result = session?.Run(
                query,
                new { treeHash, blobHash });

        if (result != null)
        {
            foreach (var record in result)
            {
                return true;
            }
        }

        return false;
    }


    public static void ProcessNeo4jOutput()
    {
        if (GlobalVars.EmitNeo)
        {
            Neo4jHelper.AddCommitParentLinks(Neo4jHelper.session, GlobalVars.GITobjectsPath, GlobalVars.workingArea);
            Neo4jHelper.AddOrphanBlobs(Neo4jHelper.session, GlobalVars.branchPath, GlobalVars.GITobjectsPath, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
            GetHEAD(Neo4jHelper.session, GlobalVars.headPath);
        }
    }

    public static void GetHEAD(ISession? session, string path)
    {
        string HeadContents = File.ReadAllText(Path.Combine(GlobalVars.GITobjectsPath, "HEAD"));

        // Is the HEAD detached in which case it contains a Commit Hash
        Match match = Regex.Match(HeadContents, "[0-9a-f]{40}");
        if (match.Success)
        {
            string HEADHash = match.Value.Substring(0, 4);
            //Create the HEAD Node
            Neo4jHelper.AddHeadToNeo(session, HEADHash, HeadContents);
            //Create Link to Commit
            Neo4jHelper.CreateHEADTOCommitLinkNeo(session, HEADHash);
        }

        match = Regex.Match(HeadContents, @"ref: refs/heads/(\w+)");
        if (match.Success)
        {
            //Console.WriteLine("HEAD Branch extract: " + match.Groups[1]?.Value);
            string branch = match.Groups[1].Value;
            //Create the HEAD Node
            Neo4jHelper.AddHeadToNeo(session, branch, HeadContents);
            //Create Link to Commit
            Neo4jHelper.CreateHEADTOBranchLinkNeo(session, branch);
        }
    }

    public static void CheckIfNeoj4EmissionEnabled()
    {
        if (GlobalVars.EmitNeo)
        {
            _driver = Neo4jHelper.GetDriver(uri, username, password);
            session = _driver.Session();
            Neo4jHelper.ClearExistingNodesInNeo(session);
        }
    }

    public static IDriver GetDriver(string uri, string username, string password)
    {
        IDriver _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
        return _driver;
    }

    public static void ClearExistingNodesInNeo(ISession session)
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

    public static void AddCommitToNeo(ISession? session, string comment, string hash, string contents)
    {
        if (GlobalVars.EmitNeo)
        {
            string name = $"commit #{hash} {comment}";

            var greeting = session?.ExecuteWrite(
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
    }

    public static void CreateLinkNeo(ISession? session, string parent, string child, string parentType, string childType)
    {
        var greeting = session?.ExecuteWrite(
        tx =>
        {
            var result = tx.Run(
                $"MATCH (t:tree), (b:blob) WHERE t.hash ='{parent}' AND b.hash ='{child}' CREATE (t)-[blob_link:blob]->(b) RETURN type(blob_link)",
                new { });

            return result;
        });
    }

    public static bool CreateHEADTOBranchLinkNeo(ISession? session, string branchName)
    {

        //Console.WriteLine("HEAD -> " + branchName);
        var greeting = session?.ExecuteWrite(
        tx =>
        {
            var result = tx.Run(
                $"MATCH (t:HEAD), (b:branch) WHERE t.name ='HEAD' AND b.name ='{branchName}' CREATE (t)-[head_link:HEAD]->(b) RETURN type(head_link)",
                new { });

            return result.Count();
        });

        return greeting > 0 ? true : false;
    }

    public static bool CreateHEADTOCommitLinkNeo(ISession? session, string childCommit)
    {
        //Console.WriteLine("HEAD -> " + childCommit);
        var greeting = session?.ExecuteWrite(
        tx =>
        {
            var result = tx.Run(
                $"MATCH (t:HEAD), (b:commit) WHERE t.name ='HEAD' AND b.hash ='{childCommit}' CREATE (t)-[head_link:HEAD]->(b) RETURN type(head_link)",
                new { });

            return result.Count();
        });

        return greeting > 0 ? true : false;
    }

    public static bool CreateCommitTOCommitLinkNeo(ISession? session, string parent, string child)
    {
        var greeting = session?.ExecuteWrite(
        tx =>
        {
            var result = tx.Run(
                $"MATCH (t:commit), (b:commit) WHERE t.hash ='{parent}' AND b.hash ='{child}' CREATE (t)-[parent_link:parent]->(b) RETURN type(parent_link)",
                new { });

            return result.Count();
        });

        return greeting > 0 ? true : false;
    }

    public static bool CreateCommitLinkNeo(ISession? session, string parent, string child, string parentType, string childType)
    {
        if (GlobalVars.EmitNeo)
        {
            var greeting = session?.ExecuteWrite(
            tx =>
            {
                var result = tx.Run(
                    $"MATCH (t:commit), (b:tree) WHERE t.hash ='{parent}' AND b.hash ='{child}' CREATE (t)-[tree_link:tree]->(b) RETURN type(tree_link)",
                    new { });

                return result.Count();
            });


            return greeting > 0 ? true : false;
        }
        return false;
    }

    public static void AddTreeToNeo(ISession? session, string hash, string contents)
    {
        if (GlobalVars.EmitNeo && !FileType.DoesNeo4jNodeExistAlready(Neo4jHelper.session, hash, "tree"))
        {
            string name = $"tree #{hash}";

            var greeting = session?.ExecuteWrite(
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
    }

    public static void AddHeadToNeo(ISession? session, string hash, string contents)
    {
        var greeting = session?.ExecuteWrite(
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
    public static bool CreateRemoteBranchLinkNeo(ISession? session, string parent, string child)
    {
        //Console.WriteLine($"Create Remote Branch link {parent} {child}");

        var greeting = session?.ExecuteWrite(
        tx =>
        {
            var result = tx.Run(
                $"MATCH (t:remotebranch), (b:commit) WHERE t.name ='{parent}' AND b.hash ='{child}' CREATE (t)-[remotebranch_link:branch]->(b) RETURN type(remotebranch_link)",
                new { });

            return result.Count();
        });

        return greeting > 0 ? true : false;
    }
    public static void AddRemoteBranchToNeo(ISession? session, string name, string hash)
    {
        name = $"remote{name}";

        var greeting = session?.ExecuteWrite(
        tx =>
        {
            var result = tx.Run(
                "CREATE (a:remotebranch) " +
                "SET a.name = $name " +
                "SET a.hash = $hash " +
                "RETURN a.name + ', from node ' + id(a)",
                new { name, hash });

            return "created node";
        });
    }

    public static bool CreateBranchLinkNeo(ISession? session, string parent, string child)
    {
        var greeting = session?.ExecuteWrite(
        tx =>
        {
            var result = tx.Run(
                $"MATCH (t:branch), (b:commit) WHERE t.name ='{parent}' AND b.hash ='{child}' CREATE (t)-[branch_link:branch]->(b) RETURN type(branch_link)",
                new { });

            return result.Count();
        });

        return greeting > 0 ? true : false;
    }

    public static void AddBranchToNeo(ISession? session, string name, string hash)
    {
        var greeting = session?.ExecuteWrite(
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
}

