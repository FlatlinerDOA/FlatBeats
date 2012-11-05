//--------------------------------------------------------------------------------------------------
// <copyright file="Locator.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Flatliner.Phone.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// In memory instance locator, each key must be unique only within it's own type.
    /// </summary>
    public static class Locator
    {
        private static readonly Dictionary<string, object> List = new Dictionary<string, object>();

        private static readonly object SyncRoot = new object();

        public static IEnumerable<T> GetAll<T>()
        {
            lock (SyncRoot)
            {
                return GetOrCreateAllList<T>().Where(a => !EqualityComparer<T>.Default.Equals(a, default(T)));
            }
        }

        private static IList<T> GetOrCreateAllList<T>()
        {
            var name = typeof(T).FullName ?? string.Empty;
            if (List.ContainsKey(name))
            {
                return (List<T>)List[name];
            }

            var list = new List<T>();
            List.Add(name, list);
            return list;
        }

        public static T Get<T>(int key)
        {
            return Get<T>(key.ToString());
        }

        public static T Get<T>(string key)
        {
            lock (SyncRoot)
            {
                var fullKey = typeof(T).FullName + "_" + key;
                if (List.ContainsKey(fullKey))
                {
                    return (T)List[fullKey];
                }

                return default(T);
            }
        }

        public static void Set<T>(int key, T item)
        {
            Set(key.ToString(), item);
        }

        public static void Set<T>(string key, T item)
        {
            lock (SyncRoot)
            {
                var fullKey = typeof(T).FullName + "_" + key;
                var allList = GetOrCreateAllList<T>();
                if (List.ContainsKey(fullKey))
                {
                    allList.Remove((T)List[fullKey]);
                    List[fullKey] = item;
                }
                else
                {
                    List.Add(fullKey, item);
                }

                allList.Add(item);
            }
        }
    }
}
