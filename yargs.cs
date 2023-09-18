using System;
using CommandLine;

namespace Yargs
{
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('w', "web", Required = false, HelpText = "Send Output to VisualGit Website")]
        public bool Web { get; set; }

         [Option('d', "debug", Required = false, HelpText = "Enable debug mode")]
        public bool Debug { get; set; }

        [Option('b', "bare", Required = false, HelpText = "Using Bare Repo")]
        public bool Bare { get; set; }

        [Option('n', "neo", Required = false, HelpText = "Send Output to Neo4j")]
        public bool Neo { get; set; }

        [Option('j', "json", Required = false, HelpText = "Send Output to JSON")]
        public bool Json { get; set; }
    }
}
    
