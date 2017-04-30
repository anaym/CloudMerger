using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using CloudMerger.Core.Utility;

namespace MergedHosting
{
    //TODO: Что делать, если директория существует на хостах A и B, но я хочу залить в нее файл на хост С?
    internal class MergedHosting : IHosting
    {
        public const string ServiceName = "Merged Hosing";

        public MergedHosting(IHosting[] hostings, TimeSpan timeout)
        {
            this.hostings = hostings;
            this.Timeout = timeout;
        }

        public string Name => ServiceName;

        public async Task<DiskSpaceInfo> GetSpaceInfoAsync()
        {
            var taskPool = hostings.ToTask(h => h.GetSpaceInfoAsync(), Timeout);
            await taskPool.WhenAll();
            return taskPool.Completed.Select(t => t.Result).Aggregate(DiskSpaceInfo.Empty, (l, n) => l + n);
        }

        public async Task<bool> IsExistAsync(UPath path)
        {
            try
            {
                await GetItemInfoAsync(path);
                return true;
            }
            catch (ItemNotFound)
            {
                return false;
            }
        }

        public async Task<ItemInfo> GetItemInfoAsync(UPath path)
        {
            Dictionary<Task<ItemInfo>, IHosting> sources = null;
            var taskPool = hostings.ToTaskWithSource(h => h.GetItemInfoAsync(path), Timeout, out sources);
            await taskPool.WhenAll();

            var notFounded = taskPool.Finished.Where(t => t.IsFaultedWith<ItemNotFound>()).ToList();
            var dir = taskPool.Completed.Where(t => t.Result.IsDirectory).ToList();
            var file = taskPool.Completed.Where(t => t.Result.IsFile).ToList();

            var withIFSS = taskPool.Faulted.FirstOrDefault(t => t.IsFaultedWith<InconsistentFileSystemState>());
            if (withIFSS != null)
                throw new InconsistentFileSystemState("Inconsistent any hosting", withIFSS.Exception?.InnerException);

            if (file.Count == 1 && dir.Count == 0)
                return file[0].Result;
            if (file.Count == 0 && dir.Count > 0)
                return dir[0].Result.OnHostings(dir.SelectMany(d => d.Result.Hostings));
            if (file.Count == 0 && dir.Count == 0)
                throw new ItemNotFound(notFounded.Count > 0 ? "Iten not founded" : "Hosings unavailable");

            var fstat = $"Files on: {string.Join("; ", file.SelectMany(t => t.Result.Hostings).Select(h => h.Name))}";
            var dstat = $"Directories on: {string.Join("; ", dir.SelectMany(t => t.Result.Hostings).Select(h => h.Name))}";

            if (file.Count > 1 && dir.Count == 0)
                throw new InconsistentFileSystemState($"More than one hosting contains file '{path}':\n{fstat}");
            if (file.Count > 0 && dir.Count > 0)
                throw new InconsistentFileSystemState($"Inconsistent item type: file or directory?\n{fstat}\n{dstat}");

            throw new HostingException("Unexpected condition!");
        }

        public async Task<IEnumerable<ItemInfo>> GetDirectoryListAsync(UPath path)
        {
            var withDir = await GetHostingsWithDirectory(path);
            if (withDir.Count == 0)
                throw new ItemNotFound();
            var directories = new Dictionary<string, ItemInfo>();
            var files = new Dictionary<string, ItemInfo>();
            var content = await Task.WhenAll(withDir.Select(h => h.GetDirectoryListAsync(path)));
            foreach (var item in content.SelectMany(c => c))
                    if (item.IsFile)
                    {
                        if (files.ContainsKey(item.Name))
                            throw new InconsistentFileSystemState($"More than one hosting contains file '{item.Name}'");
                        if (directories.ContainsKey(item.Name))
                            throw new InconsistentFileSystemState($"{item.Name} is file or directory?");
                        files[item.Name] = item;
                    }
                    else
                    {
                        if (files.ContainsKey(item.Name))
                            throw new InconsistentFileSystemState($"{item.Name} is file or directory?");
                        directories[item.Name] = item;
                    }
            return directories.Values.Union(files.Values);
        }

        public async Task MakeDirectoryAsync(UPath path)
        {
            if (!await this.IsDirectoryOrUnexistAsync(path))
                throw new UnexpectedItemType("Expected directory");
            await Task.WhenAll(hostings.Select(h => h.MakeDirectoryAsync(path)));
        }

