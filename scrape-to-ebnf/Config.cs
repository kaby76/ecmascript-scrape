using CommandLine;
using System.Collections.Generic;

namespace scrape_to_ebnf
{
    public class Config
    {
        [Option('f', "file", Required = false, HelpText = "Read Spec in html from file instead of stdin.")]
        public string File { get; set; }

        [Option('v', "verbose", Required = false)]
        public bool Verbose { get; set; }
    }
}
