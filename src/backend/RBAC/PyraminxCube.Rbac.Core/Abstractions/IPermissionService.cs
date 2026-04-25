using PyraminxCube.Rbac.Core.Models;

namespace PyraminxCube.Rbac.Core.Abstractions
{
    /// <summary>
    /// 权限服务接口
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// 获取用户的所有权限（合并所有角色后的有效权限）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>用户权限信息</returns>
        Task<UserPermissions> GetUserPermissionsAsync(long userId, long tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查用户是否有指定的API权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="apiCode">API权限编码</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否有权限</returns>
        Task<bool> HasApiPermissionAsync(long userId, long tenantId, string apiCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查用户是否有指定的功能权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="featureCode">功能权限编码</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否有权限</returns>
        Task<bool> HasFeaturePermissionAsync(long userId, long tenantId, string featureCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据 API 路径和方法获取对应的权限编码
        /// </summary>
        /// <param name="endpoint">API路径</param>
        /// <param name="httpMethod">HTTP方法</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>API权限编码，如果未找到则返回null</returns>
        Task<string?> GetApiCodeByEndpointAsync(string endpoint, string httpMethod, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取用户的功能权限树（用于前端菜单渲染）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>功能权限树</returns>
        Task<List<FeaturePermissionInfo>> GetUserFeatureTreeAsync(long userId, long tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 刷新用户权限缓存
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task RefreshUserPermissionCacheAsync(long userId, long tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 刷新指定租户的所有用户权限缓存
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task RefreshTenantPermissionCacheAsync(long tenantId, CancellationToken cancellationToken = default);
    }
}
