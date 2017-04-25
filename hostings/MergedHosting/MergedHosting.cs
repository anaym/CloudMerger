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
    //TODO: solve file overlaps:
    //  host A: file.jpg, 12.12.2011
    //  host B: file.jpg, 13.13.2012
    //      remove file.jpg on host A (LWW)
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
            throw new NotImplementedException();

        }

        public async Task<ItemInfo> GetItemInfoAsync(UPath path)
        {
            Dictionary<Task<ItemInfo>, IHosting> sources = null;
            var taskPool = hostings.ToTaskWithSource(h => h.GetItemInfoAsync(path), Timeout, out sources);
            await taskPool.WhenAll();

            var notFounded = taskPool.Finished.Where(t => t.IsFaultedWith<ItemNotFound>()).ToList();
            var dir = taskPool.Completed.Where(t => t.Result.IsDirectory).ToList();
            var file = taskPool.Completed.Where(t => t.Result.IsFile).ToList();

            if (dir.Count == 0 && file.Count == 0 && notFounded.Count == 0)
                throw new ItemNotFound("All hostings unavailable :(");
            else if (dir.Count == 0 && file.Count == 0 && notFounded.Count > 0)
                throw new ItemNotFound();
            else if (dir.Count > 0 && file.Count == 0)
            {
                if (notFounded.Count == 0)
                    return dir.First().Result;

                await notFounded.Select(t => sources[t]).ToTask(h => h.MakeDirectoryAsync(path), Timeout).WhenAll();
                return dir.First().Result;
            }
            else if (dir.Count == 0 && file.Count > 0)
            {
                if (file.Count == 1)
                    return file.First().Result;

                await file
                    .OrderByDescending(t => t.Result.LastWriteTime)
                    .Skip(1)
                    .Select(t => sources[t])
                    .ToTask(h => h.RemoveFileAsync(path))
                    .WhenAll();

                return file.OrderByDescending(t => t.Result.LastWriteTime).First().Result;
            }
            else // dir.Count > 0 && file.Count > 0
            {
                throw new HostingException($"Unfixable merge problem: exist hostings, where '{path}' is file, but exist where it is dir");
            }
        }

        public async Task<IEnumerable<ItemInfo>> GetDirectoryListAsync(UPath path)
        {
            throw new NotImplementedException();
        }

        public async Task MakeDirectoryAsync(UPath path)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveFileAsync(UPath path)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveDirectoryAsync(UPath path, bool recursive)
        {
            throw new NotImplementedException();
        }

        public async Task MoveFileAsync(UPath source, UPath destination)
        {
            throw new NotImplementedException();
        }

        public async Task CopyFileAsync(UPath source, UPath destination)
        {
            if (!await this.IsFileAsync(source))
                throw new UnexpectedItemType(source);
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
                //find host with more space
                //upload
                throw new NotImplementedException();
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

        private Task SolveCollision(IReadOnlyList<ItemInfo> files, IReadOnlyList<ItemInfo> dir)
        {
            throw new NotImplementedException();
        }

        private readonly IHosting[] hostings;
        private readonly TimeSpan Timeout;
    }
}
