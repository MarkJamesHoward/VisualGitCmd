using Neo4j.Driver;

public abstract class Neo4jHelper
{
    public static IDriver? _driver = null;
    public static ISession? session = null;
    static string password = "";
    static string uri = "";
    static string username = "";

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

    public static void AddTreeToNeo(ISession? session, string hash, string contents)
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

