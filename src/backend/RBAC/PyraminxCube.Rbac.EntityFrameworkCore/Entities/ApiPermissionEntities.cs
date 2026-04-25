namespace PyraminxCube.Rbac.EntityFrameworkCore.Entities
{
    /// <summary>
    /// API分组表（全局）
    /// </summary>
    public class RbacApiGroup : RbacGlobalEntity
    {
        /// <summary>
        /// 分组编码
        /// </summary>
        public string GroupCode { get; set; } = string.Empty;

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// 分组描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// API分组映射
        /// </summary>
        public ICollection<RbacApiGroupMapping> ApiGroupMappings { get; set; } = [];
    }

    /// <summary>
    /// API权限表（全局）
    /// </summary>
    public class RbacApiPermission : RbacGlobalEntity
    {
        /// <summary>
        /// API编码（唯一）
        /// </summary>
        public string ApiCode { get; set; } = string.Empty;

        /// <summary>
        /// API名称
        /// </summary>
        public string ApiName { get; set; } = string.Empty;

        /// <summary>
        /// 接口路径
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// HTTP方法：GET/POST/PUT/DELETE
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// API描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否需要登录
        /// </summary>
        public bool RequireAuth { get; set; } = true;

        /// <summary>
        /// 是否需要授权（false表示仅需登录即可访问）
        /// </summary>
        public bool RequirePermission { get; set; } = true;

        /// <summary>
        /// API分组映射
        /// </summary>
        public ICollection<RbacApiGroupMapping> ApiGroupMappings { get; set; } = [];

        /// <summary>
        /// 角色API权限关联
        /// </summary>
        public ICollection<RbacRoleApiPermission> RoleApiPermissions { get; set; } = [];

        /// <summary>
        /// 功能API映射
        /// </summary>
        public ICollection<RbacFeatureApiMapping> FeatureApiMappings { get; set; } = [];
    }

    /// <summary>
    /// API与分组的映射表（全局）
    /// </summary>
    public class RbacApiGroupMapping : RbacGlobalEntity
    {
        /// <summary>
        /// API分组ID
        /// </summary>
        public long ApiGroupId { get; set; }

        /// <summary>
        /// API权限ID
        /// </summary>
        public long ApiPermissionId { get; set; }

        /// <summary>
        /// 关联API分组
        /// </summary>
        public RbacApiGroup? ApiGroup { get; set; }

        /// <summary>
        /// 关联API权限
        /// </summary>
        public RbacApiPermission? ApiPermission { get; set; }
    }

    /// <summary>
    /// 角色API权限关联表
    /// </summary>
    public class RbacRoleApiPermission : RbacEntity
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// API权限ID
        /// </summary>
        public long ApiPermissionId { get; set; }

        /// <summary>
        /// 关联角色
        /// </summary>
        public RbacRole? Role { get; set; }

        /// <summary>
        /// 关联API权限
        /// </summary>
        public RbacApiPermission? ApiPermission { get; set; }
    }
}
