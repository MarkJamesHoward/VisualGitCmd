using System.Diagnostics;
using System.Text.RegularExpressions;
using Neo4j.Driver;
using System.Text.Json;
using System.Text;
using MyProjectl;

namespace MyProject
{
    public class VisualGit
    {
        readonly string name = RandomName.randomNameGenerator.GenerateRandomPlaceName();

        bool BatchingUpFileChanges = false;
        object MainLockObj = new Object();
        static bool firstRun = true;
        int batch = 1;
        int dataID = 1;

        string password = "";
        string uri = "";
        string username = "";
        List<string> HashCodeFilenames = new List<string>();

        public void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (
                (e?.Name?.Contains(".lock", StringComparison.CurrentCultureIgnoreCase) ?? false)  ||
                (e?.Name?.Contains("tmp", StringComparison.CurrentCultureIgnoreCase) ?? false)
            )
            {
                return;
            }

            if (!BatchingUpFileChanges)
            {
                BatchingUpFileChanges = true;

                var t = Task.Run(delegate
                {
                    lock (MainLockObj)
                    {
                        batch++;

                        Console.WriteLine($"Batch {batch} Waiting for file changes to complete.....");
                        Thread.Sleep(2000);
                        BatchingUpFileChanges = false;

                        Console.WriteLine($"Batch {batch} Processing.....");
                        Run();
                        Console.WriteLine($"Batch {batch} Completed.....");
                    }

                });

            }
            else
            {
                //Console.WriteLine($"Batch {batch} batching " + e.Name);
            }
        }

