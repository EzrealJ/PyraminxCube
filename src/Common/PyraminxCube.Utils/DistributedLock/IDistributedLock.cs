using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyraminxCube.Utils.DistributedLock
{
    public interface ISyncDistributedLock
    {
        void Lock(LockOptions options, Action action);
        void Lock(LockOptions options, Action<object[]> action, params object[] args);
        TResult Lock<TResult>(LockOptions options, Func<object[], TResult> func, params object[] args);

        void TryLock(TryLockOptions options, Action action);
        void TryLock(TryLockOptions options, Action<object[]> action, params object[] args);
        TResult TryLock<TResult>(TryLockOptions options, Func<object[], TResult> func, params object[] args);
    }

    public interface IAsyncDistributedLock
    {
        Task LockAsync(LockOptions options, Func<Task> func);
        Task LockAsync(LockOptions options, Func<object[], Task> func, params object[] args);
        Task<TResult> LockAsync<TResult>(LockOptions options, Func<object[], Task<TResult>> func, params object[] args);
        Task TryLockAsync(TryLockOptions options, Func<Task> func);
        Task TryLockAsync(TryLockOptions options, Func<object[], Task> func, params object[] args);
        Task<TResult> TryLockAsync<TResult>(TryLockOptions options, Func<object[], Task<TResult>> func, params object[] args);
    }
}
