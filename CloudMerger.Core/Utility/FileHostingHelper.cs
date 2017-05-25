using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CloudMerger.Core.Primitives;
using CloudMerger.Core.Tree;

namespace CloudMerger.Core.Utility
{
    public static class FileHostingHelper
    {
        public static async Task<ItemType> GetItemType(this IHosting hosting, UPath path)
        {
            try
            {
                var info = await hosting.GetItemInfoAsync(path);
                return info.Type;
            }
            catch (ItemNotFound)
            {
                return ItemType.Unexist;
            }
        }

        public static async Task<bool> IsDirectoryAsync(this IHosting hosting, UPath path)
        {

            var result = (await hosting.TryGetItemInfoAsync(path))?.IsDirectory;
            if (result != null)
                return result.Value;
            throw new ItemNotFound(path);
        }

        public static async Task<bool> IsFileAsync(this IHosting hosting, UPath path)
        {
            var result = (await hosting.TryGetItemInfoAsync(path))?.IsFile;
            if (result != null)
                return result.Value;
            throw new ItemNotFound(path);
        }

        public static async Task<bool> IsDirectoryOrUnexistAsync(this IHosting hosting, UPath path)
        {
            return (await hosting.TryGetItemInfoAsync(path))?.IsDirectory ?? true;
        }

        public static async Task<bool> IsFileOrUnexistAsync(this IHosting hosting, UPath path)
        {
            return (await hosting.TryGetItemInfoAsync(path))?.IsFile ?? true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hosting"></param>
        /// <param name="path"></param>
        /// <returns>ItemInfo if item exist or null if item is not founded</returns>
        public static async Task<ItemInfo> TryGetItemInfoAsync(this IHosting hosting, UPath path)
        {
            try
            {
                return await hosting.GetItemInfoAsync(path);
            }
            catch (ItemNotFound)
            {
                return null;
            }
        }

        public static async Task<Node<ItemInfo>> GetItemsTreeAsync(this IHosting hosting, UPath path)
        {
            if (!await hosting.IsExistAsync(path))
                throw new ItemNotFound();
            if (await hosting.IsFileAsync(path))
                return new Node<ItemInfo>(await hosting.GetItemInfoAsync(path));
            return await hosting.GetDirectoryItemsTreeAsync(path);
        }

        public static async Task UploadFileAsync(this IHosting hosting, FileInfo source, UPath destination,
            IProgress<double> progress = null)
        {
            using (var stream = source.OpenRead())
            {
                await hosting.UploadFileAsync(stream, destination, progress);
            }
        }
        public static async Task DownloadFileAsync(this IHosting hosting, UPath source, FileInfo destination,
            IProgress<double> progress = null)
        {
            using (var stream = destination.OpenWrite())
            {
                await hosting.DownloadFileAsync(stream, source, progress);
            }
        }

        public static async Task UploadDirectoryAsync(this IHosting hosting, DirectoryInfo source, UPath destination,
            IProgress<UPath> failures = null, IProgress<UPath> successes = null, Action<UPath, Exception> logger = null)
        {
            if (!await hosting.IsDirectoryAsync(destination.Parent))
                throw new UnexpectedItemType("Parent of destination should be directory");
            if (!await hosting.IsDirectoryOrUnexistAsync(destination))
                throw new UnexpectedItemType("Destination should be a directory");
            await hosting.MakeDirectoryAsync(destination);

            var tasks = new List<Task>();
            foreach (var directory in source.GetDirectories())
            {
                var d = destination.SubPath(directory.Name);
                var task = hosting.UploadDirectoryAsync(directory, d, failures, successes, logger);
                tasks.Add(task.ContinueWith(t =>
                {
                    (t.IsFaulted ? failures : successes).Report(d);
                    if (t.IsFaulted)
                        logger(d, t.Exception.InnerException);
                }));
            }

            foreach (var file in source.GetFiles())
            {
                var f = destination.SubPath(file.Name);
                var task = hosting.UploadFileAsync(file, f);
                tasks.Add(task.ContinueWith(t =>
                {
                    (t.IsFaulted ? failures : successes).Report(f);
                    if (t.IsFaulted)
                        logger(f, t.Exception.InnerException);
                }));
            }

            await Task.WhenAll(tasks);
        }

        public static async Task DownloadDirectoryAsync(this IHosting hosting, UPath source, DirectoryInfo destination,
            IProgress<UPath> failures = null, IProgress<UPath> successes = null)
        {
            if (File.Exists(destination.FullName))
                throw new UnexpectedItemType("Destination should be a directory");
            if (destination.Parent == null || File.Exists(destination.Parent.FullName) || !destination.Parent.Exists)
                throw new UnexpectedItemType("Problem with destination parent");
            if (!destination.Exists)
                Directory.CreateDirectory(destination.FullName);

            var tasks = new List<Task>();
            var items = await hosting.GetDirectoryListAsync(source);

            foreach (var item in items)
            {
                var destFile = destination.GetSubFile(item.Name);
                var destDir = destination.GetSubDirectory(item.Name);
                var task = item.IsFile
                    ? hosting.DownloadFileAsync(item.Path, destFile)
                    : hosting.DownloadDirectoryAsync(item.Path, destDir, failures, successes);
                task = task.ContinueWith(t => (t.IsFaulted ? failures : successes).Report(item.Path));
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private static async Task<Node<ItemInfo>> GetDirectoryItemsTreeAsync(this IHosting hosting, UPath path)
        {
            var nested = (await hosting.GetDirectoryListAsync(path))
                .Select(async i =>
                {
                    return i.IsDirectory
                        ? await hosting.GetDirectoryItemsTreeAsync(i.Path)
                        : new Node<ItemInfo>(await hosting.GetItemInfoAsync(i.Path));
                }).ToList();
            await Task.WhenAll(nested);
            var node = new Node<ItemInfo>(await hosting.GetItemInfoAsync(path));
            node.Nested.AddRange(nested.Select(n => n.Result));
            return node;
        }
    }
}