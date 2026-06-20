using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FFXIVVenues.Veni.Utils
{
    internal class TypeMap<T> : IEnumerable<T> where T : class
    {

        private readonly Dictionary<string, Type> _typeMap = new();
        private readonly IServiceProvider _serviceProvider;

        public TypeMap(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TypeMap<T> Add<A>(string key) where A : T
        {
            _typeMap.Add(key, typeof(A));
            return this;
        }

        public TypeMap<T> Add(string key, Type type)
        {
            _typeMap.Add(key, type);
            return this;
        }
        
        public Type Get(string key)
        {
            return _typeMap[key];
        }

        public T Activate(string key)
        {
            var hasKey = _typeMap.ContainsKey(key);
            if (!hasKey)
            {
                return default;
            }
            return ActivatorUtilities.CreateInstance(_serviceProvider, _typeMap[key]) as T;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var key in this._typeMap.Keys)
                yield return this.Activate(key);
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            this.GetEnumerator();

    }
}
