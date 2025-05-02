using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Utils.DistributedLock
{
    public class LockOptions
    {
        public LockOptions()
        {
        }

        public LockOptions(string token, TimeSpan timeout)
        {
            Token = token;
            Timeout = timeout;
        }

        public string Token { get; set; } = string.Empty;
        public TimeSpan Timeout { get; set; }
    }
}
