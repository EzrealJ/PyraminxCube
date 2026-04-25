namespace PyraminxCube.Utils.DistributedLock
{
    /// <summary>
    /// 分布式锁接口
    /// </summary>
    /// <remarks>
    /// 使用状态对象模式传递参数，调用方可使用匿名类型或元组：
    /// <code>
    /// // 使用匿名类型
    /// lock.Lock("token", timeout, state => { 
    ///     Console.WriteLine(state.Name); 
    /// }, new { Name = "test", Count = 1 });
    /// 
    /// // 使用元组
    /// lock.Lock("token", timeout, state => { 
    ///     Console.WriteLine(state.name); 
    /// }, (name: "test", count: 1));
    /// </code>
    /// </remarks>
    public interface IDistributedLock
    {
        #region Lock Sync

        /// <summary>
        /// 加锁执行（阻塞等待直到获取锁）
        /// </summary>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="action">锁成功回调</param>
        void Lock(string token, TimeSpan timeout, Action action);

        /// <summary>
        /// 加锁执行（阻塞等待直到获取锁）
        /// </summary>
        /// <typeparam name="TState">状态对象类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="state">状态对象（可使用匿名类型或元组）</param>
        void Lock<TState>(string token, TimeSpan timeout, Action<TState> action, TState state);

        /// <summary>
        /// 加锁执行（阻塞等待直到获取锁）
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        /// <returns>回调方法的返回值</returns>
        TResult Lock<TResult>(string token, TimeSpan timeout, Func<TResult> func);

        /// <summary>
        /// 加锁执行（阻塞等待直到获取锁）
        /// </summary>
        /// <typeparam name="TState">状态对象类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="state">状态对象（可使用匿名类型或元组）</param>
        /// <returns>回调方法的返回值</returns>
        TResult Lock<TState, TResult>(string token, TimeSpan timeout, Func<TState, TResult> func, TState state);

        #endregion

        #region TryLock Sync

        /// <summary>
        /// 尝试锁定（如果获取锁失败则调用失败回调）
        /// </summary>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="actionOnFailed">获取锁失败时的回调方法</param>
        void TryLock(string token, TimeSpan timeout, Action action, Action? actionOnFailed = null);

        /// <summary>
        /// 尝试锁定（如果获取锁失败则调用失败回调）
        /// </summary>
        /// <typeparam name="TState">状态对象类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="action">锁成功回调方法</param>
        /// <param name="state">状态对象（可使用匿名类型或元组）</param>
        /// <param name="actionOnFailed">获取锁失败时的回调方法</param>
        void TryLock<TState>(string token, TimeSpan timeout, Action<TState> action, TState state, Action? actionOnFailed = null);

        /// <summary>
        /// 尝试锁定（如果获取锁失败则调用失败回调）
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="actionOnFailed">获取锁失败时的回调方法</param>
        /// <returns>回调方法的返回值，如果获取锁失败则返回default</returns>
        TResult TryLock<TResult>(string token, TimeSpan timeout, Func<TResult> func, Action? actionOnFailed = null);

        /// <summary>
        /// 尝试锁定（如果获取锁失败则调用失败回调）
        /// </summary>
        /// <typeparam name="TState">状态对象类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="state">状态对象（可使用匿名类型或元组）</param>
        /// <param name="actionOnFailed">获取锁失败时的回调方法</param>
        /// <returns>回调方法的返回值，如果获取锁失败则返回default</returns>
        TResult TryLock<TState, TResult>(string token, TimeSpan timeout, Func<TState, TResult> func, TState state, Action? actionOnFailed = null);

        #endregion

        #region Lock Async

        /// <summary>
        /// 异步加锁执行（阻塞等待直到获取锁）
        /// </summary>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        Task LockAsync(string token, TimeSpan timeout, Func<Task> func);

        /// <summary>
        /// 异步加锁执行（阻塞等待直到获取锁）
        /// </summary>
        /// <typeparam name="TState">状态对象类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="state">状态对象（可使用匿名类型或元组）</param>
        Task LockAsync<TState>(string token, TimeSpan timeout, Func<TState, Task> func, TState state);

        /// <summary>
        /// 异步加锁执行（阻塞等待直到获取锁）
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        /// <returns>回调方法的返回值</returns>
        Task<TResult> LockAsync<TResult>(string token, TimeSpan timeout, Func<Task<TResult>> func);

        /// <summary>
        /// 异步加锁执行（阻塞等待直到获取锁）
        /// </summary>
        /// <typeparam name="TState">状态对象类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="state">状态对象（可使用匿名类型或元组）</param>
        /// <returns>回调方法的返回值</returns>
        Task<TResult> LockAsync<TState, TResult>(string token, TimeSpan timeout, Func<TState, Task<TResult>> func, TState state);

        #endregion

        #region TryLock Async

        /// <summary>
        /// 异步尝试锁定（如果获取锁失败则调用失败回调）
        /// </summary>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="funcOnFailed">获取锁失败时的回调方法</param>
        Task TryLockAsync(string token, TimeSpan timeout, Func<Task> func, Func<Task>? funcOnFailed = null);

        /// <summary>
        /// 异步尝试锁定（如果获取锁失败则调用失败回调）
        /// </summary>
        /// <typeparam name="TState">状态对象类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="state">状态对象（可使用匿名类型或元组）</param>
        /// <param name="funcOnFailed">获取锁失败时的回调方法</param>
        Task TryLockAsync<TState>(string token, TimeSpan timeout, Func<TState, Task> func, TState state, Func<Task>? funcOnFailed = null);

        /// <summary>
        /// 异步尝试锁定（如果获取锁失败则调用失败回调）
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="funcOnFailed">获取锁失败时的回调方法</param>
        /// <returns>回调方法的返回值，如果获取锁失败则返回default</returns>
        Task<TResult> TryLockAsync<TResult>(string token, TimeSpan timeout, Func<Task<TResult>> func, Func<Task>? funcOnFailed = null);

        /// <summary>
        /// 异步尝试锁定（如果获取锁失败则调用失败回调）
        /// </summary>
        /// <typeparam name="TState">状态对象类型</typeparam>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="token">锁令牌，必须确保唯一性</param>
        /// <param name="timeout">锁过期时间</param>
        /// <param name="func">锁成功回调方法</param>
        /// <param name="state">状态对象（可使用匿名类型或元组）</param>
        /// <param name="funcOnFailed">获取锁失败时的回调方法</param>
        /// <returns>回调方法的返回值，如果获取锁失败则返回default</returns>
        Task<TResult> TryLockAsync<TState, TResult>(string token, TimeSpan timeout, Func<TState, Task<TResult>> func, TState state, Func<Task>? funcOnFailed = null);

        #endregion
    }
}
