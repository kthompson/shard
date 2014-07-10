using System;
using System.Collections.Generic;
using System.Reflection;

namespace Shard
{
    class Conventions : IConventions
    {
        readonly Dictionary<Type, PropertyInfo> _idCache = new Dictionary<Type, PropertyInfo>();

        PropertyInfo GetIdPropertyInternal(Type type)
        {
            return type.GetProperty("Id") ?? type.GetProperty("ID");
        }

        public string GetStoragePrefix<T>()
        {
            return typeof(T).Name.ToLower();
        }

        public PropertyInfo GetIdProperty<T>()
        {
            var type = typeof(T);
            if (!_idCache.ContainsKey(type))
            {
                return _idCache[type] = GetIdPropertyInternal(type);
            }

            return _idCache[type];
        }

        public string GetFullId<T>(string id)
        {
            if (id == null)
                return null;

            if(!id.Contains("/"))
                return GetStoragePrefix<T>() + "/" + id;

            return id;
        }

        public string GetIdForEntity<T>(T entity)
        {
            var prop = GetIdProperty<T>();
            if (prop == null)
                return null;

            var id = prop.GetValue(entity, null);

            if (id == null)
                return null;

            if (id is string)
            {
                var s = (string) id;
                return s == string.Empty ? null : s;
            }

            if (id is long && (long)id == 0)
                return null;

            if (id is int && (int)id == 0)
                return null;

            return GetFullId<T>(id.ToString());
        }
    }
}