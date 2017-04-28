using System;
using System.Collections.Generic;
using System.Linq;
using CloudMerger.Core;

namespace CloudMerger.HostingsManager
{
    public class ServicesCollection
    {
        public ServicesCollection(IHostingManager[] managers, IMultiHostingManager[] multiHostingManagers)
        {
            try
            {
                this.managers = managers.ToDictionary(m => m.Name.ToLower(), m => m);
                this.multiHostingManagers = multiHostingManagers.ToDictionary(m => m.Name.ToLower(), m => m);

                if (this.managers.Keys.Intersect(this.multiHostingManagers.Keys).Count() != 0)
                    throw new ArgumentException($"{nameof(managers)} and {nameof(multiHostingManagers)} has equal keys");
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException("Not unique key", ex);
            }
        }

        public IEnumerable<string> Managers => managers.Keys;
        public IEnumerable<string> MultiHostingManagers => multiHostingManagers.Keys;

        public bool IsContainsManager(string name) => managers.ContainsKey(name.ToLower());
        public bool IsContainsMultiHostingManager(string name) => multiHostingManagers.ContainsKey(name.ToLower());
        public bool IsContains(string name) => IsContainsManager(name) || IsContainsMultiHostingManager(name);

        public IHostingManager GetManager(string name)
        {
            if (!IsContainsManager(name))
                throw new KeyNotFoundException();
            return managers[name.ToLower()];
        }

        public IMultiHostingManager GetMultiHostingManager(string name)
        {
            if (!IsContainsMultiHostingManager(name))    
                throw new KeyNotFoundException();
            return multiHostingManagers[name.ToLower()];
        }
        
        private readonly Dictionary<string, IHostingManager> managers;
        private readonly Dictionary<string, IMultiHostingManager> multiHostingManagers;
    }
}
