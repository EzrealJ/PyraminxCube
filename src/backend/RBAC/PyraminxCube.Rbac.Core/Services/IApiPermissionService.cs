namespace PyraminxCube.Rbac.Core.Services
{
    /// <summary>
    /// API权限服务接口
    /// 提供API权限管理相关的业务功能（A-001 ~ A-010）
    /// </summary>
    public interface IApiPermissionService
    {
        #region A-001 API列表

        /// <summary>
        /// 分页查询API列表
        /// </summary>
        /// <param name="queryParams">查询参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>分页API列表</returns>
        Task<PagedResult<ApiPermissionListItem>> GetPagedListAsync(
            ApiPermissionQueryParams queryParams,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取所有API列表（用于下拉选择）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>API列表</returns>
        Task<List<ApiPermissionSimpleInfo>> GetAllAsync(
            CancellationToken cancellationToken = default);

        #endregion

        #region A-002 新增API

        /// <summary>
        /// 创建新API
        /// </summary>
        /// <param name="request">创建API请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>新创建的API ID</returns>
        Task<long> CreateAsync(
            CreateApiPermissionRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region A-003 编辑API

        /// <summary>
        /// 更新API信息
        /// </summary>
        /// <param name="request">更新API请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task UpdateAsync(
            UpdateApiPermissionRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region A-004 删除API

        /// <summary>
        /// 删除API（软删除）
        /// </summary>
        /// <param name="apiId">API ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task DeleteAsync(
            long apiId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量删除API
        /// </summary>
        /// <param name="apiIds">API ID列表</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task DeleteBatchAsync(
            List<long> apiIds,
            CancellationToken cancellationToken = default);

        #endregion

        #region A-005 API分组列表

        /// <summary>
        /// 获取所有API分组列表
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>API分组列表</returns>
        Task<List<ApiGroupListItem>> GetGroupsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取API分组及其下的API列表（用于权限分配界面）
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>API分组及其下的API列表</returns>
        Task<List<ApiGroupWithApis>> GetGroupsWithApisAsync(
            CancellationToken cancellationToken = default);

        #endregion

        #region A-006 新增API分组

        /// <summary>
        /// 创建新的API分组
        /// </summary>
        /// <param name="request">创建API分组请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>新创建的分组ID</returns>
        Task<long> CreateGroupAsync(
            CreateApiGroupRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region A-007 编辑API分组

        /// <summary>
        /// 更新API分组信息
        /// </summary>
        /// <param name="request">更新API分组请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task UpdateGroupAsync(
            UpdateApiGroupRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region A-008 删除API分组

        /// <summary>
        /// 删除API分组
        /// </summary>
        /// <param name="groupId">分组ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <exception cref="InvalidOperationException">当分组下存在API时抛出</exception>
        Task DeleteGroupAsync(
            long groupId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查API分组是否可以被删除
        /// </summary>
        /// <param name="groupId">分组ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>检查结果：canDelete-是否可删除，apiCount-关联API数</returns>
        Task<(bool CanDelete, int ApiCount)> CheckGroupCanDeleteAsync(
            long groupId,
            CancellationToken cancellationToken = default);

        #endregion

        #region A-009 API分配到分组

        /// <summary>
        /// 将API分配到指定分组
        /// </summary>
        /// <param name="request">分配分组请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AssignToGroupAsync(
            AssignApiToGroupRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量将API分配到指定分组
        /// </summary>
        /// <param name="request">批量分配分组请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AssignBatchToGroupAsync(
            AssignBatchApiToGroupRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region A-010 自动扫描API

        /// <summary>
        /// 从代码注解自动扫描API
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>扫描结果</returns>
        Task<ApiScanResult> ScanFromCodeAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 导入扫描的API
        /// </summary>
        /// <param name="request">导入请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>导入结果</returns>
        Task<ApiImportResult> ImportScannedApisAsync(
            ImportScannedApisRequest request,
            CancellationToken cancellationToken = default);

        #endregion
    }

    #region API权限相关DTO

    /// <summary>
    /// API权限查询参数
    /// </summary>
    public class ApiPermissionQueryParams
    {
        /// <summary>
        /// API名称/路径（模糊匹配）
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// HTTP方法（GET/POST/PUT/DELETE等）
        /// </summary>
        public string? HttpMethod { get; set; }

        /// <summary>
        /// 所属分组ID
        /// </summary>
        public long? GroupId { get; set; }

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
    /// API权限列表项
    /// </summary>
    public class ApiPermissionListItem
    {
        /// <summary>
        /// API ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// API编码
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
        /// HTTP方法
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// 所属分组ID
        /// </summary>
        public long? GroupId { get; set; }

        /// <summary>
        /// 所属分组名称
        /// </summary>
        public string? GroupName { get; set; }

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
    /// API权限简单信息（用于下拉选择）
    /// </summary>
    public class ApiPermissionSimpleInfo
    {
        /// <summary>
        /// API ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// API编码
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
        /// HTTP方法
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// 创建API请求
    /// </summary>
    public class CreateApiPermissionRequest
    {
        /// <summary>
        /// API编码
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
        /// HTTP方法
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// 所属分组ID
        /// </summary>
        public long? GroupId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// 更新API请求
    /// </summary>
    public class UpdateApiPermissionRequest
    {
        /// <summary>
        /// API ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// API名称
        /// </summary>
        public string ApiName { get; set; } = string.Empty;

        /// <summary>
        /// 接口路径
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// HTTP方法
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// 所属分组ID
        /// </summary>
        public long? GroupId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
    }

    #endregion

    #region API分组相关DTO

    /// <summary>
    /// API分组列表项
    /// </summary>
    public class ApiGroupListItem
    {
        /// <summary>
        /// 分组ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分组编码
        /// </summary>
        public string GroupCode { get; set; } = string.Empty;

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// API数量
        /// </summary>
        public int ApiCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedTime { get; set; }
    }

    /// <summary>
    /// API分组及其下的API列表
    /// </summary>
    public class ApiGroupWithApis
    {
        /// <summary>
        /// 分组ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分组编码
        /// </summary>
        public string GroupCode { get; set; } = string.Empty;

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 该分组下的API列表
        /// </summary>
        public List<ApiPermissionSimpleInfo> Apis { get; set; } = [];
    }

    /// <summary>
    /// 创建API分组请求
    /// </summary>
    public class CreateApiGroupRequest
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
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 更新API分组请求
    /// </summary>
    public class UpdateApiGroupRequest
    {
        /// <summary>
        /// 分组ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

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
    /// 将API分配到分组请求
    /// </summary>
    public class AssignApiToGroupRequest
    {
        /// <summary>
        /// API ID
        /// </summary>
        public long ApiId { get; set; }

        /// <summary>
        /// 目标分组ID（null表示移除分组）
        /// </summary>
        public long? GroupId { get; set; }
    }

    /// <summary>
    /// 批量将API分配到分组请求
    /// </summary>
    public class AssignBatchApiToGroupRequest
    {
        /// <summary>
        /// API ID列表
        /// </summary>
        public List<long> ApiIds { get; set; } = [];

        /// <summary>
        /// 目标分组ID（null表示移除分组）
        /// </summary>
        public long? GroupId { get; set; }
    }

    #endregion

    #region API扫描相关DTO

    /// <summary>
    /// API扫描结果
    /// </summary>
    public class ApiScanResult
    {
        /// <summary>
        /// 扫描发现的API总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 新增的API数量
        /// </summary>
        public int NewCount { get; set; }

        /// <summary>
        /// 已存在的API数量
        /// </summary>
        public int ExistingCount { get; set; }

        /// <summary>
        /// 扫描到的API列表
        /// </summary>
        public List<ScannedApi> Apis { get; set; } = [];
    }

    /// <summary>
    /// 扫描到的API信息
    /// </summary>
    public class ScannedApi
    {
        /// <summary>
        /// API编码
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
        /// HTTP方法
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// 所属分组名称（从Controller推断）
        /// </summary>
        public string? GroupName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否为新API（数据库中不存在）
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// 如果已存在，对应的API ID
        /// </summary>
        public long? ExistingId { get; set; }
    }

    /// <summary>
    /// 导入扫描API请求
    /// </summary>
    public class ImportScannedApisRequest
    {
        /// <summary>
        /// 要导入的API列表
        /// </summary>
        public List<ScannedApi> Apis { get; set; } = [];

        /// <summary>
        /// 导入策略：SkipExisting-跳过已存在，UpdateExisting-更新已存在
        /// </summary>
        public ImportStrategy Strategy { get; set; } = ImportStrategy.SkipExisting;

        /// <summary>
        /// 是否自动创建分组（如果分组不存在）
        /// </summary>
        public bool AutoCreateGroup { get; set; } = true;
    }

    /// <summary>
    /// 导入策略
    /// </summary>
    public enum ImportStrategy
    {
        /// <summary>
        /// 跳过已存在的API
        /// </summary>
        SkipExisting = 1,

        /// <summary>
        /// 更新已存在的API
        /// </summary>
        UpdateExisting = 2
    }

    /// <summary>
    /// API导入结果
    /// </summary>
    public class ApiImportResult
    {
        /// <summary>
        /// 新增数量
        /// </summary>
        public int AddedCount { get; set; }

        /// <summary>
        /// 更新数量
        /// </summary>
        public int UpdatedCount { get; set; }

        /// <summary>
        /// 跳过数量
        /// </summary>
        public int SkippedCount { get; set; }

        /// <summary>
        /// 新创建的分组数量
        /// </summary>
        public int CreatedGroupCount { get; set; }
    }

    #endregion
}