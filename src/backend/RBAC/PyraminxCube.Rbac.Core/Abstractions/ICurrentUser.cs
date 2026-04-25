namespace PyraminxCube.Rbac.Core.Abstractions
{
    /// <summary>
    /// 当前用户信息接口
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        long? UserId { get; }

        /// <summary>
        /// 租户ID
        /// </summary>
        long? TenantId { get; }

        /// <summary>
        /// 用户名
        /// </summary>
        string? UserName { get; }

        /// <summary>
        /// 是否已认证
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// 是否为超级管理员（跳过所有权限校验）
        /// </summary>
        bool IsSuperAdmin { get; }

        /// <summary>
        /// 获取用户的角色ID列表
        /// </summary>
        IReadOnlyList<long> RoleIds { get; }
    }
}
