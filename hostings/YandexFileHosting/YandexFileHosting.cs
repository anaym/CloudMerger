﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using Disk.SDK;
using Disk.SDK.Provider;

namespace YandexFileHosting
{
    internal class YandexFileHosting : IFileHosting
    {
        public string Name => $"yandex disk: {login}";

        public YandexFileHosting(string token, string login)
        {
            this.token = token;
            this.login = login;
            client = new DiskAsyncClient(token);
        }

        public async Task<DiskSpaceInfo> GetSpaceInfoAsync()
        {
            try
            {
                var spaces = await token.GetSpaceStatisticAsync();
                return new DiskSpaceInfo(new FileSize(spaces.Item1), new FileSize(spaces.Item2));
            }
            catch (Exception ex)
            {
                throw new HostUnavailable(ex);
            }
        }

        public async Task<bool> IsExistAsync(UPath path)
        {
            try
            {
                await GetItemInfoAsync(path);
            }
            catch (ItemNotFound)
            {
                return false;
            }
            return true;
        }

        public async Task<ItemInfo> GetItemInfoAsync(UPath path)
        {
            try
            {
                return ParseInfo(await client.GetItemInfoAsync(path.UnixFormat()));
            }
            catch (SdkBadParameterException ex)
            {
                throw new ItemNotFound(path, ex);
            }
            catch (SdkException ex)
            {
                throw new HostUnavailable("", ex);
            }
        }

        public async Task<IEnumerable<ItemInfo>> GetDirectoryListAsync(UPath path)
        {
            try
            {
                if (!(await client.GetItemInfoAsync(path)).IsDirectory)
                    throw new UnexpectedItemType("Expected path to directory");

                var ypath = path.UnixFormat();
                return (await client.GetListAsync(ypath))
                    .Select(ParseInfo)
                    .Where(i => i.Path != path);
            }
            catch (SdkBadParameterException ex)
            {
                throw new ItemNotFound(path, ex);
            }
            catch (SdkException ex)
            {
                throw new HostUnavailable("", ex);
            }
        }

        public async Task MakeDirectoryAsync(UPath path)
        {
            try
            {
                var info = await GetItemInfoAsync(path);
                if (info.IsDirectory)
                    return;
                if (info.IsFile)
                    throw new UnexpectedItemType("Expected path to directory or unexisted path");
            }
            catch (ItemNotFound)
            {
            }
            catch (HostUnavailable)
            {
                throw; //explicit
            }

            try
            {
                await client.MakeDirectoryAsync(path.UnixFormat());
            }
            catch (SdkException ex)
            {
                throw new ItemNotFound(ex);
            }
        }

        public async Task RemoveFileAsync(UPath path)
        {
            try
            {
                var info = await GetItemInfoAsync(path);
                if (info.IsDirectory)
                    throw new UnexpectedItemType("Expected file or unexisted path");
                await client.RemoveAsync(path.UnixFormat());
            }
            catch (SdkBadParameterException) //file or path unexist
            {
            }
            catch (ItemNotFound) //file or path unexist
            {
            }
            catch (HostUnavailable)
            {
                throw; //explicit
            }
        }

        public async Task RemoveDirectoryAsync(UPath path, bool recursive)
        {
            try
            {
                var list = await GetDirectoryListAsync(path);
                if (list.Count() != 0 && !recursive)
                    throw new InvalidOperationException("Directory is not empty, enable recursive");
                await client.RemoveAsync(path.UnixFormat());
            }
            catch (SdkBadParameterException) //dir or path unexist
            {
            }
            catch (ItemNotFound) //dir or path unexist
            {
            }
            catch (HostUnavailable)
            {
                throw; //explicit
            }
        }

        public async Task MoveFileAsync(UPath source, UPath destination)
        {
            await client.MoveAsync(source.UnixFormat(), destination.UnixFormat());
        }

        public async Task CopyFileAsync(UPath source, UPath destination)
        {
            try
            {
                var dinfo = await GetItemInfoAsync(destination);
                if (dinfo.IsDirectory)
                    throw new UnexpectedItemType("Destination should be file oe unexisted path");
            }
            catch (ItemNotFound)
            { }

            try
            {
                var sinfo = await GetItemInfoAsync(source);
                if (sinfo.IsDirectory)
                    throw new UnexpectedItemType("Source should be file");
                if (!await IsExistAsync(destination.Parent))
                    throw new ItemNotFound("Parent for destination unexist");
                await client.CopyAsync(source.UnixFormat(), destination.UnixFormat());
            }
            catch (ItemNotFound)
            {
                throw; //explicit
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UploadFileAsync(Stream stream, UPath path, IProgress<double> progress)
        {
            await token.UploadAsync(path.UnixFormat(), stream, progress);
        }

        public async Task DownloadFileAsync(Stream stream, UPath path, IProgress<double> progressProvider)
        {
            await token.DownloadAsync(path.UnixFormat(), stream, progressProvider);
        }

        public void Dispose() { }

        public override string ToString() => Name;

        private ItemInfo ParseInfo(DiskItemInfo info)
        {
            return new ItemInfo
            (
                new UPath(info.FullPath), 
                info.IsDirectory ? ItemType.Directory : ItemType.File,
                new FileSize(info.ContentLength),
                info.LastModified,
                this
            );
        }

        private DiskAsyncClient client;
        private readonly string token;
        private readonly string login;
    }
}