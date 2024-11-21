using Neo4j.Driver;

public abstract class Neo4jHelper
{
    public static IDriver? _driver = null;
    public static ISession? session = null;
    static string password = "";
    static string uri = "";
    static string username = "";

    public static void ProcessCommitForNeo4j(string commitComment, string treeHash, string hashCode_determinedFrom_dir_and_first2charOfFilename, CommitNodeExtraction CommitNode)
    {
        Neo4jHelper.AddCommitToNeo(Neo4jHelper.session, commitComment, hashCode_determinedFrom_dir_and_first2charOfFilename, CommitNode.CommitContents);
        Neo4jHelper.AddTreeToNeo(Neo4jHelper.session, treeHash, FileType.GetContents(treeHash, GlobalVars.workingArea));
        Neo4jHelper.CreateCommitLinkNeo(Neo4jHelper.session, hashCode_determinedFrom_dir_and_first2charOfFilename, treeHash, "", "");
    }

    public static void ProcessNeo4jOutput()
    {
        if (GlobalVars.EmitNeo)
        {
            Links.AddCommitParentLinks(Neo4jHelper.session, GlobalVars.GITobjectsPath, GlobalVars.workingArea);
            BlobCode.AddOrphanBlobs(Neo4jHelper.session, GlobalVars.branchPath, GlobalVars.GITobjectsPath, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
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

