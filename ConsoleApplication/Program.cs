﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Utility;
using CloudMerger.GuiPrimitives;
using CloudMerger.HostingsManager;
using CloudMerger.HostingsManager.Tree;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Factory;

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

            kernel.Bind<ServicesCollection>().ToSelf().InSingletonScope();
            kernel.Bind<Application>().ToSelf().InSingletonScope();
            kernel.Bind<HostingTreeBuilder>().ToSelf().InSingletonScope();
            kernel.Bind<CredentialsFormatter>().ToSelf().InSingletonScope();

            kernel.Get<Application>().Run(args);
        }
    }
}
