using PyraminxCube.Rbac.Core.Models.Dto;

namespace PyraminxCube.Rbac.Core.Services
{
    /// <summary>
    /// 数据维度服务接口
    /// 提供数据权限维度管理相关的业务功能（D-001 ~ D-010）
    /// </summary>
    /// <remarks>
    /// 数据维度是全局实体，不需要 tenantId 参数
    /// </remarks>
    public interface IDataDimensionService
    {
        #region D-001 维度列表

        /// <summary>
        /// 分页查询数据维度列表
        /// </summary>
        /// <param name="queryParams">查询参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>分页数据维度列表</returns>
        Task<PagedResult<DataDimensionListItem>> GetPagedListAsync(
            DataDimensionQueryParams queryParams,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取所有数据维度列表（下拉选择用）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>数据维度列表</returns>
        Task<List<DataDimensionListItem>> GetAllListAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取数据维度详情
        /// </summary>
        /// <param name="dimensionId">维度ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>数据维度详情</returns>
        Task<DataDimensionListItem?> GetByIdAsync(
            long dimensionId,
            CancellationToken cancellationToken = default);

        #endregion

        #region D-002 新增维度

        /// <summary>
        /// 创建数据维度
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>新创建的维度ID</returns>
        Task<long> CreateAsync(
            CreateDataDimensionRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region D-003 编辑维度

        /// <summary>
        /// 更新数据维度
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task UpdateAsync(
            UpdateDataDimensionRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region D-004 删除维度

        /// <summary>
        /// 删除数据维度（检查是否有使用）
        /// </summary>
        /// <param name="dimensionId">维度ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>删除结果，包含使用情况</returns>
        Task<DimensionUsageCheckResult> DeleteAsync(
            long dimensionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查维度是否正在使用
        /// </summary>
        /// <param name="dimensionId">维度ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>使用情况检查结果</returns>
        Task<DimensionUsageCheckResult> CheckUsageAsync(
            long dimensionId,
            CancellationToken cancellationToken = default);

        #endregion

        #region D-005 维度值列表

        /// <summary>
        /// 分页查询维度值列表
        /// </summary>
        /// <param name="queryParams">查询参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>分页维度值列表</returns>
        Task<PagedResult<DataScopeListItem>> GetScopePagedListAsync(
            DataScopeQueryParams queryParams,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取维度值详情
        /// </summary>
        /// <param name="scopeId">范围ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>维度值详情</returns>
        Task<DataScopeListItem?> GetScopeByIdAsync(
            long scopeId,
            CancellationToken cancellationToken = default);

        #endregion

        #region D-006 新增维度值

        /// <summary>
        /// 创建维度值
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>新创建的维度值ID</returns>
        Task<long> CreateScopeAsync(
            CreateDataScopeRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region D-007 编辑维度值

        /// <summary>
        /// 更新维度值
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task UpdateScopeAsync(
            UpdateDataScopeRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region D-008 删除维度值

        /// <summary>
        /// 删除维度值
        /// </summary>
        /// <param name="scopeId">范围ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>删除结果，包含使用情况</returns>
        Task<DataScopeUsageCheckResult> DeleteScopeAsync(
            long scopeId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查维度值是否正在使用
        /// </summary>
        /// <param name="scopeId">范围ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>使用情况检查结果</returns>
        Task<DataScopeUsageCheckResult> CheckScopeUsageAsync(
            long scopeId,
            CancellationToken cancellationToken = default);

        #endregion

        #region D-009 维度值树管理

        /// <summary>
        /// 获取维度值树形结构
        /// </summary>
        /// <param name="dimensionId">维度ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>树形结构列表</returns>
        Task<List<DataScopeTreeNode>> GetScopeTreeAsync(
            long dimensionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 移动维度值（调整层级关系）
        /// </summary>
        /// <param name="request">移动请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task MoveScopeAsync(
            MoveDataScopeRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量更新维度值排序
        /// </summary>
        /// <param name="dimensionId">维度ID</param>
        /// <param name="sortOrders">排序信息列表（ScopeId, SortOrder）</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task UpdateScopeSortOrdersAsync(
            long dimensionId,
            Dictionary<long, int> sortOrders,
            CancellationToken cancellationToken = default);

        #endregion

        #region D-010 维度映射配置

        /// <summary>
        /// 获取维度映射列表
        /// </summary>
        /// <param name="queryParams">查询参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>维度映射列表</returns>
        Task<List<DataDimensionMappingListItem>> GetMappingListAsync(
            DataDimensionMappingQueryParams queryParams,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 创建维度映射
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>新创建的映射ID</returns>
        Task<long> CreateMappingAsync(
            CreateDataDimensionMappingRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新维度映射
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task UpdateMappingAsync(
            UpdateDataDimensionMappingRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 删除维度映射
        /// </summary>
        /// <param name="mappingId">映射ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task DeleteMappingAsync(
            long mappingId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取维度映射详情
        /// </summary>
        /// <param name="mappingId">映射ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>维度映射详情</returns>
        Task<DataDimensionMappingListItem?> GetMappingByIdAsync(
            long mappingId,
            CancellationToken cancellationToken = default);

        #endregion
    }
}