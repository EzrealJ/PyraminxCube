namespace PyraminxCube.Rbac.Core.Services
{
    /// <summary>
    /// 角色服务接口
    /// 提供角色管理相关的业务功能（R-001 ~ R-009）
    /// </summary>
    public interface IRoleService
    {
        #region R-001 角色列表

        /// <summary>
        /// 分页查询角色列表
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="queryParams">查询参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>分页角色列表</returns>
        Task<PagedResult<RoleListItem>> GetPagedListAsync(
            long tenantId,
            RoleQueryParams queryParams,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取角色详情
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>角色详情</returns>
        Task<RoleDetail?> GetDetailAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取所有角色列表（用于下拉选择）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>角色列表</returns>
        Task<List<RoleSimpleInfo>> GetAllAsync(
            long tenantId,
            CancellationToken cancellationToken = default);

        #endregion

        #region R-002 新增角色

        /// <summary>
        /// 创建新角色
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="request">创建角色请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>新创建的角色ID</returns>
        Task<long> CreateAsync(
            long tenantId,
            CreateRoleRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region R-003 编辑角色

        /// <summary>
        /// 更新角色信息
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="request">更新角色请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task UpdateAsync(
            long tenantId,
            UpdateRoleRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region R-004 删除角色

        /// <summary>
        /// 删除角色（需检查是否有用户使用）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <exception cref="InvalidOperationException">当角色被用户使用时抛出</exception>
        Task DeleteAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查角色是否可以被删除
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>检查结果：canDelete-是否可删除，userCount-关联用户数</returns>
        Task<(bool CanDelete, int UserCount)> CheckCanDeleteAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default);

        #endregion

        #region R-005 为角色分配API权限

        /// <summary>
        /// 为角色分配API权限（完全替换原有权限）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="request">分配API权限请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AssignApiPermissionsAsync(
            long tenantId,
            AssignApiPermissionsRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取角色已分配的API权限ID列表
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>API权限ID列表</returns>
        Task<RoleApiPermissionInfo> GetApiPermissionsAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default);

        #endregion

        #region R-006 为角色分配功能权限

        /// <summary>
        /// 为角色分配功能权限（完全替换原有权限）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="request">分配功能权限请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AssignFeaturePermissionsAsync(
            long tenantId,
            AssignFeaturePermissionsRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取角色已分配的功能权限ID列表
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>功能权限ID列表</returns>
        Task<List<long>> GetFeaturePermissionIdsAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default);

        #endregion

        #region R-007 为角色分配数据权限

        /// <summary>
        /// 为角色分配数据权限（完全替换原有权限）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="request">分配数据权限请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AssignDataScopesAsync(
            long tenantId,
            AssignDataScopesRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取角色的数据权限配置
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>数据权限配置信息</returns>
        Task<List<RoleDataScopeInfo>> GetDataScopesAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default);

        #endregion

        #region R-008 复制角色

        /// <summary>
        /// 复制角色（复制角色的所有权限配置）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="request">复制角色请求</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>新创建的角色ID</returns>
        Task<long> CopyAsync(
            long tenantId,
            CopyRoleRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region R-009 查看角色用户

        /// <summary>
        /// 获取拥有该角色的用户列表（分页）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <param name="queryParams">查询参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>分页用户列表</returns>
        Task<PagedResult<RoleUserInfo>> GetUsersAsync(
            long tenantId,
            long roleId,
            RoleUserQueryParams queryParams,
            CancellationToken cancellationToken = default);

        #endregion
    }

    #region 角色相关DTO

    /// <summary>
    /// 角色查询参数
    /// </summary>
    public class RoleQueryParams
    {
        /// <summary>
        /// 角色编码（模糊匹配）
        /// </summary>
        public string? RoleCode { get; set; }

        /// <summary>
        /// 角色名称（模糊匹配）
        /// </summary>
        public string? RoleName { get; set; }

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
    /// 角色列表项
    /// </summary>
    public class RoleListItem
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 角色编码
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
        /// 关联用户数
        /// </summary>
        public int UserCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedTime { get; set; }
    }

    /// <summary>
    /// 角色详情
    /// </summary>
    public class RoleDetail
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 角色编码
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
        /// 关联用户数
        /// </summary>
        public int UserCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTimeOffset? ModifiedTime { get; set; }
    }

    /// <summary>
    /// 角色简单信息（用于下拉选择）
    /// </summary>
    public class RoleSimpleInfo
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 角色编码
        /// </summary>
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 创建角色请求
    /// </summary>
    public class CreateRoleRequest
    {
        /// <summary>
        /// 角色编码
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
    }

    /// <summary>
    /// 更新角色请求
    /// </summary>
    public class UpdateRoleRequest
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long Id { get; set; }

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
    }

    /// <summary>
    /// 分配API权限请求
    /// </summary>
    public class AssignApiPermissionsRequest
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// API权限ID列表（完全替换原有权限）
        /// </summary>
        public List<long> ApiPermissionIds { get; set; } = [];

        /// <summary>
        /// API分组ID列表（完全替换原有分组权限）
        /// </summary>
        public List<long> ApiGroupIds { get; set; } = [];
    }

    /// <summary>
    /// 角色API权限信息
    /// </summary>
    public class RoleApiPermissionInfo
    {
        /// <summary>
        /// 已分配的API权限ID列表
        /// </summary>
        public List<long> ApiPermissionIds { get; set; } = [];

        /// <summary>
        /// 已分配的API分组ID列表
        /// </summary>
        public List<long> ApiGroupIds { get; set; } = [];
    }

    /// <summary>
    /// 分配功能权限请求
    /// </summary>
    public class AssignFeaturePermissionsRequest
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 功能权限ID列表（完全替换原有权限）
        /// </summary>
        public List<long> FeaturePermissionIds { get; set; } = [];
    }

    /// <summary>
    /// 分配数据权限请求
    /// </summary>
    public class AssignDataScopesRequest
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 数据权限配置列表（按维度配置）
        /// </summary>
        public List<RoleDataScopeConfig> DataScopeConfigs { get; set; } = [];
    }

    /// <summary>
    /// 角色数据权限配置（按维度）
    /// </summary>
    public class RoleDataScopeConfig
    {
        /// <summary>
        /// 维度ID
        /// </summary>
        public long DimensionId { get; set; }

        /// <summary>
        /// 特殊标记：null-自定义，ALL-全部，SELF-仅自己
        /// </summary>
        public string? ScopeFlag { get; set; }

        /// <summary>
        /// 数据范围ID列表（当ScopeFlag为null时使用）
        /// </summary>
        public List<long> ScopeIds { get; set; } = [];
    }

    /// <summary>
    /// 角色数据权限信息
    /// </summary>
    public class RoleDataScopeInfo
    {
        /// <summary>
        /// 维度ID
        /// </summary>
        public long DimensionId { get; set; }

        /// <summary>
        /// 维度编码
        /// </summary>
        public string DimensionCode { get; set; } = string.Empty;

        /// <summary>
        /// 维度名称
        /// </summary>
        public string DimensionName { get; set; } = string.Empty;

        /// <summary>
        /// 特殊标记：null-自定义，ALL-全部，SELF-仅自己
        /// </summary>
        public string? ScopeFlag { get; set; }

        /// <summary>
        /// 数据范围ID列表
        /// </summary>
        public List<long> ScopeIds { get; set; } = [];
    }

    /// <summary>
    /// 复制角色请求
    /// </summary>
    public class CopyRoleRequest
    {
        /// <summary>
        /// 源角色ID
        /// </summary>
        public long SourceRoleId { get; set; }

        /// <summary>
        /// 新角色编码
        /// </summary>
        public string NewRoleCode { get; set; } = string.Empty;

        /// <summary>
        /// 新角色名称
        /// </summary>
        public string NewRoleName { get; set; } = string.Empty;

        /// <summary>
        /// 新角色描述
        /// </summary>
        public string? NewDescription { get; set; }

        /// <summary>
        /// 是否复制API权限
        /// </summary>
        public bool CopyApiPermissions { get; set; } = true;

        /// <summary>
        /// 是否复制功能权限
        /// </summary>
        public bool CopyFeaturePermissions { get; set; } = true;

        /// <summary>
        /// 是否复制数据权限
        /// </summary>
        public bool CopyDataScopes { get; set; } = true;
    }

    /// <summary>
    /// 角色用户查询参数
    /// </summary>
    public class RoleUserQueryParams
    {
        /// <summary>
        /// 用户名（模糊匹配）
        /// </summary>
        public string? Username { get; set; }

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
    /// 角色用户信息
    /// </summary>
    public class RoleUserInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 昵称
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// 状态：0-禁用，1-启用
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedTime { get; set; }
    }

    /// <summary>
    /// 分页结果
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> Items { get; set; } = [];

        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页条数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    #endregion
}
