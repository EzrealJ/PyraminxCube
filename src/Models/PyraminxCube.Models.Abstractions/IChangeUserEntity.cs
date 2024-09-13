using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Models.Abstractions
{
    public interface IChangeUserEntity<TUserId, TChangeTime>
    {
        public TUserId CreateUserId { get; set; }
        public TChangeTime CreateTime { get; set; }
        public TUserId ModifyUserId { get; set; }
        public TChangeTime ModifyTime { get; set; }
    }
}
