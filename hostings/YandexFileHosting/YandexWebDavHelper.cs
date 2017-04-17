using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YandexFileHosting
{
    internal static class YandexWebDavHelper
    {
        public static async Task<string> SendRequestAsync(this string token, string request)
        {
            HttpWebRequest webReq = (HttpWebRequest) WebRequest.Create("https://webdav.yandex.ru/");
            webReq.Accept = "*/*";
            webReq.Headers.Add("Depth: 0");
            webReq.Headers.Add("Authorization: OAuth " + token);
            webReq.Method = "PROPFIND";

            // Adding data in body request.
            byte[] buffer = new ASCIIEncoding().GetBytes(request);

            webReq.ContentType = "text/xml; encoding='utf-8";
            webReq.ContentLength = buffer.Length;

            try
            {
                await webReq.GetRequestStream().WriteAsync(buffer, 0, buffer.Length);
                var resp = (HttpWebResponse) await webReq.GetResponseAsync();
                var sr = new StreamReader(resp.GetResponseStream());
                return await sr.ReadToEndAsync();
            }
            catch
            {
            }
            return null;
        }

        public static async Task<string> SendSpaceRequestAsync(this string token)
        {
            var request = @"<D:propfind xmlns:D=""DAV:""><D:prop><D:quota-available-bytes/><D:quota-used-bytes/></D:prop></D:propfind>";
            var response = await token.SendRequestAsync(request);
            return response;
        }

        public static async Task<Tuple<long, long>> GetSpaceStatisticAsync(this string token)
        {
            var response = await token.SendSpaceRequestAsync();
            if (response == null)
                return null;
            var free = freeSpaceRe.Match(response);
            var used = usedSpaceRe.Match(response);
            if (!free.Success || !used.Success)
                return null;
            return Tuple.Create(long.Parse(free.Groups["size"].Value), long.Parse(used.Groups["size"].Value));
        }

        private static Regex freeSpaceRe = new Regex(@"<d:quota-available-bytes>(?<size>\d+?)</d:quota-available-bytes>");
        private static Regex usedSpaceRe = new Regex(@"<d:quota-used-bytes>(?<size>\d+?)</d:quota-used-bytes>");
    }
}