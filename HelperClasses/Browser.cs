public abstract class Browser
{
    public static void OpenBrowser(ref bool firstRun)
    {
        if (firstRun)
        {
            firstRun = false;
            Process.Start(new ProcessStartInfo($"https://visualgit.net/visualize?data={RandomName.Name.Replace(' ', 'x')}/1") { UseShellExecute = true });
        }
    }

    public static async Task PostAsync(bool firstrun, string name, int dataID, HttpClient httpClient, string commitjson, string blobjson, string treejson, string branchjson, string remotebranchjson, string indexfilesjson, string workingfilesjson, string HEADjson)
    {
        if (firstrun)
        {
            StandardMessages.VisualGitID(name);
            // Console.WriteLine($"Visual Git ID:  {name}"); //Outputs some random first and last name combination in the format "{first} {last}" example: "Mark Rogers"
        }

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

        HttpResponseMessage response = await Resiliance._resilienace.ExecuteAsync(async ct => await httpClient.PostAsync("GitInternals", jsonContent, ct));

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
}