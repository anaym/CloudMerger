using System;
using System.Collections.Generic;
using System.Linq;
using CloudMerger.Core;

namespace CloudMerger.HostingsManager
{
    //TODO: maybe replace to DI?
    public class ServicesCollection
    {
        //public static ServicesCollection LoadServices(DirectoryInfo directory)
        //{
        //    var managers = new 
        //    foreach (var dll in directory.GetFiles("*.dll").Select(f => f.FullName).Select(Assembly.LoadFrom))
        //    {
        //        var types = dll.GetExportedTypes()
        //            .Where(t => typeof(IHostingManager).IsAssignableFrom(t))
        //            .Where(t => !t.IsAbstract)
        //            .Where(t => !t.IsInterface)
        //            .Where(t => t.IsPublic)
        //            .ToArray();
        //        var constructors = types.Select(t => t.GetConstructor(new Type[0])).Where(c => c != null);
        //        var instances = constructors.Select(c => c.Invoke(new object[0])).Cast<IHostingManager>();
        //        foreach (var instance in instances)
        //            collection.Add(instance);
        //    }
        //    return collection;
        //}

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
