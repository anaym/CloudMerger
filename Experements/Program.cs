using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using CloudMerger.GuiPrimitives;
using CloudMerger.HostingsManager;
using YandexFileHosting;

namespace Experements
{
    class Program
    {
        static void Main(string[] args)
        {
            //var f = new YandexHostingManager().AuthorizeAsync();
            //var s = new YandexHostingManager().AuthorizeAsync();
            //Task.WaitAll(f, s);

            OAuthCredentials.FromString("123 login ---");

            var services = ServicesCollection.LoadServices(new DirectoryInfo("hostings")).Select(s => s.Value).ToArray();
            var task = OAuthCredentialsEditor.ShowNew(new OAuthCredentials {Service = "yandex DisK"}, services);
            task.Wait();

            var host = new YandexHostingManager().GetFileHostingFor(new OAuthCredentials {Token = "AQAEA7qiAp8AAAQv83T09kxsZEXmiSIla-bv4ho", Service = "yandex disk"});
            var t = host.GetSpaceInfoAsync();
            t.Wait();
            Console.WriteLine(t.Result);
            var list = host.GetDirectoryListAsync("Non empty dir");
            list.Wait();
            foreach (var itemInfo in list.Result)
            {
                Console.WriteLine(itemInfo.Name);
            }

            Console.WriteLine(task.Result);
            return;

            var loader = new HostingsTreeSerializer(new Dictionary<string, Func<IHosting[], IHosting>>
            {
                {"merged hosting", hostings => new MergedHosting(hostings, TimeSpan.FromMilliseconds(5000))}
            });

            using (var file = File.OpenRead("topology.ini"))
            {
                var tree = loader.LoadNodes(new StreamReader(file), ServicesCollection.LoadServices(new DirectoryInfo("hostings")));
                using (var res = File.CreateText("ser.ini"))
                {
                    loader.Save(res, tree);
                }
            }

            return;
            var c = new OAuthCredentials();
            Console.WriteLine(c.GetHashCode());
            Console.WriteLine(new UPath("12/34/5") == new UPath("12\\34\\5"));

            return;
            Work().Wait();
        }

        static async Task Work()
        {


            var task = await new YandexHostingManager().AuthorizeAsync();
            //Console.WriteLine(task.Result);
            //var token = task.Result;
            var tfcmToken = "AQAEA7qiAp8AAAQv83T09kxsZEXmiSIla-bv4ho";

            var token = "AQAEA7qhwJsLAAQv81iOHoUvvkxzjVPcOVYNKUs";
            var hosting = new YandexHostingManager().GetFileHostingFor(new OAuthCredentials());
            //Console.WriteLine(await hosting.FreeSpaceAsync());
            //Console.WriteLine(await hosting.UsedSpaceAsync());
            //Console.WriteLine(await hosting.TotalSpaceAsync());

            //using (var file = File.OpenRead("21164947493733.jpg"))
            //{
            //    await hosting.UploadFileAsync(file, "21164947493733.jpg");
            //}

            //await hosting.MakeDirectoryAsync("A");

            await hosting.MakeDirectoryAsync("Empty dir");

            return;
            Console.WriteLine("----");

            //await hosting.RemoveFileAsync("A", "21164947493733.jpg");
            await hosting.MoveFileAsync("21164947493733.jpg", "A/21164947493733.jpg");

            using (var file = File.OpenWrite("downloaded 21164947493733.jpg"))
            {
                await hosting.DownloadFileAsync(file, "A/21164947493733.jpg", new ProgressProvider());
            }

            foreach (var info in await hosting.GetDirectoryListAsync(""))
            {
                //Console.WriteLine(info.Path);
                //Console.WriteLine(info.Size);
                //Console.WriteLine(info.IsDirectory);
                //Console.WriteLine(info.LastWriteTime);
                //Console.WriteLine();
            }

            Console.ReadKey();
        }
    }

    class ProgressProvider : IProgress<double>
    {
        public void Report(double value)
        {
            Console.WriteLine(value);
        }
    }
}
