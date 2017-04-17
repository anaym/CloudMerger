using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CloudMerger.Core;

namespace CloudMerger.Application
{
    //TODO: maybe replace to DI?
    public class ServicesCollection : IReadOnlyDictionary<string, IService>
    {
        public static ServicesCollection LoadServices(DirectoryInfo directory)
        {
            var collection = new ServicesCollection();
            foreach (var dll in directory.GetFiles("*.dll").Select(f => f.FullName).Select(Assembly.LoadFrom))
            {
                var types = dll.GetExportedTypes()
                    .Where(t => typeof(IService).IsAssignableFrom(t))
                    .Where(t => !t.IsAbstract)
                    .Where(t => !t.IsInterface)
                    .Where(t => t.IsPublic)
                    .ToArray();
                var constructors = types.Select(t => t.GetConstructor(new Type[0])).Where(c => c != null);
                var instances = constructors.Select(c => c.Invoke(new object[0])).Cast<IService>();
                foreach (var instance in instances)
                    collection.Add(instance);
            }
            return collection;
        }

        public ServicesCollection()
        {
            services = new Dictionary<string, IService>();
        }

        public void Add(IService service)
        {
            if (services.ContainsKey(service.Name.ToLower()))
                throw new InvalidOperationException("Servie already exist");
            services.Add(service.Name.ToLower(), service);
        }

        public bool ContainsKey(string key) => services.ContainsKey(key.ToLower());
        public bool TryGetValue(string key, out IService value) => services.TryGetValue(key.ToLower(), out value);
        public IService this[string key] => services[key.ToLower()];

        public IEnumerator<KeyValuePair<string, IService>> GetEnumerator() => services.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => services.Count;
        public IEnumerable<string> Keys => services.Keys;
        public IEnumerable<IService> Values => services.Values;

        private readonly Dictionary<string, IService> services;
    }
}
