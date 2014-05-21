using System;
using System.Reflection;

namespace Shard
{
    class Conventions : IConventions
    {
        public Func<Type, PropertyInfo> GetIdProperty { get; set; }
        public Func<Type, string> GetStoragePrefix { get; set; }
        public Func<object, string> IdForEntity { get; set; }
        public Func<Type, object, string> GetFullId { get; set; }
        public Func<Type, string> GetFreeId { get; set; }
    }
}