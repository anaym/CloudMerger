using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using CloudMerger.Core.Utility;

namespace ConsoleApplication
{
    public class FileHostingClient
    {
        public FileHostingClient(IHostingProvider hostingProvider)
        {
            this.hostingProvider = hostingProvider;
            dirListVisuzlizer = new ConsoleTableVisualizer(new[] { ConsoleColor.Cyan, ConsoleColor.Red, ConsoleColor.Yellow }, new[] { -1, 10, 10 }, 0);
        }

        [Command("show list of nested files and directories", true)]
        public async Task List(string path)
        {
            var items = await Hosting.GetDirectoryListAsync(path);
            dirListVisuzlizer.PagedShow(items.Select(i => new[] { i.Name, i.LastWriteTime.ToString(), i.IsDirectory ? "" : i.Size.ToShortString() }));
        }

        [Command("upload file to cloud", true)]
        public async Task Upload(string from, string to = null)
        {
            to = to ?? from;
            Console.WriteLine($"upload '{from}' -> '{to}'");
            using (var stream = File.OpenRead(from))
            {
                await Hosting.UploadFileAsync(stream, new UPath(to), new ConsoleProgressProvider());
            }
        }

        [Command("download file from cloud", true)]
        public async Task Download(string from, string to = null)
        {
            to = to ?? from;
            Console.WriteLine($"download '{from}' -> '{to}'");
            using (var stream = File.OpenWrite(to))
            {
                await Hosting.DownloadFileAsync(stream, new UPath(from), new ConsoleProgressProvider());
            }
        }

        [Command("show info about free disk space", true)]
        public async Task Info()
        {
            var o = Console.ForegroundColor;
            var r = ConsoleColor.Red;
            var g = ConsoleColor.Green;
            var b = ConsoleColor.Cyan;
            var sizes = await Hosting.GetSpaceInfoAsync();

            var usedPercent = 100 * sizes.UsedSpace.TotalBytes / sizes.TotalSpace.TotalBytes;
            var freePercent = 100 * sizes.FreeSpace.TotalBytes / sizes.TotalSpace.TotalBytes;
            ColoredConsole.WriteLine(o, "Used ", r, sizes.UsedSpace, o, " from ", b, sizes.TotalSpace, o, " - ", r, usedPercent, "%");
            ColoredConsole.WriteLine(o, "Free ", g, sizes.FreeSpace, o, " from ", b, sizes.TotalSpace, o, " - ", g, freePercent, "%");

            var maxCells = Console.WindowWidth - 1;
            var usedCells = maxCells * usedPercent / 100;
            ColoredConsole.Write(r, string.Join("", Enumerable.Repeat("|", (int)usedCells)));
            ColoredConsole.WriteLine(g, string.Join("", Enumerable.Repeat(".", maxCells - (int)usedCells)));
        }

        [Command("show info about file or directory", true)]
        public async Task Info(string path)
        {
            var o = Console.ForegroundColor;
            var r = ConsoleColor.Red;
            var g = ConsoleColor.Green;
            var b = ConsoleColor.Cyan;
            var info = await Hosting.GetItemInfoAsync(path);

            ColoredConsole.WriteLine(b, info.IsDirectory ? "DIRECTORY" : "FILE");
            ColoredConsole.WriteLine("Name:\t", b, info.Name);
            ColoredConsole.WriteLine("Size:\t", b, info.Size);
            ColoredConsole.WriteLine("LW time:", b, info.LastWriteTime);
            ColoredConsole.WriteLine("On hostings: ");
            foreach (var hosting in info.Hostings)
                ColoredConsole.WriteLine("\t", b, hosting.Name);
        }

        [Command("create empty directory", true)]
        public async Task MakeDir(string path)
        {
            await Hosting.MakeDirectoryAsync(path);
        }

        [Command("rename file or folder", true)]
        public async Task Rename(string path, string newName)
        {
            await Hosting.RenameAsync(path, newName);
        }

        [Command("remove file or directory [maybe with files]")]
        public async Task Delete(string path)
        {
            if (await Hosting.IsDirectoryAsync(path))
                await Hosting.RemoveDirectoryAsync(path, true);
            else
                await Hosting.RemoveFileAsync(path);
        }

        private IHosting Hosting
        {
            get
            {
                var host = hostingProvider.Hosting;
                if (host == null)
                    throw new InvalidOperationException("Hosting is unconfigured ");
                return host;
            }
        }
        private readonly IHostingProvider hostingProvider;
        private readonly ConsoleTableVisualizer dirListVisuzlizer;
    }
}