using System;
using System.IO;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using CloudMerger.HostingsManager;
using CloudMerger.HostingsManager.Tree;
using CommandLine;

namespace ConsoleApplication
{
    public class Application : ConsoleApplication
    {
        public const string TopologyFileName = "topology.ini";
        
        public Application(ServicesCollection services, HostingTreeBuilder treeBuilder, CredentialsFormatter formatter)
        {
            this.services = services;
            topology = File.ReadAllText(TopologyFileName);
            hosting = treeBuilder.FromCredentials(formatter.ParseNodes(topology));
        }

        public void Run(string[] args)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("> ");
                Console.ForegroundColor = ConsoleColor.White;
                var command = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                try
                {
                    Execute(command);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.GetType().Name}");
                    Console.WriteLine(ex.Message);
                    lastError = ex;
                }
                finally
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
            }
        }

        [Command]
        public async Task Upload(string from, string to=null)
        {
            to = to ?? from;
            Console.WriteLine($"upload '{from}' -> '{to}'");
            using (var stream = File.OpenRead(from))
            {
                await hosting.UploadFileAsync(stream, new UPath(to));
            }
        }

        [Command]
        public async Task Download(string from, string to=null)
        {
            to = to ?? from;
            Console.WriteLine($"download '{from}' -> '{to}'");
            using (var stream = File.OpenWrite(to))
            {
                await hosting.DownloadFileAsync(stream, new UPath(from));
            }
        }

        [Command]
        public async Task Auth(string service)
        {
            Console.WriteLine(await services.GetManager(service).AuthorizeAsync());
        }
        [Command] public void Services() => Console.WriteLine(string.Join("\n", services.Managers));
        [Command] public void Topology() => Console.WriteLine(topology);
        [Command] public void Help() => ShowHelp();
        [Command] public void Exit() => Environment.Exit(0);
        [Command] public void Error() => Console.WriteLine($"{lastError?.GetType()?.Name}\n{lastError}");

        private readonly IHosting hosting;
        private readonly string topology;
        private readonly ServicesCollection services;
        private Exception lastError = null;
    }
}