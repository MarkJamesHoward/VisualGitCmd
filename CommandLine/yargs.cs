using System;
using CommandLine;

namespace Yargs
{
    public class Options
    {
        [Option(
            'w',
            "web",
            Required = false,
            Default = true,
            HelpText = "Send Output to VisualGit Website - Default"
        )]
        public bool Web { get; set; }

        [Option('a', "api", Required = false, HelpText = "URL for api")]
        public string? Api { get; set; }

        [Option('n', "neo", Required = false, HelpText = "Send Output to Neo4j - Experimental")]
        public bool Neo { get; set; }

        [Option(
            'w',
            "wsl",
            Required = false,
            Default = false,
            HelpText = "Running in WSL environment"
        )]
        public bool IsWSL { get; set; }

        [Option('j', "jsonPath", Required = false, HelpText = "Folder to send JSON")]
        public string? Json { get; set; }

        [Option(
            'p',
            "RepoPath",
            Required = false,
            HelpText = "Git Repo Folder. By default we look in the current folder. But if this option is specified then it will override and use this path instead"
        )]
        public string? RepoPath { get; set; }

        [Option('d', "debug", Required = false, HelpText = "Enable debug mode")]
        public bool Debug { get; set; }

        [Option('s', "singlerun", Required = false, HelpText = "Enable single run mode")]
        public bool SingleRun { get; set; }

        [Option('l', "localdebugAPI", Required = false, HelpText = "Enable local debug of API")]
        public bool LocalDebugAPI { get; set; }

        [Option(
            'h',
            "localdebugwebsite",
            Required = false,
            HelpText = "Enable local debug of Website"
        )]
        public bool LocalDebugWebsite { get; set; }

        [Option('b', "bare", Required = false, HelpText = "Using Bare Repo")]
        public bool Bare { get; set; }

        [Option(
            'e',
            "extract",
            Required = false,
            Default = false,
            HelpText = "Extraction from the file contents will take place."
        )]
        public bool Extract { get; set; }

        [Option('r', "unpackrefs", Required = false, Default = false, HelpText = "Unpack Refs")]
        public bool UnpackRefs { get; set; }
    }
}
