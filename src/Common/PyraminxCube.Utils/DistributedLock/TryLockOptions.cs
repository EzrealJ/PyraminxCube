using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Utils.DistributedLock
{
    public class TryLockOptions : LockOptions
    {
        public TryLockOptions()
        {
        }

        public TryLockOptions(string token, TimeSpan timeout) : base(token, timeout)
        {
        }

        public Action? OnFailed { get; set; }
    }
}
