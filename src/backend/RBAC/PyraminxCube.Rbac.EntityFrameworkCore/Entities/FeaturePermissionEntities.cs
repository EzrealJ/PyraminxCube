using PyraminxCube.Rbac.Core.Models;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Entities
{
    /// <summary>
    /// 功能权限表（全局）
    /// </summary>
    public class RbacFeaturePermission : RbacGlobalEntity
    {
        /// <summary>
        /// 功能编码（唯一）
        /// </summary>
        public string FeatureCode { get; set; } = string.Empty;

        /// <summary>
        /// 功能名称
        /// </summary>
        public string FeatureName { get; set; } = string.Empty;

        /// <summary>
        /// 功能类型：1-模块，2-页面，3-按钮
        /// </summary>
        public FeatureType FeatureType { get; set; }

        /// <summary>
        /// 父级功能编码（null表示顶级）
        /// </summary>
        public string? ParentCode { get; set; }

        /// <summary>
        /// 前端路由路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 功能描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 角色功能权限关联
        /// </summary>
        public ICollection<RbacRoleFeaturePermission> RoleFeaturePermissions { get; set; } = [];

        /// <summary>
        /// 功能API映射
        /// </summary>
        public ICollection<RbacFeatureApiMapping> FeatureApiMappings { get; set; } = [];
    }

    /// <summary>
    /// 角色功能权限关联表
    /// </summary>
    public class RbacRoleFeaturePermission : RbacEntity
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 功能权限ID
        /// </summary>
        public long FeaturePermissionId { get; set; }

        /// <summary>
        /// 关联角色
        /// </summary>
        public RbacRole? Role { get; set; }

        /// <summary>
        /// 关联功能权限
        /// </summary>
        public RbacFeaturePermission? FeaturePermission { get; set; }
    }

    /// <summary>
    /// 功能与API的映射表（全局）
    /// </summary>
    public class RbacFeatureApiMapping : RbacGlobalEntity
    {
        /// <summary>
        /// 功能权限ID
        /// </summary>
        public long FeaturePermissionId { get; set; }

        /// <summary>
        /// API权限ID
        /// </summary>
        public long ApiPermissionId { get; set; }

        /// <summary>
        /// 关联功能权限
        /// </summary>
        public RbacFeaturePermission? FeaturePermission { get; set; }

        /// <summary>
        /// 关联API权限
        /// </summary>
        public RbacApiPermission? ApiPermission { get; set; }
    }
}
