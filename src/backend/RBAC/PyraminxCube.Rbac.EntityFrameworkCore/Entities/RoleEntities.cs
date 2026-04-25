namespace PyraminxCube.Rbac.EntityFrameworkCore.Entities
{
    /// <summary>
    /// 角色表
    /// </summary>
    public class RbacRole : RbacEntity
    {
        /// <summary>
        /// 角色编码（唯一）
        /// </summary>
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// 角色描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 用户角色关联
        /// </summary>
        public ICollection<RbacUserRole> UserRoles { get; set; } = [];

        /// <summary>
        /// 角色API权限关联
        /// </summary>
        public ICollection<RbacRoleApiPermission> RoleApiPermissions { get; set; } = [];

        /// <summary>
        /// 角色功能权限关联
        /// </summary>
        public ICollection<RbacRoleFeaturePermission> RoleFeaturePermissions { get; set; } = [];

        /// <summary>
        /// 角色数据权限关联
        /// </summary>
        public ICollection<RbacRoleDataScope> RoleDataScopes { get; set; } = [];
    }

    /// <summary>
    /// 用户角色关联表
    /// </summary>
    public class RbacUserRole : RbacEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 关联用户
        /// </summary>
        public RbacUser? User { get; set; }

        /// <summary>
        /// 关联角色
        /// </summary>
        public RbacRole? Role { get; set; }
    }
}
