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
        private void LockRelease(string key, string value)
        {
            if (!_db.LockRelease(key, value))
            {
                _logger.LogError("value:{Value}, LockRelease is error.", value);
            }
        }

        #region Lock Sync


        public void Lock(LockOptions options, Action action)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            while (!_db.LockTake(key, value, options.Timeout))
            {
                Thread.Sleep(_millisecondsDelay);
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

        public void Lock(LockOptions options, Action<object[]> action, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            while (!_db.LockTake(key, value, options.Timeout))
            {
                Thread.Sleep(_millisecondsDelay);
            }
            try
            {
                action(args);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult Lock<TResult>(LockOptions options, Func<object[], TResult> func, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            while (!_db.LockTake(key, value, options.Timeout))
            {
                Thread.Sleep(_millisecondsDelay);
            }
            try
            {
                return func(args);
            }
            finally
            {
                LockRelease(key, value);
            }
        }
        #endregion


        #region TryLock Sync

        public void TryLock(TryLockOptions options, Action action)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            if (!_db.LockTake(key, value, options.Timeout))
            {
                options.OnFailed?.Invoke();
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

        public void TryLock(TryLockOptions options, Action<object[]> action, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(action);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            if (!_db.LockTake(key, value, options.Timeout))
            {
                options.OnFailed?.Invoke();
                return;
            }

            try
            {
                action(args);
            }
            finally
            {
                LockRelease(key, value);
            }
        }

        public TResult TryLock<TResult>(TryLockOptions options, Func<object[], TResult> func, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(func);

            string key = NormalizeKey(options.Token);
            string value = GetLockValue(options.Token);
            if (!_db.LockTake(key, value, options.Timeout))
            {
                options.OnFailed?.Invoke();
                return default!;
            }

            try
            {
                return func(args);
            }
            finally
            {
                LockRelease(key, value);
            }
        }
        #endregion
    }
}
