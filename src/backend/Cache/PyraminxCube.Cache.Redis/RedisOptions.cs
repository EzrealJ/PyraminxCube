using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace PyraminxCube.Cache.Redis
{
    /// <summary>
    /// Redis设置
    /// </summary>
    public class RedisOptions
    {
        public RedisOptions() => ConfigurationOptions = new();

        /// <summary>
        /// The configuration used to connect to Redis.
        /// This is preferred over Configuration.
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; set; }
        /// <summary>
        /// 过期时长（秒）
        /// </summary>
        public long Expires { get; set; }
        /// <summary>
        /// The Redis instance name.
        /// </summary>
        public string InstanceName { get; set; } = string.Empty;
        /// <summary>
        /// 键值前缀
        /// </summary>
        public string KeyPrefix { get; set; } = string.Empty;
    }
}
