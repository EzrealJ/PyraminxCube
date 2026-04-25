using PyraminxCube.Rbac.Core.Models;
using PyraminxCube.Rbac.Core.Models.Dto;

namespace PyraminxCube.Rbac.Core.Services
{
    /// <summary>
    /// 用户服务接口
    /// 提供用户管理相关的业务功能（U-001 ~ U-008）
    /// </summary>
    public interface IUserService
    {
        #region U-001 用户列表

        /// <summary>
        /// 分页查询用户列表
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="queryParams">查询参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>分页用户列表</returns>
        Task<PagedResult<UserListItem>> GetPagedListAsync(
            long tenantId,
            UserQueryParams queryParams,
            CancellationToken cancellationToken = default);

        #endregion

        #region U-002 新增用户

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="request">创建用户请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>新创建的用户ID</returns>
        Task<long> CreateAsync(
            long tenantId,
            CreateUserRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region U-003 编辑用户

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="request">更新用户请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task UpdateAsync(
            long tenantId,
            UpdateUserRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region U-004 删除用户

        /// <summary>
        /// 删除用户（软删除）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task DeleteAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default);

        #endregion

        #region U-005 启用/禁用用户

        /// <summary>
        /// 启用用户
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task EnableAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 禁用用户
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task DisableAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 切换用户状态
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>切换后的状态：true-启用，false-禁用</returns>
        Task<bool> ToggleStatusAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default);

        #endregion

        #region U-006 重置密码

        /// <summary>
        /// 重置用户密码
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="request">重置密码请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task ResetPasswordAsync(
            long tenantId,
            ResetPasswordRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region U-007 分配角色

        /// <summary>
        /// 为用户分配角色（完全替换原有角色）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="request">分配角色请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AssignRolesAsync(
            long tenantId,
            AssignRolesRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取用户已分配的角色ID列表
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>角色ID列表</returns>
        Task<List<long>> GetUserRoleIdsAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default);

        #endregion

        #region U-008 查看用户权限

        /// <summary>
        /// 获取用户的有效权限（合并所有角色后的权限）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>用户权限信息</returns>
        Task<UserPermissions> GetUserPermissionsAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取用户的功能权限树（用于前端菜单渲染）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>功能权限树</returns>
        Task<List<FeaturePermissionInfo>> GetUserFeatureTreeAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default);

        #endregion
    }
}
