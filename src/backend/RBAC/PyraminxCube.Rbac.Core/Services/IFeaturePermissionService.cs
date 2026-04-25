using PyraminxCube.Rbac.Core.Models;

namespace PyraminxCube.Rbac.Core.Services
{
    /// <summary>
    /// 功能权限服务接口
    /// 提供功能按钮权限管理相关的业务功能（F-001 ~ F-007）
    /// </summary>
    public interface IFeaturePermissionService
    {
        #region F-001 功能权限树

        /// <summary>
        /// 获取所有功能权限树形列表（树形展示所有功能权限）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>功能权限树形列表</returns>
        Task<List<FeaturePermissionTreeNode>> GetTreeAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取功能权限树形列表（仅启用的）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>启用的功能权限树形列表</returns>
        Task<List<FeaturePermissionTreeNode>> GetEnabledTreeAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取功能权限简单列表（用于下拉选��，平铺结构）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>功能权限简单列表</returns>
        Task<List<FeaturePermissionSimpleInfo>> GetAllAsync(
            CancellationToken cancellationToken = default);

        #endregion

        #region F-002 新增功能节点

        /// <summary>
        /// 创建新功能节点（模块/页面/按钮）
        /// </summary>
        /// <param name="request">创建功能节点请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>新创建的功能节点ID</returns>
        Task<long> CreateAsync(
            CreateFeaturePermissionRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region F-003 编辑功能节点

        /// <summary>
        /// 更新功能节点信息
        /// </summary>
        /// <param name="request">更新功能节点请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task UpdateAsync(
            UpdateFeaturePermissionRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region F-004 ���除功能节点

        /// <summary>
        /// 删除功能节点（软删除）
        /// </summary>
        /// <param name="featureId">功能节点ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task DeleteAsync(
            long featureId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查功能节点是否可以被删除
        /// </summary>
        /// <param name="featureId">功能节点ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>检查结果：canDelete-是否可删除，childCount-子节点数</returns>
        Task<(bool CanDelete, int ChildCount)> CheckCanDeleteAsync(
            long featureId,
            CancellationToken cancellationToken = default);

        #endregion

        #region F-005 调整节点顺序

        /// <summary>
        /// 调整功能节点顺序（拖拽排序）
        /// </summary>
        /// <param name="request">调整顺序请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task UpdateSortOrderAsync(
            UpdateFeatureSortOrderRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量调整功能节点顺序
        /// </summary>
        /// <param name="requests">调整顺序请求列表</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task UpdateSortOrderBatchAsync(
            List<UpdateFeatureSortOrderRequest> requests,
            CancellationToken cancellationToken = default);

        #endregion

        #region F-006 关联API

        /// <summary>
        /// 为功能按钮关联对应的API（F-006）
        /// </summary>
        /// <param name="request">关联API请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AssignApiAsync(
            AssignApiToFeatureRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 移除功能按钮的API关联
        /// </summary>
        /// <param name="featureId">功能按钮ID</param>
        /// <param name="apiId">API ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task RemoveApiAsync(
            long featureId,
            long apiId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取功能按钮已关联的API列表
        /// </summary>
        /// <param name="featureId">功能按钮ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>已关联的API列表</returns>
        Task<List<ApiPermissionSimpleInfo>> GetAssignedApisAsync(
            long featureId,
            CancellationToken cancellationToken = default);

        #endregion

        #region F-007 批量关联API

        /// <summary>
        /// 批量设置按钮与API的关联（F-007）
        /// </summary>
        /// <param name="request">批量关联API请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AssignBatchApisAsync(
            BatchAssignApisToFeaturesRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 清除功能按钮的所有API关联
        /// </summary>
        /// <param name="featureId">功能按钮ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task ClearAllApisAsync(
            long featureId,
            CancellationToken cancellationToken = default);

        #endregion
    }

    #region 功能权限相关DTO

    /// <summary>
    /// 功能权限树节点
    /// </summary>
    public class FeaturePermissionTreeNode
    {
        /// <summary>
        /// 功能节点ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 功能编码
        /// </summary>
        public string FeatureCode { get; set; } = string.Empty;

        /// <summary>
        /// 功能名称
        /// </summary>
        public string FeatureName { get; set; } = string.Empty;

        /// <summary>
        /// 功能类型
        /// </summary>
        public FeatureType FeatureType { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        public long? ParentId { get; set; }

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
        /// 状态：0-禁用，1-启用
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// ���联的API编码列表（仅按钮类型有值）
        /// </summary>
        public List<string> ApiCodes { get; set; } = [];

        /// <summary>
        /// 子级功能列表
        /// </summary>
        public List<FeaturePermissionTreeNode> Children { get; set; } = [];
    }

    /// <summary>
    /// 功能权限简单信息（用于下拉选择）
    /// </summary>
    public class FeaturePermissionSimpleInfo
    {
        /// <summary>
        /// 功能节点ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 功能编码
        /// </summary>
        public string FeatureCode { get; set; } = string.Empty;

        /// <summary>
        /// 功能名称
        /// </summary>
        public string FeatureName { get; set; } = string.Empty;

        /// <summary>
        /// 功能类型
        /// </summary>
        public FeatureType FeatureType { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 层级深度（从1开始）
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 完整路径（用于展示，如：系统管理/用户管理/新增用户）
        /// </summary>
        public string FullPath { get; set; } = string.Empty;
    }

    /// <summary>
    /// 创建功能节点请求
    /// </summary>
    public class CreateFeaturePermissionRequest
    {
        /// <summary>
        /// 功能编码
        /// </summary>
        public string FeatureCode { get; set; } = string.Empty;

        /// <summary>
        /// 功能名称
        /// </summary>
        public string FeatureName { get; set; } = string.Empty;

        /// <summary>
        /// 功能类型
        /// </summary>
        public FeatureType FeatureType { get; set; }

        /// <summary>
        /// 父级ID（null表示根节点）
        /// </summary>
        public long? ParentId { get; set; }

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
        /// 状态：0-禁用，1-启用
        /// </summary>
        public int Status { get; set; } = 1;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// 更新功能节点请求
    /// </summary>
    public class UpdateFeaturePermissionRequest
    {
        /// <summary>
        /// 功能节点ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 功能名称
        /// </summary>
        public string FeatureName { get; set; } = string.Empty;

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
        /// 状态：0-禁用，1-启用
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// 调整功能节点顺序请求
    /// </summary>
    public class UpdateFeatureSortOrderRequest
    {
        /// <summary>
        /// 功能节点ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 新的排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 新的父级ID（用于移动节点到其他父级下）
        /// </summary>
        public long? ParentId { get; set; }
    }

    /// <summary>
    /// 关联API到功能按钮请求
    /// </summary>
    public class AssignApiToFeatureRequest
    {
        /// <summary>
        /// 功能按钮ID
        /// </summary>
        public long FeatureId { get; set; }

        /// <summary>
        /// API ID
        /// </summary>
        public long ApiId { get; set; }
    }

    /// <summary>
    /// 批量关联API到功能按钮请求
    /// </summary>
    public class BatchAssignApisToFeaturesRequest
    {
        /// <summary>
        /// 功能按钮ID
        /// </summary>
        public long FeatureId { get; set; }

        /// <summary>
        /// API ID列表
        /// </summary>
        public List<long> ApiIds { get; set; } = [];

        /// <summary>
        /// 是否替换现有关联（true-替换，false-追加）
        /// </summary>
        public bool IsReplace { get; set; } = true;
    }

    #endregion
}