        public void Run()
        {
            List<CommitNode> CommitNodes = new List<CommitNode>();
            List<TreeNode> TreeNodes = new List<TreeNode>();
            List<Blob> blobs = new List<Blob>();
            List<Branch> branches = new List<Branch>();
            List<Branch> remoteBranches = new List<Branch>();

            HEAD HEAD = new HEAD();

            // Get all the files in the .git/objects folder
            try
            {
                List<string> remoteBranchFiles = new List<string>();
                List<string> branchFiles = new List<string>();

                branchFiles = Directory.GetFiles(GlobalVars.branchPath).ToList();
                RemoteBranches.GetRemoteBranches(ref remoteBranchFiles);

                List<string> directories = Directory.GetDirectories(GlobalVars.path).ToList();
                List<string> files = new List<string>();

                IDriver _driver;
                ISession? session = null;

                if (GlobalVars.EmitNeo)
                {
                    _driver = GetDriver(uri, username, password);
                    session = _driver.Session();
                    ClearExistingNodesInNeo(session);
                }


                foreach (string dir in directories)
                {
                    if (dir.Contains("pack") || dir.Contains("info"))
                    {
                        break;
                    }

                    files = Directory.GetFiles(dir).ToList();

                    foreach (string file in files)
                    {
                        if (file.Contains("pack-") || file.Contains(".idx"))
                        {
                            break;
                        }
              
                        string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);

                        HashCodeFilenames.Add(hashCode);

                        string fileType = FileType.GetFileType(hashCode, GlobalVars.workingArea);

                        //Console.WriteLine($"{fileType.TrimEnd('\n', '\r')} {hashCode}");

                        if (fileType.Contains("commit"))
                        {
                            string commitContents;
                            commitContents = FileType.GetContents(hashCode, GlobalVars.workingArea);
                       
                            var match = Regex.Match(commitContents, "tree ([0-9a-f]{4})");
                            var commitParents = Regex.Matches(commitContents, "parent ([0-9a-f]{4})");
                            var commitComment = Regex.Match(commitContents, "\n\n(.+)\n");

                            if (match.Success)
                            {
                                // Get details of the tree,parent and comment in this commit
                                string treeHash = match.Groups[1].Value;
                                //Console.WriteLine($"\t-> tree {treeHash}");

                                List<string> commitParentHashes = new List<string>();

                                foreach (Match commitParentMatch in commitParents)
                                {
                                    //string parentHash = commitParent.Groups[1].Value;
                                    commitParentHashes.Add(commitParentMatch.Groups[1].Value);
                                    //Console.WriteLine($"\t-> hashCode parent commit {commitParentMatch.Groups[1].Value}");
                                }

                                string comment = commitComment.Groups[1].Value;
                                comment = comment.Trim();

                                if (GlobalVars.EmitNeo)
                                {
                                    Neo4j.AddCommitToNeo(session, comment, hashCode, commitContents);
                                }

                                if (GlobalVars.EmitNeo && !FileType.DoesNodeExistAlready(session, treeHash, "tree"))
                                {
                                    if (GlobalVars.EmitNeo)
                                        Neo4j.AddTreeToNeo(session, treeHash, FileType.GetContents(treeHash, GlobalVars.workingArea));
                                }

                                CreateTreeJson(treeHash, FileType.GetContents(treeHash, GlobalVars.workingArea), TreeNodes);
                                CreateCommitJson(commitParentHashes, comment, hashCode, treeHash, commitContents, CommitNodes);

                                if (GlobalVars.EmitNeo)
                                {
                                    Neo4j.CreateCommitLinkNeo(session, hashCode, treeHash, "", "");
                                }

                                // Get the details of the Blobs in this Tree
                                string tree = FileType.GetContents(match.Groups[1].Value, GlobalVars.workingArea);
                                var blobsInTree = Regex.Matches(tree, @"blob ([0-9a-f]{4})[0-9a-f]{36}.([\w\.]+)");

                                foreach (Match blobMatch in blobsInTree)
                                {
                                    string blobHash = blobMatch.Groups[1].Value;
                                    string blobContents = string.Empty;

                                    if (GlobalVars.PerformTextExtraction)
                                    {
                                        FileType.GetContents(blobHash, GlobalVars.workingArea);
                                    }

                                    //Console.WriteLine($"\t\t-> blob {blobHash} {blobMatch.Groups[2]}");
                                    if (GlobalVars.EmitNeo && !FileType.DoesNodeExistAlready(session, blobHash, "blob"))
                                    {
                                        if (GlobalVars.EmitNeo)
                                            BlobCode.AddBlobToNeo(session, blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, blobContents);
                                    }
                                    //Console.WriteLine($"Adding non orphan blob {blobMatch.Groups[1].Value}");

                                    BlobCode.AddBlobToJson(treeHash, blobMatch.Groups[2].Value, blobMatch.Groups[1].Value, blobContents, blobs);

                                    if (GlobalVars.EmitNeo && !DoesTreeToBlobLinkExist(session, match.Groups[1].Value, blobHash))
                                    {
                                        if (GlobalVars.EmitNeo)
                                            Neo4j.CreateLinkNeo(session, match.Groups[1].Value, blobMatch.Groups[1].Value, "", "");
                                    }

                                    CreateTreeToBlobLinkJson(match.Groups[1].Value, blobMatch.Groups[1].Value, TreeNodes);
                                }
                            }
                            else
                            {
                                //Console.WriteLine("No Tree found in Commit");
                            }
                        }
                    }

                }
                
                GitBranches.ProcessBranches(branchFiles, session, ref branches);
                RemoteBranches.ProcessRemoteBranches(remoteBranchFiles, session, ref remoteBranches);

                if (GlobalVars.EmitNeo)
                {
                    AddCommitParentLinks(session, GlobalVars.path, GlobalVars.workingArea);
                    BlobCode.AddOrphanBlobs(session, GlobalVars.branchPath, GlobalVars.path, blobs, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
                    GetHEAD(session, GlobalVars.head);
                }


                if (GlobalVars.EmitJsonOnly)
                {
                    BlobCode.AddOrphanBlobsToJson(GlobalVars.branchPath, GlobalVars.path, blobs, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
                    OutputNodesJson(CommitNodes, GlobalVars.CommitNodesJsonFile);
                    OutputNodesJson(TreeNodes, GlobalVars.TreeNodesJsonFile);
                    OutputNodesJson(blobs, GlobalVars.BlobNodesJsonFile);
                    OutputHEADJson(HEAD, GlobalVars.HeadNodesJsonFile, GlobalVars.head);
                    OutputBranchJson(branches, TreeNodes, blobs, GlobalVars.BranchNodesJsonFile);
                    OutputIndexFilesJson(GlobalVars.IndexFilesJsonFile);
                    OutputWorkingFilesJson(GlobalVars.workingArea, GlobalVars.WorkingFilesJsonFile);
                }

                if (GlobalVars.EmitWeb)
                {
                    BlobCode.AddOrphanBlobsToJson(GlobalVars.branchPath, GlobalVars.path, blobs, GlobalVars.workingArea, GlobalVars.PerformTextExtraction);
                    OutputNodesJsonToAPI(firstRun, name, dataID++, CommitNodes, blobs, TreeNodes, branches, remoteBranches, IndexFilesJsonNodes(GlobalVars.workingArea), WorkingFilesNodes(GlobalVars.workingArea), HEADNodes(GlobalVars.head));
                }

                // Only run this on the first run
                if (firstRun)
                {
                    firstRun = false;
                    Process.Start(new ProcessStartInfo($"https://visualgit.net/visualize?data={name.Replace(' ', 'x')}/1") { UseShellExecute = true });
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains($"Could not find a part of the GlobalVars.path"))
                {
                    Console.WriteLine("Waiting for Git to be initiased in this folder...");

                    if (GlobalVars.debug)
                    {
                        Console.WriteLine($"Details: {e.Message}");
                    }
                    else {
                        Console.WriteLine($"Details: {e.Message}");

                    }
                }
                else
                {
                    Console.WriteLine($"Error while getting files in {GlobalVars.path} {e.Message} {e}");
                }
            }
        }

        void CreateCommitJson(List<string> parentCommitHash, string comment, string hash, string treeHash, string contents, List<CommitNode> CommitNodes)
        {
            CommitNode n = new CommitNode();
            n.text = comment;
            n.hash = hash;
            n.parent = parentCommitHash;
            n.tree = treeHash;

            if (!CommitNodes.Exists(i => i.hash == n.hash))
                CommitNodes.Add(n);
        }

        void CreateTreeJson(string treeHash, string contents, List<TreeNode> TreeNodes)
        {
            TreeNode tn = new TreeNode();
            tn.hash = treeHash;
            tn.blobs = new List<string>();

            if (!TreeNodes.Exists(i => i.hash == tn.hash))
            {
                TreeNodes.Add(tn);
            }
        }

        static HEAD HEADNodes(string path)
        {
            string HeadContents = File.ReadAllText(Path.Combine(path, "HEAD"));
            //Console.WriteLine("Outputting JSON HEAD");
            string HEADHash = "";

            // Is the HEAD detached in which case it contains a Commit Hash
            Match match = Regex.Match(HeadContents, "[0-9a-f]{40}");
            if (match.Success)
            {
                //Console.WriteLine("Outputting JSON HEAD match found 1");
                HEADHash = match.Value.Substring(0, 4);
            }
            match = Regex.Match(HeadContents, @"ref: refs/heads/(\w+)");
            if (match.Success)
            {
                //Console.WriteLine("Outputting JSON HEAD match found 2");

                //Console.WriteLine("HEAD Branch extract: " + match.Groups[1]?.Value);
                HEADHash = match.Groups[1].Value;
                //CreateHEADTOBranchLinkNeo(session, branch);
            }
            HEAD h = new HEAD();
            h.hash = HEADHash;
            return h;

        }

        void OutputHEADJson(HEAD head, string JsonPath, string path)
        {
            string HeadContents = File.ReadAllText(Path.Combine(GlobalVars.path, "HEAD"));
            //Console.WriteLine("Outputting JSON HEAD");
            string HEADHash = "";

            // Is the HEAD detached in which case it contains a Commit Hash
            Match match = Regex.Match(HeadContents, "[0-9a-f]{40}");
            if (match.Success)
            {
                //Console.WriteLine("Outputting JSON HEAD match found 1");
                HEADHash = match.Value.Substring(0, 4);
            }
            match = Regex.Match(HeadContents, @"ref: refs/heads/(\w+)");
            if (match.Success)
            {
                //Console.WriteLine("Outputting JSON HEAD match found 2");

                //Console.WriteLine("HEAD Branch extract: " + match.Groups[1]?.Value);
                HEADHash = match.Groups[1].Value;
                //CreateHEADTOBranchLinkNeo(session, branch);
            }
            HEAD h = new HEAD();
            h.hash = HEADHash;

            var Json = string.Empty;
            Json = JsonSerializer.Serialize(h);

            //Console.WriteLine(Json);
            File.WriteAllText(JsonPath, Json);
        }


        void OutputBranchJson<T>(List<T> Nodes, List<TreeNode> TreeNodes, List<Blob> blobs, string JsonPath)
        {
            var Json = string.Empty;

            Json = JsonSerializer.Serialize(Nodes);

            //Console.WriteLine(Json);
            File.WriteAllText(JsonPath, Json);
        }

        static List<WorkingFile> WorkingFilesNodes(string workingFolder)
        {

            List<string> files = FileType.GetWorkingFiles(workingFolder);
            List<WorkingFile> WorkingFilesList = new List<WorkingFile>();


            foreach (string file in files)
            {
                WorkingFile FileObj = new WorkingFile();
                FileObj.filename = file;
                if (GlobalVars.PerformTextExtraction)
                {
                    FileObj.contents = FileType.GetFileContents(Path.Combine(workingFolder, file));
                }
                WorkingFilesList.Add(FileObj);
            }
            return WorkingFilesList;
        }

        void OutputWorkingFilesJson(string workingFolder, string JsonPath)
        {
            var Json = string.Empty;
            List<WorkingFile> WorkingFilesList = new List<WorkingFile>();

            List<string> files = FileType.GetWorkingFiles(workingFolder);

            foreach (string file in files)
            {
                WorkingFile FileObj = new WorkingFile();
                FileObj.filename = file;
                FileObj.contents = FileType.GetFileContents(Path.Combine(workingFolder, file));
                WorkingFilesList.Add(FileObj);
            }

            Json = JsonSerializer.Serialize(WorkingFilesList);
            File.WriteAllText(JsonPath, Json);
        }

        static List<IndexFile> IndexFilesJsonNodes(string workingArea)
        {
            var Json = string.Empty;
            List<IndexFile> IndexFilesList = new List<IndexFile>();

            string files = FileType.GetIndexFiles(GlobalVars.workingArea);
            // Console.WriteLine(files);
            List<string> fileList = files.Split("\n").ToList();

            foreach (string file in fileList)
            {
                IndexFile FileObj = new IndexFile();
                FileObj.filename = file;
                IndexFilesList.Add(FileObj);
            }

            return IndexFilesList;

        }

        void OutputIndexFilesJson(string JsonPath)
        {
            var Json = string.Empty;
            List<IndexFile> IndexFilesList = new List<IndexFile>();

            string files = FileType.GetIndexFiles("");
            //Console.WriteLine(files);
            List<string> fileList = files.Split("\n").ToList();

            foreach (string file in fileList)
            {
                IndexFile FileObj = new IndexFile();
                FileObj.filename = file;
                IndexFilesList.Add(FileObj);
            }

            Json = JsonSerializer.Serialize(IndexFilesList);

            Console.WriteLine(JsonPath);
            File.WriteAllText(JsonPath, Json);
        }


        static async Task PostAsync(bool firstrun, string name, int dataID, HttpClient httpClient, string commitjson, string blobjson, string treejson, string branchjson, string remotebranchjson, string indexfilesjson, string workingfilesjson, string HEADjson)
        {
            if (firstrun)
                Console.WriteLine($"Visual Git ID:  {name}"); //Outputs some random first and last name combination in the format "{first} {last}" example: "Mark Rogers"

            using StringContent jsonContent = new(
                JsonSerializer.Serialize(new
                {
                    userId = $"{name.Replace(' ', 'x')}",
                    id = $"{dataID++}",
                    commitNodes = commitjson ?? "",
                    blobNodes = blobjson ?? "",
                    treeNodes = treejson ?? "",
                    branchNodes = branchjson ?? "",
                    remoteBranchNodes = remotebranchjson ?? "",
                    headNodes = HEADjson ?? "",
                    indexFilesNodes = indexfilesjson ?? "",
                    workingFilesNodes = workingfilesjson ?? ""
                }),
                    Encoding.UTF8,
                    "application/json");

            // var resilienace =  new ResiliencePipelineBuilder()
            // .AddRetry(new RetryStrategyOptions {
            //         ShouldHandle = new PredicateBuilder().Handle<Exception>(),
            //         Delay = TimeSpan.FromSeconds(2),
            //         MaxRetryAttempts = 2,
            //         BackoffType = DelayBackoffType.Exponential
            // })
            // .AddTimeout(TimeSpan.FromSeconds(30))
            // .Build();

            HttpResponseMessage response = await Resiliance._resilienace.ExecuteAsync(async ct => await httpClient.PostAsync("GitInternals",jsonContent, ct)); 

            try
            {
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Please restart VisualGit...");
            }
        }

        static async void OutputNodesJsonToAPI(bool firstrun, string name, int dataID, List<CommitNode> CommitNodes,
         List<Blob> BlobNodes, List<TreeNode> TreeNodes, List<Branch> BranchNodes, List<Branch> RemoteBranchNodes,
         List<IndexFile> IndexFilesNodes, List<WorkingFile> WorkingFilesNodes, HEAD HEADNodes)
        {
            var Json = string.Empty;

            var CommitJson = JsonSerializer.Serialize(CommitNodes);
            var BlobJson = JsonSerializer.Serialize(BlobNodes);
            var TreeJson = JsonSerializer.Serialize(TreeNodes);
            var BranchJson = JsonSerializer.Serialize(BranchNodes);
            var RemoteBranchJson = JsonSerializer.Serialize(RemoteBranchNodes);
            var IndexFilesJson = JsonSerializer.Serialize(IndexFilesNodes);
            var WorkingFilesJson = JsonSerializer.Serialize(WorkingFilesNodes);
            var HEADJson = JsonSerializer.Serialize(HEADNodes);

            HttpClient sharedClient = new()
            {
                BaseAddress = new Uri("https://gitvisualiserapi.azurewebsites.net/api/gitinternals"),
            };
            await PostAsync(firstrun, name, dataID, sharedClient, CommitJson, BlobJson, TreeJson, BranchJson, RemoteBranchJson, IndexFilesJson, WorkingFilesJson, HEADJson);
        }

        void OutputNodesJson<T>(List<T> Nodes, string JsonPath)
        {
            var Json = string.Empty;

            Json = JsonSerializer.Serialize(Nodes);

            //Console.WriteLine(Json);
            //Console.WriteLine(JsonPath);
            File.WriteAllText(JsonPath, Json);
        }


        void GetHEAD(ISession? session, string path)
        {
            string HeadContents = File.ReadAllText(Path.Combine(GlobalVars.path, "HEAD"));

            // Is the HEAD detached in which case it contains a Commit Hash
            Match match = Regex.Match(HeadContents, "[0-9a-f]{40}");
            if (match.Success)
            {
                string HEADHash = match.Value.Substring(0, 4);
                //Create the HEAD Node
                Neo4j.AddHeadToNeo(session, HEADHash, HeadContents);
                //Create Link to Commit
                Neo4j.CreateHEADTOCommitLinkNeo(session, HEADHash);
            }

            match = Regex.Match(HeadContents, @"ref: refs/heads/(\w+)");
            if (match.Success)
            {
                //Console.WriteLine("HEAD Branch extract: " + match.Groups[1]?.Value);
                string branch = match.Groups[1].Value;
                //Create the HEAD Node
                Neo4j.AddHeadToNeo(session, branch, HeadContents);
                //Create Link to Commit
                Neo4j.CreateHEADTOBranchLinkNeo(session, branch);
            }
        }



        static bool DoesTreeToBlobLinkExist(ISession? session, string treeHash, string blobHash)
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




        void AddCommitParentLinks(ISession? session, string path, string workingArea)
        {
            List<string> directories = Directory.GetDirectories(GlobalVars.path).ToList();

            foreach (string dir in directories)
            {
                var files = Directory.GetFiles(dir).ToList();

                foreach (string file in files)
                {

                    string hashCode = Path.GetFileName(dir) + Path.GetFileName(file).Substring(0, 2);
                    string fileType = FileType.GetFileType(hashCode, GlobalVars.workingArea);

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

                                Neo4j.CreateCommitTOCommitLinkNeo(session, hashCode, parentHash);
                            }

                        }
                    }
                }
            }
        }

        static IDriver GetDriver(string uri, string username, string password)
        {
            IDriver _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
            return _driver;
        }

        void ClearExistingNodesInNeo(ISession session)
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

        void CreateTreeToBlobLinkJson(string parent, string child, List<TreeNode> treeNodes)
        {
            var treeNode = treeNodes?.Find(i => i.hash == parent);
            treeNode?.blobs?.Add(child);
        }

    

  


       


        


    }

   

}
