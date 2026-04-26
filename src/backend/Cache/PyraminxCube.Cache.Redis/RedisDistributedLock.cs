using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using PyraminxCube.Cache.Abstractions;
using PyraminxCube.Utils.DistributedLock;
using StackExchange.Redis;

namespace PyraminxCube.Cache.Redis
{
    /// <summary>
    /// Redis分布式锁实现
    /// </summary>
    /// <remarks>
    /// 特性：
    /// - 支持取消令牌 (CancellationToken)
    /// - 指数退避 + 抖动策略
    /// - 自动续期支持
    /// - 锁状态检查
    /// - 可观测性 (Metrics)
    /// </remarks>
    internal class RedisDistributedLock : IDistributedLock
    {
        private readonly IDatabase _db;
        private readonly ILogger _logger;
        private readonly IDistributedCacheKeyNormalizer _keyNormalizer;

        // 指数退避配置
        private const int InitialDelayMs = 10;       // 初始延迟 10ms
        private const int MaxDelayMs = 1000;         // 最大延迟 1000ms
        private const double BackoffFactor = 2.0;    // 退避因子
        private const double JitterFactor = 0.2;     // 抖动因子 ±20%

        // Metrics
        private static readonly Meter Meter = new("PyraminxCube.DistributedLock");
        private static readonly Counter<long> LockAcquiredCounter = Meter.CreateCounter<long>("lock_acquired_total", "次", "锁获取成功次数");
        private static readonly Counter<long> LockFailedCounter = Meter.CreateCounter<long>("lock_failed_total", "次", "锁获取失败次数");
        private static readonly Histogram<double> LockWaitDuration = Meter.CreateHistogram<double>("lock_wait_duration_ms", "毫秒", "锁等待时长");
        private static readonly Counter<long> LockRenewalFailedCounter = Meter.CreateCounter<long>("lock_renewal_failed_total", "次", "锁续期失败次数");

        private static string _machineName = Environment.MachineName;

        /// <summary>
        /// 机器名（用于锁值，可通过测试替换）
        /// </summary>
        public static string MachineName { get => _machineName; set => _machineName = value; }

