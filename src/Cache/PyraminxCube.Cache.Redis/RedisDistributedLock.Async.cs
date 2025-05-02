using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PyraminxCube.Utils.DistributedLock;

namespace PyraminxCube.Cache.Redis
{
    internal partial class RedisDistributedLock
    {
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

        #region Lock Async


        public async Task LockAsync(LockOptions options, Func<Task> func)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            while (!await _db.LockTakeAsync(key, value, options.Timeout).ConfigureAwait(false))
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

        public async Task LockAsync(LockOptions options, Func<object[], Task> func, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            while (!await _db.LockTakeAsync(key, value, options.Timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
            }
            try
            {
                await func(args).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> LockAsync<TResult>(LockOptions options, Func<object[], Task<TResult>> func, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            while (!await _db.LockTakeAsync(key, value, options.Timeout).ConfigureAwait(false))
            {
                await Task.Delay(_millisecondsDelay).ConfigureAwait(false);
            }
            try
            {
                return await func(args).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }
        #endregion


        #region TryLock Async

        public async Task TryLockAsync(TryLockOptions options, Func<Task> func)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            if (!await _db.LockTakeAsync(key, value, options.Timeout).ConfigureAwait(false))
            {
                if (options.OnFailed != null)
                {
                    await Task.Run(options.OnFailed).ConfigureAwait(false);
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

        public async Task TryLockAsync(TryLockOptions options, Func<object[], Task> func, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            if (!await _db.LockTakeAsync(key, value, options.Timeout).ConfigureAwait(false))
            {
                if (options.OnFailed != null)
                {
                    await Task.Run(options.OnFailed).ConfigureAwait(false);
                }
                return;
            }

            try
            {
                await func(args).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }

        public async Task<TResult> TryLockAsync<TResult>(TryLockOptions options, Func<object[], Task<TResult>> func, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            if (!await _db.LockTakeAsync(key, value, options.Timeout).ConfigureAwait(false))
            {
                if (options.OnFailed != null)
                {
                    await Task.Run(options.OnFailed).ConfigureAwait(false);
                }
                return default!;
            }

            try
            {
                return await func(args).ConfigureAwait(false);
            }
            finally
            {
                await LockReleaseAsync(key, value).ConfigureAwait(false);
            }
        }
        #endregion
    }
}
