namespace PyraminxCube.Rbac.Core.Models.Dto
{
    #region D-001 维度列表

    /// <summary>
    /// 数据维度查询参数
    /// </summary>
    public class DataDimensionQueryParams
    {
        /// <summary>
        /// 维度编码（模糊匹配）
        /// </summary>
        public string? DimensionCode { get; set; }

        /// <summary>
        /// 维度名称（模糊匹配）
        /// </summary>
        public string? DimensionName { get; set; }

        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页条数
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 数据维度列表项
    /// </summary>
    public class DataDimensionListItem
    {
        /// <summary>
        /// 维度ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 维度编码
        /// </summary>
        public string DimensionCode { get; set; } = string.Empty;

        /// <summary>
        /// 维度名称
        /// </summary>
        public string DimensionName { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 维度值数量
        /// </summary>
        public int ScopeCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedTime { get; set; }
    }

    #endregion

    #region D-002 & D-003 新增/编辑维度

    /// <summary>
    /// 创建数据维度请求
    /// </summary>
    public class CreateDataDimensionRequest
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
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 更新数据维度请求
    /// </summary>
    public class UpdateDataDimensionRequest
    {
        /// <summary>
        /// 维度ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 维度名称
        /// </summary>
        public string DimensionName { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }
    }

    #endregion

    #region D-005 维度值列表

    /// <summary>
    /// 数据范围值查询参数
    /// </summary>
    public class DataScopeQueryParams
    {
        /// <summary>
        /// 维度ID
        /// </summary>
        public long DimensionId { get; set; }

        /// <summary>
        /// 范围编码（模糊匹配）
        /// </summary>
        public string? ScopeCode { get; set; }

        /// <summary>
        /// 范围名称（模糊匹配）
        /// </summary>
        public string? ScopeName { get; set; }

        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页条数
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 数据范围值列表项
    /// </summary>
    public class DataScopeListItem
    {
        /// <summary>
        /// 范围ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 维度ID
        /// </summary>
        public long DimensionId { get; set; }

        /// <summary>
        /// 范围编码
        /// </summary>
        public string ScopeCode { get; set; } = string.Empty;

        /// <summary>
        /// 范围名称
        /// </summary>
        public string ScopeName { get; set; } = string.Empty;

        /// <summary>
        /// 父级ID
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 父级名称
        /// </summary>
        public string? ParentName { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedTime { get; set; }
    }

    #endregion

    #region D-006 & D-007 新增/编辑维度值

    /// <summary>
    /// 创建数据范围值请求
    /// </summary>
    public class CreateDataScopeRequest
    {
        /// <summary>
        /// 维度ID
        /// </summary>
        public long DimensionId { get; set; }

        /// <summary>
        /// 范围编码（同一维度内唯一）
        /// </summary>
        public string ScopeCode { get; set; } = string.Empty;

        /// <summary>
        /// 范围名称
        /// </summary>
        public string ScopeName { get; set; } = string.Empty;

        /// <summary>
        /// 父级ID（支持层级结构）
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// 更新数据范围值请求
    /// </summary>
    public class UpdateDataScopeRequest
    {
        /// <summary>
        /// 范围ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 范围名称
        /// </summary>
        public string ScopeName { get; set; } = string.Empty;

        /// <summary>
        /// 父级ID（支持层级结构）
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
    }

    #endregion

    #region D-009 维度值树管理

    /// <summary>
    /// 数据范围值树节点
    /// </summary>
    public class DataScopeTreeNode
    {
        /// <summary>
        /// 范围ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 维度ID
        /// </summary>
        public long DimensionId { get; set; }

        /// <summary>
        /// 范围编码
        /// </summary>
        public string ScopeCode { get; set; } = string.Empty;

        /// <summary>
        /// 范围名称
        /// </summary>
        public string ScopeName { get; set; } = string.Empty;

        /// <summary>
        /// 父级ID
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 子节点列表
        /// </summary>
        public List<DataScopeTreeNode> Children { get; set; } = [];
    }

    /// <summary>
    /// 移动数据范围值请求
    /// </summary>
    public class MoveDataScopeRequest
    {
        /// <summary>
        /// 要移动的范围ID
        /// </summary>
        public long ScopeId { get; set; }

        /// <summary>
        /// 新的父级ID（null表示移动到顶级）
        /// </summary>
        public long? NewParentId { get; set; }

        /// <summary>
        /// 新的排序号
        /// </summary>
        public int NewSortOrder { get; set; }
    }

    #endregion

    #region D-010 维度映射配置

    /// <summary>
    /// 数据维度映射查询参数
    /// </summary>
    public class DataDimensionMappingQueryParams
    {
        /// <summary>
        /// 维度ID
        /// </summary>
        public long DimensionId { get; set; }

        /// <summary>
        /// 表名（模糊匹配）
        /// </summary>
        public string? TableName { get; set; }
    }

    /// <summary>
    /// 数据维度映射列表项
    /// </summary>
    public class DataDimensionMappingListItem
    {
        /// <summary>
        /// 映射ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 维度ID
        /// </summary>
        public long DimensionId { get; set; }

        /// <summary>
        /// 维度名称
        /// </summary>
        public string DimensionName { get; set; } = string.Empty;

        /// <summary>
        /// 业务表名
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// 关联字段名
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedTime { get; set; }
    }

    /// <summary>
    /// 创建数据维度映射请求
    /// </summary>
    public class CreateDataDimensionMappingRequest
    {
        /// <summary>
        /// 维度ID
        /// </summary>
        public long DimensionId { get; set; }

        /// <summary>
        /// 业务表名
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// 关联字段名
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// 更新数据维度映射请求
    /// </summary>
    public class UpdateDataDimensionMappingRequest
    {
        /// <summary>
        /// 映射ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 业务表名
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// 关联字段名
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
    }

    #endregion

    #region 其他辅助类

    /// <summary>
    /// 维度使用情况检查结果
    /// </summary>
    public class DimensionUsageCheckResult
    {
        /// <summary>
        /// 是否正在使用
        /// </summary>
        public bool IsInUse { get; set; }

        /// <summary>
        /// 使用该维度的角色数量
        /// </summary>
        public int RoleCount { get; set; }

        /// <summary>
        /// 关联的维度值数量
        /// </summary>
        public int ScopeCount { get; set; }

        /// <summary>
        /// 警告消息
        /// </summary>
        public string? WarningMessage { get; set; }
    }

    /// <summary>
    /// 数据范围值使用情况检查结果
    /// </summary>
    public class DataScopeUsageCheckResult
    {
        /// <summary>
        /// 是否正在使用
        /// </summary>
        public bool IsInUse { get; set; }

        /// <summary>
        /// 使用该范围的角色数量
        /// </summary>
        public int RoleCount { get; set; }

        /// <summary>
        /// 子节点数量
        /// </summary>
        public int ChildCount { get; set; }

        /// <summary>
        /// 警告消息
        /// </summary>
        public string? WarningMessage { get; set; }
    }

    #endregion
}