        public RedisDistributedLock(
            RedisClient client,
            IDistributedCacheKeyNormalizer keyNormalizer,
            ILogger<RedisDistributedLock> logger)
        {
            ArgumentNullException.ThrowIfNull(client);
            _db = client.Database;
            _keyNormalizer = keyNormalizer;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Lock Sync

        public void Lock(string token, TimeSpan timeout, Action action, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue();
            SpinWaitForLock(key, value, timeout, cancellationToken);
            try
            {
                action();
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public void Lock<TState>(string token, TimeSpan timeout, Action<TState> action, TState state, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue();
            SpinWaitForLock(key, value, timeout, cancellationToken);
            try
            {
                action(state);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult Lock<TResult>(string token, TimeSpan timeout, Func<TResult> func, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();
            SpinWaitForLock(key, value, timeout, cancellationToken);
            try
            {
                return func();
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult Lock<TState, TResult>(string token, TimeSpan timeout, Func<TState, TResult> func, TState state, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();
            SpinWaitForLock(key, value, timeout, cancellationToken);
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

        public void TryLock(string token, TimeSpan timeout, Action action, Action? actionOnFailed = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue();

            if (!_db.LockTake(key, value, timeout))
            {
                RecordLockFailed(token);
                actionOnFailed?.Invoke();
                return;
            }

            RecordLockAcquired(token);
            try
            {
                action();
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public void TryLock<TState>(string token, TimeSpan timeout, Action<TState> action, TState state, Action? actionOnFailed = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue();

            if (!_db.LockTake(key, value, timeout))
            {
                RecordLockFailed(token);
                actionOnFailed?.Invoke();
                return;
            }

            RecordLockAcquired(token);
            try
            {
                action(state);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult TryLock<TResult>(string token, TimeSpan timeout, Func<TResult> func, Action? actionOnFailed = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();

            if (!_db.LockTake(key, value, timeout))
            {
                RecordLockFailed(token);
                actionOnFailed?.Invoke();
                return default!;
            }

            RecordLockAcquired(token);
            try
            {
                return func();
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult TryLock<TState, TResult>(string token, TimeSpan timeout, Func<TState, TResult> func, TState state, Action? actionOnFailed = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();

            if (!_db.LockTake(key, value, timeout))
            {
                RecordLockFailed(token);
                actionOnFailed?.Invoke();
                return default!;
            }

            RecordLockAcquired(token);
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

        public async Task LockAsync(string token, TimeSpan timeout, Func<Task> func, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();
            await SpinWaitForLockAsync(key, value, timeout, cancellationToken).ConfigureAwait(false);
            try
            {
                await func().ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task LockAsync<TState>(string token, TimeSpan timeout, Func<TState, Task> func, TState state, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();
            await SpinWaitForLockAsync(key, value, timeout, cancellationToken).ConfigureAwait(false);
            try
            {
                await func(state).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> LockAsync<TResult>(string token, TimeSpan timeout, Func<Task<TResult>> func, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();
            await SpinWaitForLockAsync(key, value, timeout, cancellationToken).ConfigureAwait(false);
            try
            {
                return await func().ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> LockAsync<TState, TResult>(string token, TimeSpan timeout, Func<TState, Task<TResult>> func, TState state, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();
            await SpinWaitForLockAsync(key, value, timeout, cancellationToken).ConfigureAwait(false);
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

        public async Task TryLockAsync(string token, TimeSpan timeout, Func<Task> func, Func<Task>? funcOnFailed = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();

            if (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                RecordLockFailed(token);
                if (funcOnFailed != null)
                {
                    await funcOnFailed().ConfigureAwait(false);
                }
                return;
            }

            RecordLockAcquired(token);
            try
            {
                await func().ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task TryLockAsync<TState>(string token, TimeSpan timeout, Func<TState, Task> func, TState state, Func<Task>? funcOnFailed = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();

            if (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                RecordLockFailed(token);
                if (funcOnFailed != null)
                {
                    await funcOnFailed().ConfigureAwait(false);
                }
                return;
            }

            RecordLockAcquired(token);
            try
            {
                await func(state).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> TryLockAsync<TResult>(string token, TimeSpan timeout, Func<Task<TResult>> func, Func<Task>? funcOnFailed = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();

            if (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                RecordLockFailed(token);
                if (funcOnFailed != null)
                {
                    await funcOnFailed().ConfigureAwait(false);
                }
                return default!;
            }

            RecordLockAcquired(token);
            try
            {
                return await func().ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> TryLockAsync<TState, TResult>(string token, TimeSpan timeout, Func<TState, Task<TResult>> func, TState state, Func<Task>? funcOnFailed = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue();

            if (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                RecordLockFailed(token);
                if (funcOnFailed != null)
                {
                    await funcOnFailed().ConfigureAwait(false);
                }
                return default!;
            }

            RecordLockAcquired(token);
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

        #region Lock Status & Auto Renewal

        public async Task<bool> IsHeldAsync(string token, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);

            string key = NormalizeKey(token);
            var ttl = await _db.KeyTimeToLiveAsync(key).ConfigureAwait(false);
            return ttl.HasValue && ttl.Value > TimeSpan.Zero;
        }

        public async Task LockWithAutoRenewalAsync(string token, TimeSpan lockTime, Func<CancellationToken, Task> func,
            TimeSpan? renewalInterval = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            var interval = renewalInterval ?? TimeSpan.FromMilliseconds(lockTime.TotalMilliseconds / 3);
            string key = NormalizeKey(token);
            string value = GetLockValue();

            // 获取锁
            await SpinWaitForLockAsync(key, value, lockTime, cancellationToken).ConfigureAwait(false);

            // 创建续期取消令牌
            using var renewalCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, renewalCts.Token);

            // 启动后台续期任务
            var renewalTask = Task.Run(async () =>
            {
                while (!renewalCts.Token.IsCancellationRequested)
                {
                    await Task.Delay(interval, renewalCts.Token).ConfigureAwait(false);

                    try
                    {
                        // 续期：设置新的过期时间
                        var renewed = await _db.LockExtendAsync(key, value, lockTime).ConfigureAwait(false);
                        if (!renewed)
                        {
                            _logger.LogWarning("锁续期失败: token={Token}, 可能已被其他客户端获取", token);
                            LockRenewalFailedCounter.Add(1, new KeyValuePair<string, object?>("token", token));
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "锁续期异常: token={Token}", token);
                        LockRenewalFailedCounter.Add(1, new KeyValuePair<string, object?>("token", token));
                    }
                }
            }, renewalCts.Token);

            try
            {
                await func(linkedCts.Token).ConfigureAwait(false);
            }
            finally
            {
                // 停止续期任务
                await renewalCts.CancelAsync().ConfigureAwait(false);
                try
                {
                    await renewalTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // 预期的取消，忽略
                }

                // 释放锁
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> LockWithAutoRenewalAsync<TResult>(string token, TimeSpan lockTime, Func<CancellationToken, Task<TResult>> func,
            TimeSpan? renewalInterval = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            var interval = renewalInterval ?? TimeSpan.FromMilliseconds(lockTime.TotalMilliseconds / 3);
            string key = NormalizeKey(token);
            string value = GetLockValue();

            // 获取锁
            await SpinWaitForLockAsync(key, value, lockTime, cancellationToken).ConfigureAwait(false);

            // 创建续期取消令牌
            using var renewalCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, renewalCts.Token);

            // 启动后台续期任务
            var renewalTask = Task.Run(async () =>
            {
                while (!renewalCts.Token.IsCancellationRequested)
                {
                    await Task.Delay(interval, renewalCts.Token).ConfigureAwait(false);

                    try
                    {
                        var renewed = await _db.LockExtendAsync(key, value, lockTime).ConfigureAwait(false);
                        if (!renewed)
                        {
                            _logger.LogWarning("锁续期失败: token={Token}, 可能已被其他客户端获取", token);
                            LockRenewalFailedCounter.Add(1, new KeyValuePair<string, object?>("token", token));
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "锁续期异常: token={Token}", token);
                        LockRenewalFailedCounter.Add(1, new KeyValuePair<string, object?>("token", token));
                    }
                }
            }, renewalCts.Token);

            try
            {
                return await func(linkedCts.Token).ConfigureAwait(false);
            }
            finally
            {
                // 停止续期任务
                await renewalCts.CancelAsync().ConfigureAwait(false);
                try
                {
                    await renewalTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // 预期的取消，忽略
                }

                // 释放锁
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        #endregion

        #region Private Methods

        private string NormalizeKey(string key) =>
            _keyNormalizer.NormalizeKey(new DistributedCacheKeyNormalizeArgs(key, "redis_distributed_lock"));

        /// <summary>
        /// 获取锁定值（每次获取锁生成唯一值）
        /// </summary>
        private static string GetLockValue() => $"{_machineName}_{Guid.NewGuid():N}";

        /// <summary>
        /// 同步自旋等待获取锁（指数退避 + 抖动）
        /// </summary>
        private void SpinWaitForLock(string key, string value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            int delayMs = InitialDelayMs;
            int attempt = 0;

            while (!_db.LockTake(key, value, timeout))
            {
                cancellationToken.ThrowIfCancellationRequested();

                attempt++;
                int actualDelay = CalculateDelayWithJitter(delayMs);

                _logger.LogDebug("等待获取锁: key={Key}, 尝试次数={Attempt}, 延迟={Delay}ms",
                    key, attempt, actualDelay);

                Thread.Sleep(actualDelay);
                delayMs = Math.Min((int)(delayMs * BackoffFactor), MaxDelayMs);
            }

            sw.Stop();
            RecordLockAcquired(key, sw.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// 异步自旋等待获取锁（指数退避 + 抖动）
        /// </summary>
        private async Task SpinWaitForLockAsync(string key, string value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            int delayMs = InitialDelayMs;
            int attempt = 0;

            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                attempt++;
                int actualDelay = CalculateDelayWithJitter(delayMs);

                _logger.LogDebug("等待获取锁: key={Key}, 尝试次数={Attempt}, 延迟={Delay}ms",
                    key, attempt, actualDelay);

                await Task.Delay(actualDelay, cancellationToken).ConfigureAwait(false);
                delayMs = Math.Min((int)(delayMs * BackoffFactor), MaxDelayMs);
            }

            sw.Stop();
            RecordLockAcquired(key, sw.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// 计算带抖动的延迟时间
        /// </summary>
        private static int CalculateDelayWithJitter(int baseDelayMs)
        {
            // 添加 ±20% 的随机抖动
            double jitter = baseDelayMs * JitterFactor * (Random.Shared.NextDouble() * 2 - 1);
            return Math.Max(1, (int)(baseDelayMs + jitter));
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        private void LockRelease(string key, string value)
        {
            if (!_db.LockRelease(key, value))
            {
                _logger.LogWarning("锁释放失败: key={Key}, value={Value}, 可能已过期或被其他客户端持有", key, value);
            }
        }

        /// <summary>
        /// 异步释放锁
        /// </summary>
        private async Task LockReleaseAsync(string key, string value)
        {
            if (!await _db.LockReleaseAsync(key, value).ConfigureAwait(false))
            {
                _logger.LogWarning("锁释放失败: key={Key}, value={Value}, 可能已过期或被其他客户端持有", key, value);
            }
        }

        #endregion

        #region Metrics Helpers

        private void RecordLockAcquired(string token, double waitDurationMs = 0)
        {
            LockAcquiredCounter.Add(1, new KeyValuePair<string, object?>("token", token));
            if (waitDurationMs > 0)
            {
                LockWaitDuration.Record(waitDurationMs, new KeyValuePair<string, object?>("token", token));
            }
        }

        private void RecordLockFailed(string token)
        {
            LockFailedCounter.Add(1, new KeyValuePair<string, object?>("token", token));
        }

        #endregion
    }
}
