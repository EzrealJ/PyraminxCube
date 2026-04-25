using Microsoft.Extensions.Logging;
using PyraminxCube.Cache.Abstractions;
using PyraminxCube.Utils.DistributedLock;
using StackExchange.Redis;

namespace PyraminxCube.Cache.Redis
{
    /// <summary>
    /// Redis分布式锁
    /// </summary>
    internal class RedisDistributedLock : IDistributedLock
    {
        private readonly IDatabase _db;
        private readonly ILogger _logger;
        private readonly int _millisecondsDelay = 50;
        private readonly IDistributedCacheKeyNormalizer _keyNormalizer;
        private static string _machineName = Environment.MachineName;

        public static string MachineName { get => _machineName; set => _machineName = value; }

        public RedisDistributedLock(RedisClient client,
            IDistributedCacheKeyNormalizer keyNormalizer,
            ILogger<RedisDistributedLock> logger)
        {
            ArgumentNullException.ThrowIfNull(client);

            _db = client.Database;
            _keyNormalizer = keyNormalizer;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Lock Sync

        public void Lock(string token, TimeSpan timeout, Action action)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            SpinWaitForLock(key, value, timeout);
            try
            {
                action();
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public void Lock<TState>(string token, TimeSpan timeout, Action<TState> action, TState state)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            SpinWaitForLock(key, value, timeout);
            try
            {
                action(state);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult Lock<TResult>(string token, TimeSpan timeout, Func<TResult> func)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            SpinWaitForLock(key, value, timeout);
            try
            {
                return func();
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult Lock<TState, TResult>(string token, TimeSpan timeout, Func<TState, TResult> func, TState state)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            SpinWaitForLock(key, value, timeout);
            try
            {
                return func(state);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        #endregion

        #region TryLock Sync

        public void TryLock(string token, TimeSpan timeout, Action action, Action? actionOnFailed = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            if (!_db.LockTake(key, value, timeout))
            {
                actionOnFailed?.Invoke();
                return;
            }

            try
            {
                action();
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public void TryLock<TState>(string token, TimeSpan timeout, Action<TState> action, TState state, Action? actionOnFailed = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            if (!_db.LockTake(key, value, timeout))
            {
                actionOnFailed?.Invoke();
                return;
            }

            try
            {
                action(state);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult TryLock<TResult>(string token, TimeSpan timeout, Func<TResult> func, Action? actionOnFailed = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            if (!_db.LockTake(key, value, timeout))
            {
                actionOnFailed?.Invoke();
                return default!;
            }

            try
            {
                return func();
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult TryLock<TState, TResult>(string token, TimeSpan timeout, Func<TState, TResult> func, TState state, Action? actionOnFailed = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            if (!_db.LockTake(key, value, timeout))
            {
                actionOnFailed?.Invoke();
                return default!;
            }

            try
            {
                return func(state);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        #endregion

        #region Lock Async

        public async Task LockAsync(string token, TimeSpan timeout, Func<Task> func)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            await SpinWaitForLockAsync(key, value, timeout).ConfigureAwait(false);
            try
            {
                await func().ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task LockAsync<TState>(string token, TimeSpan timeout, Func<TState, Task> func, TState state)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            await SpinWaitForLockAsync(key, value, timeout).ConfigureAwait(false);
            try
            {
                await func(state).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> LockAsync<TResult>(string token, TimeSpan timeout, Func<Task<TResult>> func)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            await SpinWaitForLockAsync(key, value, timeout).ConfigureAwait(false);
            try
            {
                return await func().ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> LockAsync<TState, TResult>(string token, TimeSpan timeout, Func<TState, Task<TResult>> func, TState state)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            await SpinWaitForLockAsync(key, value, timeout).ConfigureAwait(false);
            try
            {
                return await func(state).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        #endregion

        #region TryLock Async

        public async Task TryLockAsync(string token, TimeSpan timeout, Func<Task> func, Func<Task>? funcOnFailed = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            if (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                if (funcOnFailed != null)
                {
                    await funcOnFailed().ConfigureAwait(false);
                }
                return;
            }

            try
            {
                await func().ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task TryLockAsync<TState>(string token, TimeSpan timeout, Func<TState, Task> func, TState state, Func<Task>? funcOnFailed = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            if (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                if (funcOnFailed != null)
                {
                    await funcOnFailed().ConfigureAwait(false);
                }
                return;
            }

            try
            {
                await func(state).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> TryLockAsync<TResult>(string token, TimeSpan timeout, Func<Task<TResult>> func, Func<Task>? funcOnFailed = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            if (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                if (funcOnFailed != null)
                {
                    await funcOnFailed().ConfigureAwait(false);
                }
                return default!;
            }

            try
            {
                return await func().ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> TryLockAsync<TState, TResult>(string token, TimeSpan timeout, Func<TState, Task<TResult>> func, TState state, Func<Task>? funcOnFailed = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            if (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                if (funcOnFailed != null)
                {
                    await funcOnFailed().ConfigureAwait(false);
                }
                return default!;
            }

            try
            {
                return await func(state).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        #endregion

        #region Private Methods

        private string NormalizeKey(string key) => _keyNormalizer.NormalizeKey(new DistributedCacheKeyNormalizeArgs(key, "redis_distributed_lock"));

        /// <summary>
        /// 同步自旋等待获取锁
        /// </summary>
        private void SpinWaitForLock(string key, string value, TimeSpan timeout)
        {
            while (!_db.LockTake(key, value, timeout))
            {
                Thread.Sleep(_millisecondsDelay);
            }
        }

        /// <summary>
        /// 异步自旋等待获取锁
        /// </summary>
        private async Task SpinWaitForLockAsync(string key, string value, TimeSpan timeout)
        {
            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        private void LockRelease(string key, string value)
        {
            if (!_db.LockRelease(key, value))
            {
                _logger.LogError("value:{Value}, LockRelease failed.", value);
            }
        }

        /// <summary>
        /// 异步释放锁
        /// </summary>
        private async Task LockReleaseAsync(string key, string value)
        {
            if (!await _db.LockReleaseAsync(key, value).ConfigureAwait(false))
            {
                _logger.LogError("value:{Value}, LockRelease failed.", value);
            }
        }

        /// <summary>
        /// 获取锁定值
        /// </summary>
        private static string GetLockValue(string token) => $"{token}_{MachineName}_{Environment.CurrentManagedThreadId}";

        #endregion
    }
}
