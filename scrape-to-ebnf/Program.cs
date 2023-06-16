using CommandLine;
using CommandLine.Text;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace scrape_to_ebnf
{
    public class Program
    {
        static void Main(string[] args)
        {
            Config config = null;
            // Parse options, stop if we see a bogus option, or something like --help.
            var result = new CommandLine.Parser().ParseArguments<Config>(args);
            bool stop = false;
            result.WithNotParsed(
                errs =>
                {
                    DisplayHelp(result, errs);
                    stop = true;
                });
            if (stop) return;
            result.WithParsed(o =>
            {
                config = o;
                // Overwrite the defaults with what was passed on the command line.
                var ty = typeof(Config);
                foreach (var prop in ty.GetProperties())
                {
                    if (prop.GetValue(o, null) != null)
                    {
                        prop.SetValue(config, prop.GetValue(o, null));
                    }
                }
            });
            if (stop) return;
            
            string lines;
            if (!(config.File != null && config.File != ""))
            {
                if (config.Verbose)
                {
                    System.Console.Error.WriteLine("reading from stdin");
                }
                for (; ; )
                {
                    lines = System.Console.In.ReadToEnd();
                    if (lines != null && lines != "") break;
                }
                lines = lines.Trim();
            }
            else
            {
                if (config.Verbose)
                {
                    System.Console.Error.WriteLine("reading from file >>>" + config.File + "<<<");
                }
                lines = File.ReadAllText(config.File);
            }
            
            var doc = new HtmlDocument();
            doc.LoadHtml(lines);
            var nodes = doc.DocumentNode.SelectNodes("//emu-annex[@id = 'sec-grammar-summary']//emu-production");
            foreach (var node in nodes)
            {
                var lhs_index = node.ChildNodes
                    .Select((value, index) => new { value, index })
                    .Where(e => e.value.Name == "emu-nt")
                    .Select(e => e.index)
                    .FirstOrDefault();
                var lhs = node.ChildNodes[lhs_index];
                //System.Console.WriteLine(lhs.InnerText);
                var s = node.Attributes["name"].Value;
                System.Console.Write(s + " ");
                var cc_index = node.ChildNodes
                    .Select((value, index) => new { value, index })
                    .Where(e => e.index > lhs_index && e.value.Name == "emu-geq")
                    .Select(e => e.index)
                    .FirstOrDefault();
                var cc = node.ChildNodes[cc_index];
                System.Console.Write(/*cc.InnerText*/ ": ");
                int start = cc_index++;
                bool do_or = false;
                while (true)
                {
                    var rhs_index = node.ChildNodes
                        .Select((value, index) => new { value, index })
                        .Where(e => e.index > start && e.value.Name == "emu-rhs")
                        .Select(e => e.index)
                        .FirstOrDefault();
                    if (rhs_index == 0) break;
                    start = rhs_index + 1;
                    var rhs = node.ChildNodes[rhs_index];
                    if (do_or) System.Console.Write(" | ");
                    do_or = true;
                    OutputRhs(rhs);
                }

                System.Console.WriteLine(";");
                System.Console.WriteLine();
            }
        }

        private static void OutputRhs(HtmlNode rhs)
        {
            foreach (var node in rhs.ChildNodes)
            {
                if (node.Name == "emu-gprose")
                {
                    System.Console.Write(node.InnerText);
                }
                else if (node.Name == "emu-nt")
                {
                    if (node.ChildNodes.Count > 0)
                    {
                        if (node.ChildNodes.Count == 1)
                        {
                            if (node.ChildNodes[0].Name != "a")
                                throw new Exception();
                            System.Console.Write(node.ChildNodes[0].InnerText);
                        }
                        else if (node.ChildNodes.Count == 2)
                        {
                            if (node.ChildNodes[0].Name != "a")
                                throw new Exception();
                            System.Console.Write(node.ChildNodes[0].InnerText);
                            if (node.ChildNodes[1].Name != "emu-mods")
                                throw new Exception();
                            var c = node.ChildNodes[1];
                            var cc = c.ChildNodes[0];
                            if (cc.Name == "emu-opt")
                                System.Console.Write("?");
                            else if (cc.Name == "emu-params")
                                ;//System.Console.Write("/* " + node.InnerText + " */");
                            else
                                throw new Exception();
                        }
                        else throw new Exception();
                    }
                    else System.Console.Write(node.InnerText);
                }
                else if (node.Name == "#text")
                {
                    //System.Console.Write(node.InnerText);
                    System.Console.Write(" ");
                }
                else if (node.Name == "emu-gann")
                {
                    System.Console.Write("/* " + node.InnerText + " */");
                }
                else if (node.Name == "emu-t")
                {
                    System.Console.Write("'" + node.InnerText + "'");
                }
                else if (node.Name == "emu-gmod")
                {
                    System.Console.Write("/* " + node.InnerText + " */");
                }
                else if (node.Name == "emu-constraints")
                {
                    System.Console.Write("/* " + node.InnerText + " */");
                }
                else throw new Exception();
            }
            //System.Console.WriteLine();
        }

        string Fixup(string text)
        {
            Regex r = new Regex("[<]/a.*[<]/");
            return text;
        }

        static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            HelpText helpText = null;
            if (errs.IsVersion())  //check if error is version request
                helpText = HelpText.AutoBuild(result);
            else
            {
                helpText = HelpText.AutoBuild(result, h =>
                {
                    h.AdditionalNewLineAfterOption = false;
                    h.Heading = "ecmascript-scrape";
                    h.Copyright = "Copyright (c) 2023 Ken Domino"; //change copyright text
                    h.AddPreOptionsText("");
                    return HelpText.DefaultParsingErrorsHandler(result, h);
                }, e => e);
            }
            Console.WriteLine(helpText);
        }
    }
}
