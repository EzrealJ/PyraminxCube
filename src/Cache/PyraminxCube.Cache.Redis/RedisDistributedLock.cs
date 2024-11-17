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
    internal class RedisDistributedLock : IDistributedLock
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

        #region Lock Sync
        public void Lock(string token, TimeSpan timeout, Action action)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!_db.LockTake(key, value, timeout))
            {
                Thread.Sleep(_millisecondsDelay);
                Thread.Sleep(1000);
                Task.Delay(1000).Wait();
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

        public void Lock<TArgs1>(string token, TimeSpan timeout, Action<TArgs1> action, TArgs1 args1)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!_db.LockTake(key, value, timeout))
            {
                Thread.Sleep(_millisecondsDelay);
            }
            try
            {
                action(args1);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public void Lock<TArgs1, TArgs2>(string token, TimeSpan timeout, Action<TArgs1, TArgs2> action, TArgs1 args1, TArgs2 args2)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!_db.LockTake(key, value, timeout))
            {
                Thread.Sleep(_millisecondsDelay);
            }
            try
            {
                action(args1, args2);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public void Lock<TArgs1, TArgs2, TArgs3>(string token, TimeSpan timeout, Action<TArgs1, TArgs2, TArgs3> action, TArgs1 args1, TArgs2 args2, TArgs3 args3)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!_db.LockTake(key, value, timeout))
            {
                Thread.Sleep(_millisecondsDelay);
            }
            try
            {
                action(args1, args2, args3);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public void Lock<TArgs1, TArgs2, TArgs3, TArgs4>(string token, TimeSpan timeout, Action<TArgs1, TArgs2, TArgs3, TArgs4> action, TArgs1 args1, TArgs2 args2, TArgs3 args3, TArgs4 args4)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!_db.LockTake(key, value, timeout))
            {
                Thread.Sleep(_millisecondsDelay);
            }
            try
            {
                action(args1, args2, args3, args4);
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
            while (!_db.LockTake(key, value, timeout))
            {
                Thread.Sleep(_millisecondsDelay);
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

        public TResult Lock<TArgs1, TResult>(string token, TimeSpan timeout, Func<TArgs1, TResult> func, TArgs1 args1)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!_db.LockTake(key, value, timeout))
            {
                Thread.Sleep(_millisecondsDelay);
            }
            try
            {
                return func(args1);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult Lock<TArgs1, TArgs2, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TResult> func, TArgs1 args1, TArgs2 args2)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!_db.LockTake(key, value, timeout))
            {
                Thread.Sleep(_millisecondsDelay);
            }
            try
            {
                return func(args1, args2);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult Lock<TArgs1, TArgs2, TArgs3, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, TResult> func, TArgs1 args1, TArgs2 args2, TArgs3 args3)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!_db.LockTake(key, value, timeout))
            {
                Thread.Sleep(_millisecondsDelay);
            }
            try
            {
                return func(args1, args2, args3);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult Lock<TArgs1, TArgs2, TArgs3, TArgs4, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, TArgs4, TResult> func, TArgs1 args1, TArgs2 args2, TArgs3 args3, TArgs4 args4)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!_db.LockTake(key, value, timeout))
            {
                Thread.Sleep(_millisecondsDelay);
            }
            try
            {
                return func(args1, args2, args3, args4);
            }
            finally
            {
                LockRelease(key, value);
            }
        }
        #endregion

        #region TryLock Sync
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="action">锁定后的回调方法</param>
        /// <param name="actionOnFailed">错误回调方法</param>
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
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="action">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        public void TryLock<TArgs1>(string token, TimeSpan timeout, Action<TArgs1> action, TArgs1 args1, Action? actionOnFailed = null)
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
                action(args1);
            }
            finally
            {
                LockRelease(key, value);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="action">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        public void TryLock<TArgs1, TArgs2>(string token, TimeSpan timeout, Action<TArgs1, TArgs2> action, TArgs1 args1, TArgs2 args2, Action? actionOnFailed = null)
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
                action(args1, args2);
            }
            finally
            {
                LockRelease(key, value);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <typeparam name="TArgs3">参数3类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="action">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        public void TryLock<TArgs1, TArgs2, TArgs3>(string token, TimeSpan timeout, Action<TArgs1, TArgs2, TArgs3> action, TArgs1 args1, TArgs2 args2, TArgs3 args3, Action? actionOnFailed = null)
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
                action(args1, args2, args3);
            }
            finally
            {
                LockRelease(key, value);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <typeparam name="TArgs3">参数3类型</typeparam>
        /// <typeparam name="TArgs4">参数4类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="action">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        public void TryLock<TArgs1, TArgs2, TArgs3, TArgs4>(string token, TimeSpan timeout, Action<TArgs1, TArgs2, TArgs3, TArgs4> action, TArgs1 args1, TArgs2 args2, TArgs3 args3, TArgs4 args4, Action? actionOnFailed = null)
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
                action(args1, args2, args3, args4);
            }
            finally
            {
                LockRelease(key, value);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="actionOnFailed">错误回调方法</param>
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
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        public TResult TryLock<TArgs1, TResult>(string token, TimeSpan timeout, Func<TArgs1, TResult> func, TArgs1 args1, Action? actionOnFailed = null)
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
                return func(args1);
            }
            finally
            {
                LockRelease(key, value);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        public TResult TryLock<TArgs1, TArgs2, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TResult> func, TArgs1 args1, TArgs2 args2, Action? actionOnFailed = null)
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
                return func(args1, args2);
            }
            finally
            {
                LockRelease(key, value);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <typeparam name="TArgs3">参数3类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        public TResult TryLock<TArgs1, TArgs2, TArgs3, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, TResult> func, TArgs1 args1, TArgs2 args2, TArgs3 args3, Action? actionOnFailed = null)
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
                return func(args1, args2, args3);
            }
            finally
            {
                LockRelease(key, value);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <typeparam name="TArgs3">参数3类型</typeparam>
        /// <typeparam name="TArgs4">参数4类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        public TResult TryLock<TArgs1, TArgs2, TArgs3, TArgs4, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, TArgs4, TResult> func, TArgs1 args1, TArgs2 args2, TArgs3 args3, TArgs4 args4, Action? actionOnFailed = null)
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
                return func(args1, args2, args3, args4);
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
            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
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

        public async Task LockAsync<TArgs1>(string token, TimeSpan timeout, Func<TArgs1, Task> func, TArgs1 args1)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
            }
            try
            {
                await func(args1).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task LockAsync<TArgs1, TArgs2>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, Task> func, TArgs1 args1, TArgs2 args2)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
            }
            try
            {
                await func(args1, args2).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task LockAsync<TArgs1, TArgs2, TArgs3>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, Task> func, TArgs1 args1, TArgs2 args2, TArgs3 args3)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
            }
            try
            {
                await func(args1, args2, args3).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task LockAsync<TArgs1, TArgs2, TArgs3, TArgs4>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, TArgs4, Task> func, TArgs1 args1, TArgs2 args2, TArgs3 args3, TArgs4 args4)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
            }
            try
            {
                await func(args1, args2, args3, args4).ConfigureAwait(false);
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
            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
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

        public async Task<TResult> LockAsync<TArgs1, TResult>(string token, TimeSpan timeout, Func<TArgs1, Task<TResult>> func, TArgs1 args1)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
            }
            try
            {
                return await func(args1).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> LockAsync<TArgs1, TArgs2, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, Task<TResult>> func, TArgs1 args1, TArgs2 args2)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
            }
            try
            {
                return await func(args1, args2).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> LockAsync<TArgs1, TArgs2, TArgs3, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, Task<TResult>> func, TArgs1 args1, TArgs2 args2, TArgs3 args3)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
            }
            try
            {
                return await func(args1, args2, args3).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> LockAsync<TArgs1, TArgs2, TArgs3, TArgs4, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, TArgs4, Task<TResult>> func, TArgs1 args1, TArgs2 args2, TArgs3 args3, TArgs4 args4)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            while (!await _db.LockTakeAsync(key, value, timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
            }
            try
            {
                return await func(args1, args2, args3, args4).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }


        #endregion

        #region TryLock ASync
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="funcOnFailed">错误回调方法</param>
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
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        public async Task TryLockAsync<TArgs1>(string token, TimeSpan timeout, Func<TArgs1, Task> func, TArgs1 args1, Func<Task>? funcOnFailed = null)
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
                await func(args1).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        public async Task TryLockAsync<TArgs1, TArgs2>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, Task> func, TArgs1 args1, TArgs2 args2, Func<Task>? funcOnFailed = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(token);
            string value = GetLockValue(token);
            if (!await _db.LockTakeAsync(key, token, timeout))
            {
                if (funcOnFailed != null)
                {
                    await funcOnFailed().ConfigureAwait(false);
                }
                return;
            }

            try
            {
                await func(args1, args2).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <typeparam name="TArgs3">参数3类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        public async Task TryLockAsync<TArgs1, TArgs2, TArgs3>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, Task> func, TArgs1 args1, TArgs2 args2, TArgs3 args3, Func<Task>? funcOnFailed = null)
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
                await func(args1, args2, args3).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }

        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <typeparam name="TArgs3">参数3类型</typeparam>
        /// <typeparam name="TArgs4">参数4类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        public async Task TryLockAsync<TArgs1, TArgs2, TArgs3, TArgs4>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, TArgs4, Task> func, TArgs1 args1, TArgs2 args2, TArgs3 args3, TArgs4 args4, Func<Task>? funcOnFailed = null)
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
                await func(args1, args2, args3, args4).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="funcOnFailed">错误回调方法</param>
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
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        public async Task<TResult> TryLockAsync<TArgs1, TResult>(string token, TimeSpan timeout, Func<TArgs1, Task<TResult>> func, TArgs1 args1, Func<Task>? funcOnFailed = null)
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
                return await func(args1).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        public async Task<TResult> TryLockAsync<TArgs1, TArgs2, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, Task<TResult>> func, TArgs1 args1, TArgs2 args2, Func<Task>? funcOnFailed = null)
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
                return await func(args1, args2).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <typeparam name="TArgs3">参数3类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        public async Task<TResult> TryLockAsync<TArgs1, TArgs2, TArgs3, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, Task<TResult>> func, TArgs1 args1, TArgs2 args2, TArgs3 args3, Func<Task>? funcOnFailed = null)
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
                return await func(args1, args2, args3).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TArgs1">参数1类型</typeparam>
        /// <typeparam name="TArgs2">参数2类型</typeparam>
        /// <typeparam name="TArgs3">参数3类型</typeparam>
        /// <typeparam name="TArgs4">参数4类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁定令牌，必须确保唯一性</param>
        /// <param name="timeout">锁定过期时长</param>
        /// <param name="func">锁定后的回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        public async Task<TResult> TryLockAsync<TArgs1, TArgs2, TArgs3, TArgs4, TResult>(string token, TimeSpan timeout, Func<TArgs1, TArgs2, TArgs3, TArgs4, Task<TResult>> func, TArgs1 args1, TArgs2 args2, TArgs3 args3, TArgs4 args4, Func<Task>? funcOnFailed = null)
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
                return await func(args1, args2, args3, args4).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }
        #endregion

        private string NormalizeKey(string key) => _keyNormalizer.NormalizeKey(new DistributedCacheKeyNormalizeArgs(key, "redis_distributed_lock"));
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        private void LockRelease(string key, string value)
        {
            if (!_db.LockRelease(key, value))
            {
                _logger.LogError("value:{Value}, LockRelease is error.", value);
            }
        }
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        private async Task LockReleaseAsync(string key, string value)
        {
            if (!await _db.LockReleaseAsync(key, value).ConfigureAwait(false))
            {
                _logger.LogError("value:{Value}, LockRelease is error.", value);
            }
        }

        /// <summary>
        /// 获取锁定值
        /// </summary>
        /// <returns>返回一个不相同的值</returns>
        private static string GetLockValue(string token) => $"{token}_{MachineName}";
    }

}
