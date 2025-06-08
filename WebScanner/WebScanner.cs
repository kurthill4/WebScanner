/// <AINOTE TOPIC="About this file">
/// This file is part of the WebScanner project, which is designed to scan web pages for specific content.
/// This file (WebScanner.cs) is the main console program file.  The main() function resides here.
/// </AINOTE>
/// <AINOTE TOPIC="Miscellaneous notes about configuring powershell and VSCode and using copilot">
/// Powershell looks in the $PROFILE directory for a profile file that it runs at startup.
/// When running a PS1 script, it runs in a new instance of powershell, so changes do not persist.
/// In order fto persist changes, you need to run the script in the current instance of powershell.
/// You can do this by using the dot-sourcing operator (.) followed by the path to the script.
/// </AINOTE>
/// 

using System;
using System.IO;
using System.Threading.Tasks;
using System.CommandLine;
using System.Collections.Generic;
using UrlScannerLib;

namespace WebScanner
{
    public class ScanOptions
    {
        public string? Url { get; set; }
        public FileInfo? File { get; set; }
        public FileInfo? Output { get; set; }
        public int? Depth { get; set; }
        public string[] Include { get; set; } = Array.Empty<string>();
        public string[] Exclude { get; set; } = Array.Empty<string>();
        public bool Verbose { get; set; }
    }

    public class ScanConfig
    {
        public List<Uri> Uris { get; set; } = new List<Uri>();
        public FileInfo? Output { get; set; }
        public int? Depth { get; set; }
        public string[] Include { get; set; } = Array.Empty<string>();
        public string[] Exclude { get; set; } = Array.Empty<string>();
        public bool Verbose { get; set; }
        public int? Timeout { get; set; } // Timeout in seconds

        public void AddUri(string uriString)
        {
            if (string.IsNullOrWhiteSpace(uriString))
                throw new ArgumentException("URI string cannot be null or whitespace.", nameof(uriString));

            if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
                throw new UriFormatException($"Invalid URI: {uriString}");

            Uris.Add(uri);
        }
    }

    partial class WebScanner
    {
        static ScanConfig _config = new ScanConfig();
        


        static RootCommand PrepareRootCommand()
        {
            var urlArgument = new Argument<string?>("url", "The URL to scan.") { Arity = ArgumentArity.ZeroOrOne };
            var fileOption = new Option<FileInfo?>(
                aliases: new[] { "--file", "-f" },
                description: "A text file containing a list of URLs (one per line)."
            );
            var outputOption = new Option<FileInfo?>(
                aliases: new[] { "--output", "-o" },
                description: "Path to save scan results."
            );
            var depthOption = new Option<int?>(
                aliases: new[] { "--depth", "-d" },
                description: "How deep to follow links (default: 1)."
            );
            var includeOption = new Option<string[]>(
                aliases: new[] { "--include", "-i" },
                description: "Only scan URLs matching this pattern.",
                getDefaultValue: () => Array.Empty<string>()
            );
            var excludeOption = new Option<string[]>(
                aliases: new[] { "--exclude", "-x" },
                description: "Exclude URLs matching this pattern.",
                getDefaultValue: () => Array.Empty<string>()
            );
            var verboseOption = new Option<bool>(
                aliases: new[] { "--verbose", "-v" },
                description: "Enable verbose output."
            );
            var timeoutOption = new Option<int?>(
                aliases: new[] { "--timeout", "-t" },
                description: "Timeout in seconds for HTTP requests (default: 100)."
            );

            var rootCommand = new RootCommand("WebScanner utility")
            {
                urlArgument,
                fileOption,
                outputOption,
                depthOption,
                includeOption,
                excludeOption,
                verboseOption,
                timeoutOption
            };

            Action<string?, FileInfo?, FileInfo?, int?, string[], string[], bool, int?> handlerWrapper = ParseCommandLine;

            rootCommand.SetHandler(handlerWrapper, urlArgument, fileOption, outputOption, depthOption, includeOption, excludeOption, verboseOption, timeoutOption);

            return rootCommand;
        }

        private static void ParseCommandLine(string? url, FileInfo? file, FileInfo? output, int? depth, string[] include, string[] exclude, bool verbose, int? timeout)
        {
            _config.Output = output;
            _config.Depth = depth;
            _config.Include = include;
            _config.Exclude = exclude;
            _config.Verbose = verbose;
            _config.Timeout = timeout;

            /// <AINOTE CODE-DIRECTION>
            /// Populate _config with the specified URL's provided via command line or file.  To avoid repetitive code,
            /// create a string list and add the URLs from both the command line and any file specified.  Then iterate
            /// through the list and verify each URI is a valid HTTP or HTTPS URL and add to _config.Uris.
            /// For invalid URL's, print an error message.  Do not assume AddUri is doing any validation.
            /// </AINOTE>

            var urlList = new List<string>();
            if (!string.IsNullOrWhiteSpace(url))
            {
                urlList.Add(url);
            }
            if (file != null && file.Exists)
            {
                var fileUrls = File.ReadAllLines(file.FullName);
                foreach (var line in fileUrls)
                {
                    var trimmed = line.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        urlList.Add(trimmed);
                }
            }
            foreach (var urlString in urlList)
            {
                if (Uri.TryCreate(urlString, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    _config.Uris.Add(uri);
                }
                else
                {
                    Console.WriteLine($"Invalid URL: {urlString}");
                }
            }
        }

        static async Task<int> Main(string[] args)
        {
            // Set sane defaults:
            _config.Output = null;
            _config.Depth = 1;
            _config.Include = Array.Empty<string>();
            _config.Exclude = Array.Empty<string>();
            _config.Verbose = false;

            var rootCommand = PrepareRootCommand();
            int result = await rootCommand.InvokeAsync(args);

            // Print out the resulting scan config after command line parsing
            PrintConfig();
            

            if (_config.Verbose)
            {
                Console.WriteLine($"Scanning URL: {_config.Uris[0]}");
            }

            UrlScanner scanner = await UrlScanner.CreateAsync(_config.Uris[0]);
            Console.WriteLine($"Scanning completed for: {_config.Uris[0]}");


            // Print the list of links found in the scanned page
            var links = scanner.ExtractHyperlinks();
            if (links.Count > 0)
            {
                Console.WriteLine("Links found:");
                foreach (var link in links)
                {
                    Console.WriteLine($"  {link}");
                }
            }
            else
            {
                Console.WriteLine("No links found.");
            }
            return result;
        }
    }
}