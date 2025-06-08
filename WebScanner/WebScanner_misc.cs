// Miscellaneous utility, informational and debug functions for WebScanner.

namespace WebScanner
{
    partial class WebScanner
    {
        private static void PrintConfig()
        {
            Console.WriteLine("ScanConfig:");
            Console.WriteLine($"  Output: {_config.Output}");
            Console.WriteLine($"  Depth: {_config.Depth}");
            Console.WriteLine($"  Include: [{string.Join(", ", _config.Include)}]");
            Console.WriteLine($"  Exclude: [{string.Join(", ", _config.Exclude)}]");
            Console.WriteLine($"  Verbose: {_config.Verbose}");
            Console.WriteLine($"  Timeout: {_config.Timeout}");
            Console.WriteLine($"  Uris:");
            foreach (var uri in _config.Uris)
            {
                Console.WriteLine($"    {uri}");
            }
        }

    }
}
