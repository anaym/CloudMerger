using System;
using System.IO;
using System.Linq;
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
    public class TopologyConfigurator : IHostingProvider
    {
        public const string TopologyFileName = "topology.ini";
        public IHosting Hosting { get; private set; }

        public TopologyConfigurator(ServicesCollection services, HostingTreeBuilder treeBuilder,
            CredentialsFormatter formatter)
        {
            this.services = services;
            this.treeBuilder = treeBuilder;
            this.formatter = formatter;
            try
            {
                LoadFromFile();
            }
            catch (Exception ex)
            {
                ColoredConsole.WriteLine(ConsoleColor.Red, $"Can`t load topology:\n{ex.GetType().Name}\n{ex.Message}");
            }
        }

        [Command("configure your accounts tree")]
        public async Task Topology()
        {
            var result = await TopologyEditor.ShowNew(topology, services);
            if (result.HasBeenCanceled)
                return;

            topology = result;
            Hosting = treeBuilder.FromCredentials(topology);
            SaveToFile();
        }

        [Command("show list of available services")]
        public void Services() => Console.WriteLine(string.Join("\n", services.ManagerNames));

        private void LoadFromFile()
        {
            if (!File.Exists(TopologyFileName))
                return;
            using (var text = File.OpenText(TopologyFileName))
            {
                topology = formatter.ParseNodes(text);
            }
            if (topology != null)
                Hosting = treeBuilder.FromCredentials(topology);
            else
                Hosting = null;
        }

        private void SaveToFile()
        {
            if (topology == null)
            {
                if (File.Exists(TopologyFileName))
                    File.Delete(TopologyFileName);
            }
            using (var file = File.OpenWrite(TopologyFileName))
            {
                using (var text = new StreamWriter(file))
                {
                    formatter.BuildNodes(topology, text);
                }
            }
        }

        private Node<OAuthCredentials> topology;
        private readonly ServicesCollection services;
        private readonly HostingTreeBuilder treeBuilder;
        private readonly CredentialsFormatter formatter;
    }
}