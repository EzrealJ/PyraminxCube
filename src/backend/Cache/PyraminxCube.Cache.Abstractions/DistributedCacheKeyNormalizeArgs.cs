using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Cache.Abstractions
{
    public class DistributedCacheKeyNormalizeArgs(string key, string prefix)
    {
        private readonly string _key = key;
        private readonly string _prefix = prefix;

        public string Key => _key;

        public string Prefix => _prefix;
    }
}
