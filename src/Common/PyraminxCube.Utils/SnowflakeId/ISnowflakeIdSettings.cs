using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Utils.SnowflakeId
{
    public interface ISnowflakeIdSettings
    {
        /// <summary>
        /// 终端ID
        /// </summary>
        long WorkerId { get; }
        /// <summary>
        /// 数据中心ID
        /// </summary>
        long DataCenterId { get; }
    }
}
