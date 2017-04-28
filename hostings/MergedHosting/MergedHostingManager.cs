using System;
using CloudMerger.Core;

namespace MergedHosting
{
    public class MergedHostingManager : IMultiHostingManager
    {
        public string Name => "Merged Hosting";

        public IHosting GetFileHostingFor(IHosting[] hostings)
        {
            return new MergedHosting(hostings, TimeSpan.FromSeconds(60));
        }
    }
}