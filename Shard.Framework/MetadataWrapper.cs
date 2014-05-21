using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shard
{
    class MetadataWrapper<T>
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public T Source { get; set; }
    }
}
