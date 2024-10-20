using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace PyraminxCube.Cache.Redis
{
    public class RedisDistributedCache : IDistributedCache
    {
        #region IDistributedCache
        private const string ABSOLUTE_EXPIRATION_KEY = "absexp";
        private const string SLIDING_EXPIRATION_KEY = "sldexp";
        private const string DATA_KEY = "data";
        private const string SET_SCRIPT = ($@"
                redis.call('HSET', KEYS[1], '{ABSOLUTE_EXPIRATION_KEY}', ARGV[1], '{SLIDING_EXPIRATION_KEY}', ARGV[2], '{DATA_KEY}', ARGV[4])
                if ARGV[3] ~= '-1' then
                  redis.call('EXPIRE', KEYS[1], ARGV[3])
                end
                return 1");
        [Obsolete("适用于旧版本Redis的命令")]
        private const string SET_SCRIPT_PRE_EXTENDED_SET_COMMAND = ($@"
                redis.call('HMSET', KEYS[1], '{ABSOLUTE_EXPIRATION_KEY}', ARGV[1], '{SLIDING_EXPIRATION_KEY}', ARGV[2], '{DATA_KEY}', ARGV[4])
                if ARGV[3] ~= '-1' then
                  redis.call('EXPIRE', KEYS[1], ARGV[3])
                end
                return 1");

        // combined keys - same hash keys fetched constantly; avoid allocating an array each time
        private static readonly RedisValue[] _hashMembersAbsoluteExpirationSlidingExpirationData = [ABSOLUTE_EXPIRATION_KEY, SLIDING_EXPIRATION_KEY, DATA_KEY];
        private static readonly RedisValue[] _hashMembersAbsoluteExpirationSlidingExpiration = [ABSOLUTE_EXPIRATION_KEY, SLIDING_EXPIRATION_KEY];

        private static RedisValue[] GetHashFields(bool getData) => getData
            ? _hashMembersAbsoluteExpirationSlidingExpirationData
            : _hashMembersAbsoluteExpirationSlidingExpiration;

        private const long NOT_PRESENT = -1;
        private static readonly Version _serverVersionWithExtendedSetCommand = new(4, 0, 0);

        private readonly IDatabase _cache;
        private readonly string _setScript = SET_SCRIPT;

        private readonly RedisKey _instancePrefix;
        private readonly ILogger _logger;


        /// <summary>
        /// Initializes a new instance of <see cref="RedisClient"/>.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="client">RedisClient</param>
        /// <param name="logger"></param>
        public RedisDistributedCache(IOptions<RedisOptions> options, RedisClient client, ILogger<RedisDistributedCache> logger)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            _cache = client.Database;
            _logger = logger;

            string instanceName = options.Value.InstanceName ?? string.Empty;
            if (!string.IsNullOrEmpty(instanceName))
            {
                // SE.Redis allows efficient append of key-prefix scenarios, but we can help it
                // avoid some work/allocations by forcing the key-prefix to be a byte[]; SE.Redis
                // would do this itself anyway, using UTF8
                _instancePrefix = (RedisKey)Encoding.UTF8.GetBytes(instanceName);
            }
            try
            {
                foreach (var endPoint in client.Connection.GetEndPoints())
                {
                    if (client.Connection.GetServer(endPoint).Version < _serverVersionWithExtendedSetCommand)
                    {
                        _setScript = SET_SCRIPT_PRE_EXTENDED_SET_COMMAND;
                        return;
                    }
                }
            }
            catch (NotSupportedException ex)
            {
                OnRedisError(ex);
                // The GetServer call may not be supported with some configurations, in which
                // case let's also fall back to using the older command.
                _setScript = SET_SCRIPT_PRE_EXTENDED_SET_COMMAND;
            }
        }

        /// <inheritdoc />
        public byte[] Get(string key)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            return GetAndRefresh(key, getData: true);
        }

        /// <inheritdoc />
        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            token.ThrowIfCancellationRequested();

            return await GetAndRefreshAsync(key, getData: true, token: token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            try
            {
                _cache.ScriptEvaluate(_setScript, [_instancePrefix.Append(key)],
                    [
                        absoluteExpiration?.Ticks ?? NOT_PRESENT,
                        options.SlidingExpiration?.Ticks ?? NOT_PRESENT,
                        GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NOT_PRESENT,
                        value
                    ]);
            }
            catch (Exception ex)
            {
                OnRedisError(ex);
            }
        }

        /// <inheritdoc />
        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            token.ThrowIfCancellationRequested();

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            try
            {
                await _cache.ScriptEvaluateAsync(_setScript, [_instancePrefix.Append(key)],
                    [
                        absoluteExpiration?.Ticks ?? NOT_PRESENT,
                        options.SlidingExpiration?.Ticks ?? NOT_PRESENT,
                        GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NOT_PRESENT,
                        value
                    ]).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnRedisError(ex);
            }
        }

        /// <inheritdoc />
        public void Refresh(string key)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            GetAndRefresh(key, getData: false);
        }

        /// <inheritdoc />
        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            token.ThrowIfCancellationRequested();

            await GetAndRefreshAsync(key, getData: false, token: token).ConfigureAwait(false);
        }

        private byte[] GetAndRefresh(string key, bool getData)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            RedisValue[] results = [];
            try
            {
                results = _cache.HashGet(_instancePrefix.Append(key), GetHashFields(getData), CommandFlags.PreferReplica);
            }
            catch (Exception ex)
            {
                OnRedisError(ex);
            }

            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                Refresh(key, absExpr, sldExpr);
            }

            if (results.Length >= 3 && !results[2].IsNull)
            {
                return results[2]!;
            }

            return [];
        }

        private async Task<byte[]> GetAndRefreshAsync(string key, bool getData, CancellationToken token = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            token.ThrowIfCancellationRequested();

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            RedisValue[] results = [];
            try
            {
                results = await _cache.HashGetAsync(_instancePrefix.Append(key), GetHashFields(getData),
                    CommandFlags.PreferReplica).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnRedisError(ex);
            }

            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                await RefreshAsync(key, absExpr, sldExpr, token).ConfigureAwait(false);
            }

            if (results.Length >= 3 && !results[2].IsNull)
            {
                return results[2]!;
            }

            return [];
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            try
            {
                _cache.KeyDelete(_instancePrefix.Append(key), CommandFlags.FireAndForget);
            }
            catch (Exception ex)
            {
                OnRedisError(ex);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            try
            {
                await _cache.KeyDeleteAsync(_instancePrefix.Append(key), CommandFlags.FireAndForget).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnRedisError(ex);
            }
        }

        private static void MapMetadata(RedisValue[] results, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            long? absoluteExpirationTicks = (long?)results[0];
            if (absoluteExpirationTicks.HasValue && absoluteExpirationTicks.Value != NOT_PRESENT)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            }
            long? slidingExpirationTicks = (long?)results[1];
            if (slidingExpirationTicks.HasValue && slidingExpirationTicks.Value != NOT_PRESENT)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
            }
        }

        private void Refresh(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            if (sldExpr.HasValue)
            {
                TimeSpan? expr;
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                try
                {
                    _cache.KeyExpire(_instancePrefix.Append(key), expr, flags: CommandFlags.FireAndForget);
                }
                catch (Exception ex)
                {
                    OnRedisError(ex);
                }
            }
        }

        private async Task RefreshAsync(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr, CancellationToken token = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            token.ThrowIfCancellationRequested();

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            if (sldExpr.HasValue)
            {
                TimeSpan? expr;
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                try
                {
                    await _cache.KeyExpireAsync(_instancePrefix.Append(key), expr, flags: CommandFlags.FireAndForget).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    OnRedisError(ex);
                }
            }
        }

        private static long? GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration, DistributedCacheEntryOptions options)
        {
            if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
            {
                return (long)Math.Min(
                    (absoluteExpiration.Value - creationTime).TotalSeconds,
                    options.SlidingExpiration.Value.TotalSeconds);
            }
            else if (absoluteExpiration.HasValue)
            {
                return (long)(absoluteExpiration.Value - creationTime).TotalSeconds;
            }
            else if (options.SlidingExpiration.HasValue)
            {
                return (long)options.SlidingExpiration.Value.TotalSeconds;
            }
            return null;
        }

        private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue)
            {
                ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.AbsoluteExpiration.Value, creationTime, nameof(DistributedCacheEntryOptions.AbsoluteExpiration));
            }

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                return creationTime + options.AbsoluteExpirationRelativeToNow;
            }

            return options.AbsoluteExpiration;
        }

        private void OnRedisError(Exception exception)
        {
            string message = exception.Message;
            _logger.LogError(exception, "{message}", message);
        }
        #endregion 
    }
}
