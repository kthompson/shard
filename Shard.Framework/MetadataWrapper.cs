using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shard
{
    interface IMetadataWrapper
    {
        string Id { get; set; }
        string Type { get; set; }
        object GetSource();
    }

    class MetadataWrapper<T> : IMetadataWrapper
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public T Source { get; set; }

        public object GetSource()
        {
            return this.Source;
        }
    }
}
