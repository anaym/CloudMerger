using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Utility;
using CloudMerger.HostingsManager;
using Ninject;
using Ninject.Extensions.Conventions;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostings = new DirectoryInfo(Environment.CurrentDirectory).GetSubDirectory("hostings");

            var kernel = new StandardKernel();
            kernel.Bind(scaner =>
            {
                scaner
                    .FromAssembliesInPath(hostings.FullName)
                    .Select(t => typeof(IHostingManager).IsAssignableFrom(t))
                    .BindAllInterfaces();
            });
            kernel.Bind(scaner =>
            {
                scaner
                    .FromAssembliesInPath(hostings.FullName)
                    .Select(t => typeof(IMultiHostingManager).IsAssignableFrom(t))
                    .BindAllInterfaces();
            });

            kernel.Bind<ServicesCollection>().ToSelf();
            kernel.Bind<Application>().ToSelf();

            var s = kernel.Get<ServicesCollection>();
            kernel.Get<Application>().Run(args);
        }
    }
}