        public async Task RemoveFileAsync(UPath path)
        {
            if (!await this.IsFileOrUnexistAsync(path))
                throw new UnexpectedItemType("Expected file");
            await Task.WhenAll(hostings.Select(h => h.RemoveFileAsync(path)));
        }

        public async Task RemoveDirectoryAsync(UPath path, bool recursive)
        {
            if (!await IsExistAsync(path))
                return;
            if (await this.IsFileAsync(path))
                throw new UnexpectedItemType("Expected directory");
            if (!recursive && (await GetDirectoryListAsync(path)).Any())
                throw new InvalidOperationException("Directory is not empty and recursive disabled'");
            await Task.WhenAll(hostings.Select(h => h.RemoveDirectoryAsync(path, recursive)));
        }

        public async Task RenameAsync(UPath path, string newName)
        {
            if (new UPath(newName).Parent != "/")
                throw new ArgumentException("Expected name, not path", nameof(newName));
            var dest = path.Parent.SubPath(newName);
            if (!await IsExistAsync(path))
                throw new ItemNotFound("Source is not founded");
            if (await IsExistAsync(dest))
                throw new InvalidOperationException("Destiantion already exist");
            await Task.WhenAny(hostings.Select(async h =>
            {
                if (!await IsExistAsync(path))
                    return;
                await h.RenameAsync(path, newName);
            }));
        }

        public async Task UploadFileAsync(Stream stream, UPath path, IProgress<double> progressProvider = null)
        {
            if (!await this.IsFileOrUnexistAsync(path))
                throw new UnexpectedItemType("Expected file or unexisted path");
            try
            {
                var host = await GetHostingWithFile(path);
                await host.UploadFileAsync(stream, path, progressProvider);
            }
            catch (ItemNotFound)
            {
                if (!await IsExistAsync(path.Parent))
                    throw new ItemNotFound("Parent not founded");

                var sizes = (await Task.WhenAll(hostings.Select(h => h.GetSpaceInfoAsync())))
                    .Select(h => h.FreeSpace).ToList();

                var host = hostings[sizes.IndexOf(sizes.Max())];
                await host.MakeDirectoryAsync(path.Parent); //if directory don`t exist only on this host
                await host.UploadFileAsync(stream, path, progressProvider);
            }
        }

        public async Task DownloadFileAsync(Stream stream, UPath path, IProgress<double> progressProvider = null)
        {
            if (await this.IsDirectoryAsync(path))
                throw new UnexpectedItemType("Expected file");
            var host = await GetHostingWithFile(path);
            await host.DownloadFileAsync(stream, path, progressProvider);
        }

        public void Dispose()
        {
            foreach (var hosting in hostings)
                hosting.Dispose();
        }

        private async Task<IHosting> GetHostingWithFile(UPath path)
        {
            var workedHostings = hostings.ToList();
            var tasks = hostings.Select(h => h.IsFileAsync(path).WithTimeOut(Timeout)).ToList();

            IHosting withFile = null;
            while (tasks.Count > 0)
            {
                var completed = await Task.WhenAny(tasks);
                var hosting = workedHostings[tasks.IndexOf(completed)];
                if (!completed.IsFaulted && completed.Result)
                {
                    if (withFile != null)
                        throw new HostingException("More one hosting contains file!");
                    withFile = hosting;
                }
                workedHostings.Remove(hosting);
                tasks.Remove(completed);
            }
            if (withFile != null)
                return withFile;
            throw new ItemNotFound($"File '{path}' not founded");
        }

        private async Task<List<IHosting>> GetHostingsWithDirectory(UPath path)
        {
            var withDir = new List<IHosting>();
            var workedHostings = hostings.ToList();
            var tasks = hostings.Select(h => h.TryGetItemInfoAsync(path)).ToList();
            while (tasks.Count > 0)
            {
                var completed = await Task.WhenAny(tasks);
                var hosting = workedHostings[tasks.IndexOf(completed)];
                if (completed.IsFaultedWith<InconsistentFileSystemState>())
                    throw new InconsistentFileSystemState(completed.Exception?.InnerException);
                if (!completed.IsFaulted && completed.Result != null)
                {
                    if (completed.Result.IsDirectory)
                        withDir.Add(hosting);
                    else
                        throw new UnexpectedItemType("Expected path to directory");
                }
                workedHostings.Remove(hosting);
                tasks.Remove(completed);
            }
            return withDir;
        }

        private readonly IHosting[] hostings;
        private readonly TimeSpan Timeout;
    }
}
