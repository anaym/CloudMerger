using System;
using System.IO;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using CloudMerger.Core.Tree;
using CloudMerger.GuiPrimitives;
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
            this.treeBuilder = treeBuilder;
            this.formatter = formatter;
            if (File.Exists(TopologyFileName))
            {
                topology = formatter.ParseNodes(File.ReadAllText(TopologyFileName));
                if (topology != null)
                    hosting = treeBuilder.FromCredentials(topology);    
            }
        }

        public void Run(string[] args)
        {
            while (true)
            {
                try
                {
                    if (hosting == null)
                    {
                        Topology().Wait();
                        continue;
                    }
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("> ");
                    Console.ForegroundColor = ConsoleColor.White;
                    var command = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Execute(command);
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException)
                        ex = ((AggregateException) ex).InnerException;
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
        public async Task List(string path)
        {
            var i = 0;
            foreach (var item in await hosting.GetDirectoryListAsync(path))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{i,3} {(item.IsFile ? 'F' : 'D')}");
                Console.ForegroundColor = item.IsFile ? ConsoleColor.Cyan : ConsoleColor.Yellow;
                Console.Write($" '{item.Name}' \t");
                Console.ForegroundColor = ConsoleColor.Green;
                if (item.IsFile)
                    Console.Write($"[{item.Size}] {item.LastWriteTime}");
                Console.WriteLine();
                i++;
            }
        }

        [Command]
        public async Task Upload(string from, string to = null)
        {
            to = to ?? from;
            Console.WriteLine($"upload '{from}' -> '{to}'");
            using (var stream = File.OpenRead(from))
            {
                await hosting.UploadFileAsync(stream, new UPath(to), new ConsoleProgressProvider());
            }
        }

        [Command]
        public async Task Download(string from, string to = null)
        {
            to = to ?? from;
            Console.WriteLine($"download '{from}' -> '{to}'");
            using (var stream = File.OpenWrite(to))
            {
                await hosting.DownloadFileAsync(stream, new UPath(from), new ConsoleProgressProvider());
            }
        }

        [Command]
        public async Task Topology()
        {
            var result = await TopologyEditor.ShowNew(topology, services);
            if (result.HasBeenCanceled)
                return;

            topology = result;
            if (topology == null)
            {
                Console.WriteLine("Warning: you describe empty topology!");
                File.Delete(TopologyFileName);
                hosting = null;
            }
            else
            {
                hosting = treeBuilder.FromCredentials(topology);
                if (!File.Exists(TopologyFileName))
                    File.Create(TopologyFileName);
                using (var file = new StreamWriter(File.OpenWrite(TopologyFileName)))
                {
                    formatter.BuildNodes(topology, file);
                }
            }
        }

        [Command] public void Services() => Console.WriteLine(string.Join("\n", services.ManagerNames));
        [Command] public void Help() => ShowHelp();
        [Command] public void Exit() => Environment.Exit(0);
        [Command] public void Error() => Console.WriteLine($"{lastError?.GetType()?.Name}\n{lastError}");

        private IHosting hosting;
        private Node<OAuthCredentials> topology;
        private readonly ServicesCollection services;
        private readonly HostingTreeBuilder treeBuilder;
        private readonly CredentialsFormatter formatter;
        private Exception lastError = null;
    }
}