using PyraminxCube.Rbac.Core.Models;

namespace PyraminxCube.Rbac.Core.Abstractions
{
    /// <summary>
    /// 权限缓存接口
    /// </summary>
    public interface IPermissionCache
    {
        /// <summary>
        /// 获取用户权限缓存
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>用户权限信息，如果缓存不存在则返回null</returns>
        Task<UserPermissions?> GetAsync(long userId, long tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 设置用户权限缓存
        /// </summary>
        /// <param name="permissions">用户权限信息</param>
        /// <param name="expiration">过期时间</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task SetAsync(UserPermissions permissions, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 移除用户权限缓存
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task RemoveAsync(long userId, long tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 移除指定租户的所有用户权限缓存
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task RemoveByTenantAsync(long tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 移除所有权限缓存
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        Task RemoveAllAsync(CancellationToken cancellationToken = default);
    }
}
