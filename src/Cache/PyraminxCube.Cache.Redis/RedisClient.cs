using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace PyraminxCube.Cache.Redis
{
    /// <summary>
    /// Redis客户端
    /// </summary>
    public class RedisClient : IDisposable
    {
        /// <summary>
        /// 默认数据库
        /// </summary>
        public IDatabase Database => Connection.GetDatabase(DefaultDataBase);
        /// <summary>
        /// Connection
        /// </summary>
        public ConnectionMultiplexer Connection { get; }
        internal ConfigurationOptions ConfigurationOptions { get; }
        /// <summary>
        /// 默认数据库序号
        /// </summary>
        public int DefaultDataBase { get; }
        private bool _disposed;
        public RedisClient(IOptions<RedisOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);

            ConfigurationOptions = options.Value.ConfigurationOptions;
            //使用线性重试策略，5秒的间隔
            ConfigurationOptions.ReconnectRetryPolicy = new LinearRetry(5000);

            DefaultDataBase = ConfigurationOptions.DefaultDatabase ?? 0;
            Connection = ConnectionMultiplexer.Connect(ConfigurationOptions);
        }
        /// <summary>
        /// 刷新数据库
        /// </summary>
        public async Task FlushDatabaseAsync(int database = -1)
        {
            var servers = Connection.GetServers();
            foreach (var server in servers)
            {
                if (server.IsConnected && !server.IsReplica)
                {
                    await server.FlushDatabaseAsync(database == -1 ? DefaultDataBase : database);
                }
            }
        }

        /// <summary>
        /// 刷新数据库
        /// </summary>
        public void FlushDatabase(int database = -1)
        {
            var servers = Connection.GetServers();
            foreach (var server in servers)
            {
                if (server.IsConnected && !server.IsReplica)
                {
                    server.FlushDatabase(database == -1 ? DefaultDataBase : database);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                Connection.Dispose();
            }
            _disposed = true;
        }
    }

}
