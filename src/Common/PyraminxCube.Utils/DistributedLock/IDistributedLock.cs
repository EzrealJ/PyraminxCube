using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Utils.DistributedLock
{
    public interface IDistributedLock
    {
        #region LockSync
        /// <summary>
        /// 加锁执行<paramref name="action"/>
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="action">锁成功回调</param>
        void Lock(string token, TimeSpan timeout, Action action);
        /// <summary>
        /// 加锁执行<paramref name="action"/>
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        void Lock<TArg1>(string token, TimeSpan timeout, Action<TArg1> action, TArg1 args1);
        /// <summary>
        /// 加锁执行<paramref name="action"/>
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        void Lock<TArg1, TArg2>(string token, TimeSpan timeout, Action<TArg1, TArg2> action, TArg1 args1, TArg2 args2);
        /// <summary>
        /// 加锁执行<paramref name="action"/>
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        void Lock<TArg1, TArg2, TArg3>(string token, TimeSpan timeout, Action<TArg1, TArg2, TArg3> action, TArg1 args1, TArg2 args2, TArg3 args3);
        /// <summary>
        /// 加锁执行<paramref name="action"/>
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        void Lock<TArg1, TArg2, TArg3, TArg4>(string token, TimeSpan timeout, Action<TArg1, TArg2, TArg3, TArg4> action, TArg1 args1, TArg2 args2, TArg3 args3, TArg4 args4);
        /// <summary>
        /// 加锁执行<paramref name="func"/>
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        TResult Lock<TResult>(string token, TimeSpan timeout, Func<TResult> func);
        /// <summary>
        /// 加锁执行<paramref name="func"/>
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        TResult Lock<TArg1, TResult>(string token, TimeSpan timeout, Func<TArg1, TResult> func, TArg1 args1);
        /// <summary>
        /// 加锁执行<paramref name="func"/>
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        TResult Lock<TArg1, TArg2, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, TResult> func, TArg1 args1, TArg2 args2);
        /// <summary>
        /// 加锁执行<paramref name="func"/>
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        TResult Lock<TArg1, TArg2, TArg3, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, TResult> func, TArg1 args1, TArg2 args2, TArg3 args3);
        /// <summary>
        /// 加锁执行<paramref name="func"/>
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        TResult Lock<TArg1, TArg2, TArg3, TArg4, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, TArg4, TResult> func, TArg1 args1, TArg2 args2, TArg3 args3, TArg4 args4);
        #endregion

        #region TryLockSync
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        void TryLock(string token, TimeSpan timeout, Action action, Action? actionOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        void TryLock<TArg1>(string token, TimeSpan timeout, Action<TArg1> action, TArg1 args1, Action? actionOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        void TryLock<TArg1, TArg2>(string token, TimeSpan timeout, Action<TArg1, TArg2> action, TArg1 args1, TArg2 args2, Action? actionOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        void TryLock<TArg1, TArg2, TArg3>(string token, TimeSpan timeout, Action<TArg1, TArg2, TArg3> action, TArg1 args1, TArg2 args2, TArg3 args3, Action? actionOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        void TryLock<TArg1, TArg2, TArg3, TArg4>(string token, TimeSpan timeout, Action<TArg1, TArg2, TArg3, TArg4> action, TArg1 args1, TArg2 args2, TArg3 args3, TArg4 args4, Action? actionOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        TResult TryLock<TResult>(string token, TimeSpan timeout, Func<TResult> func, Action? actionOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        TResult TryLock<TArg1, TResult>(string token, TimeSpan timeout, Func<TArg1, TResult> func, TArg1 args1, Action? actionOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        TResult TryLock<TArg1, TArg2, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, TResult> func, TArg1 args1, TArg2 args2, Action? actionOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        TResult TryLock<TArg1, TArg2, TArg3, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, TResult> func, TArg1 args1, TArg2 args2, TArg3 args3, Action? actionOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        /// <param name="actionOnFailed">错误回调方法</param>
        TResult TryLock<TArg1, TArg2, TArg3, TArg4, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, TArg4, TResult> func, TArg1 args1, TArg2 args2, TArg3 args3, TArg4 args4, Action? actionOnFailed = null);
        #endregion

        #region LockAsync
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        Task LockAsync(string token, TimeSpan timeout, Func<Task> func);
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        Task LockAsync<TArg1>(string token, TimeSpan timeout, Func<TArg1, Task> func, TArg1 args1);
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        Task LockAsync<TArg1, TArg2>(string token, TimeSpan timeout, Func<TArg1, TArg2, Task> func, TArg1 args1, TArg2 args2);
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        Task LockAsync<TArg1, TArg2, TArg3>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, Task> func, TArg1 args1, TArg2 args2, TArg3 args3);
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        Task LockAsync<TArg1, TArg2, TArg3, TArg4>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, TArg4, Task> func, TArg1 args1, TArg2 args2, TArg3 args3, TArg4 args4);
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        Task<TResult> LockAsync<TResult>(string token, TimeSpan timeout, Func<Task<TResult>> func);
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        Task<TResult> LockAsync<TArg1, TResult>(string token, TimeSpan timeout, Func<TArg1, Task<TResult>> func, TArg1 args1);
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        Task<TResult> LockAsync<TArg1, TArg2, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, Task<TResult>> func, TArg1 args1, TArg2 args2);
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        Task<TResult> LockAsync<TArg1, TArg2, TArg3, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, Task<TResult>> func, TArg1 args1, TArg2 args2, TArg3 args3);
        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        Task<TResult> LockAsync<TArg1, TArg2, TArg3, TArg4, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, TArg4, Task<TResult>> func, TArg1 args1, TArg2 args2, TArg3 args3, TArg4 args4);
        #endregion

        #region TryLockSync
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        Task TryLockAsync(string token, TimeSpan timeout, Func<Task> func, Func<Task>? funcOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        Task TryLockAsync<TArg1>(string token, TimeSpan timeout, Func<TArg1, Task> func, TArg1 args1, Func<Task>? funcOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        Task TryLockAsync<TArg1, TArg2>(string token, TimeSpan timeout, Func<TArg1, TArg2, Task> func, TArg1 args1, TArg2 args2, Func<Task>? funcOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        Task TryLockAsync<TArg1, TArg2, TArg3>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, Task> func, TArg1 args1, TArg2 args2, TArg3 args3, Func<Task>? funcOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        Task TryLockAsync<TArg1, TArg2, TArg3, TArg4>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, TArg4, Task> func, TArg1 args1, TArg2 args2, TArg3 args3, TArg4 args4, Func<Task>? funcOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        Task<TResult> TryLockAsync<TResult>(string token, TimeSpan timeout, Func<Task<TResult>> func, Func<Task>? funcOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        Task<TResult> TryLockAsync<TArg1, TResult>(string token, TimeSpan timeout, Func<TArg1, Task<TResult>> func, TArg1 args1, Func<Task>? funcOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        Task<TResult> TryLockAsync<TArg1, TArg2, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, Task<TResult>> func, TArg1 args1, TArg2 args2, Func<Task>? funcOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        Task<TResult> TryLockAsync<TArg1, TArg2, TArg3, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, Task<TResult>> func, TArg1 args1, TArg2 args2, TArg3 args3, Func<Task>? funcOnFailed = null);
        /// <summary>
        /// 尝试锁定(如果已经在执行，则调用错误处理)
        /// </summary>
        /// <param name="token">锁令牌</param>
        /// <param name="timeout">锁过期时长</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="args1">参数1</param>
        /// <param name="args2">参数2</param>
        /// <param name="args3">参数3</param>
        /// <param name="args4">参数4</param>
        /// <param name="funcOnFailed">错误回调方法</param>
        Task<TResult> TryLockAsync<TArg1, TArg2, TArg3, TArg4, TResult>(string token, TimeSpan timeout, Func<TArg1, TArg2, TArg3, TArg4, Task<TResult>> func, TArg1 args1, TArg2 args2, TArg3 args3, TArg4 args4, Func<Task>? funcOnFailed = null);
        #endregion

    }
}
