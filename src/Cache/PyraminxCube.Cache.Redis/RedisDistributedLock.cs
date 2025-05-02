using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PyraminxCube.Cache.Abstractions;
using PyraminxCube.Utils.DistributedLock;
using StackExchange.Redis;

namespace PyraminxCube.Cache.Redis
{
    /// <summary>
    /// Redis分布式锁
    /// </summary>
    internal partial class RedisDistributedLock : ISyncDistributedLock, IAsyncDistributedLock
    {
        private readonly IDatabase _db;
        private readonly ILogger _logger;
        private readonly int _millisecondsDelay = 5;
        private readonly IDistributedCacheKeyNormalizer _keyNormalizer;
        private static string _machineName = Environment.MachineName;

        public static string MachineName { get => _machineName; set => _machineName = value; }

        public RedisDistributedLock(RedisClient client,
            IDistributedCacheKeyNormalizer keyNormalizer,
            ILogger<RedisDistributedLock> logger)
        {
            ArgumentNullException.ThrowIfNull(client);

            _db = client.Database;
            this._keyNormalizer = keyNormalizer;
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }





        private string NormalizeKey(string key) => _keyNormalizer.NormalizeKey(new DistributedCacheKeyNormalizeArgs(key, "redis_distributed_lock"));



        /// <summary>
        /// 获取锁定值
        /// </summary>
        /// <returns>返回一个不相同的值</returns>
        private static string GetLockValue(string token) => $"{token}_{MachineName}";


    }

}
