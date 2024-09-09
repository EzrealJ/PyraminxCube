using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Models.Abstractions
{
    public interface IChangeUserEntity<TUserId, TChangeTime>
    {
        public TUserId CreateUserId { get; }
        public TChangeTime CreateTime { get; }
        public TUserId ModifyUserId { get; }
        public TChangeTime ModifyTime { get; }
    }
}
