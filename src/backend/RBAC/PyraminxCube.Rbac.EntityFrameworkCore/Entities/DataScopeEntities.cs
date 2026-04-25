using PyraminxCube.Rbac.Core.Models;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Entities
{
    /// <summary>
    /// 数据权限维度表（全局或租户级）
    /// </summary>
    public class RbacDataDimension : RbacGlobalEntity
    {
        /// <summary>
        /// 维度编码（唯一）
        /// </summary>
        public string DimensionCode { get; set; } = string.Empty;

        /// <summary>
        /// 维度名称
        /// </summary>
        public string DimensionName { get; set; } = string.Empty;

        /// <summary>
        /// 维度描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 维度值来源表名（用于下拉选择时的数据来源）
        /// </summary>
        public string? SourceTable { get; set; }

        /// <summary>
        /// 维度值的ID字段名
        /// </summary>
        public string? SourceIdField { get; set; }

        /// <summary>
        /// 维度值的名称字段名
        /// </summary>
        public string? SourceNameField { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 角色数据权限
        /// </summary>
        public ICollection<RbacRoleDataScope> RoleDataScopes { get; set; } = [];

        /// <summary>
        /// 角色数据权限标记
        /// </summary>
        public ICollection<RbacRoleDataScopeFlag> RoleDataScopeFlags { get; set; } = [];

        /// <summary>
        /// 维度映射配置
        /// </summary>
        public ICollection<RbacDataDimensionMapping> DimensionMappings { get; set; } = [];
    }

    /// <summary>
    /// 数据权限范围值表（租户级）
    /// </summary>
    public class RbacDataScope : RbacEntity
    {
        /// <summary>
        /// 维度编码
        /// </summary>
        public string DimensionCode { get; set; } = string.Empty;

        /// <summary>
        /// 范围值ID（如部门ID、区域ID）
        /// </summary>
        public long ScopeId { get; set; }

        /// <summary>
        /// 范围值名称（冗余存储，便于显示）
        /// </summary>
        public string ScopeName { get; set; } = string.Empty;

        /// <summary>
        /// 父级范围值ID（用于层级结构）
        /// </summary>
        public long? ParentScopeId { get; set; }

        /// <summary>
        /// 层级路径（如：1/2/3/，便于查询子级）
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 角色数据权限关联表（具体值）
    /// </summary>
    public class RbacRoleDataScope : RbacEntity
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 数据维度ID
        /// </summary>
        public long DataDimensionId { get; set; }

        /// <summary>
        /// 具体的范围值ID
        /// </summary>
        public long ScopeId { get; set; }

        /// <summary>
        /// 关联角色
        /// </summary>
        public RbacRole? Role { get; set; }

        /// <summary>
        /// 关联数据维度
        /// </summary>
        public RbacDataDimension? DataDimension { get; set; }
    }

    /// <summary>
    /// 角色数据权限标记表（特殊标记：ALL/SELF）
    /// </summary>
    public class RbacRoleDataScopeFlag : RbacEntity
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 数据维度ID
        /// </summary>
        public long DataDimensionId { get; set; }

        /// <summary>
        /// 特殊标记：ALL-全部，SELF-仅自己
        /// </summary>
        public DataScopeFlag Flag { get; set; }

        /// <summary>
        /// 关联角色
        /// </summary>
        public RbacRole? Role { get; set; }

        /// <summary>
        /// 关联数据维度
        /// </summary>
        public RbacDataDimension? DataDimension { get; set; }
    }

    /// <summary>
    /// 数据维度与实体映射配置表（全局）
    /// </summary>
    public class RbacDataDimensionMapping : RbacGlobalEntity
    {
        /// <summary>
        /// 数据维度ID
        /// </summary>
        public long DataDimensionId { get; set; }

        /// <summary>
        /// 实体类型全名（如：MyApp.Entities.Order）
        /// </summary>
        public string EntityTypeName { get; set; } = string.Empty;

        /// <summary>
        /// 实体属性名（如：DepartmentId）
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// 关联数据维度
        /// </summary>
        public RbacDataDimension? DataDimension { get; set; }
    }
}
