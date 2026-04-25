namespace PyraminxCube.Rbac.Core.Models.Dto
{
    /// <summary>
    /// 用户查询参数
    /// </summary>
    public class UserQueryParams
    {
        /// <summary>
        /// 用户名（模糊匹配）
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// 邮箱（模糊匹配）
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 状态：null-全部，0-禁用，1-启用
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 创建时间开始
        /// </summary>
        public DateTimeOffset? CreatedTimeStart { get; set; }

        /// <summary>
        /// 创建时间结束
        /// </summary>
        public DateTimeOffset? CreatedTimeEnd { get; set; }

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
    /// 用户列表项
    /// </summary>
    public class UserListItem
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
        /// 状态：0-禁用，1-启用
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 是否为超级管理员
        /// </summary>
        public bool IsSuperAdmin { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// 头像URL
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// 已分配的角色列表
        /// </summary>
        public List<UserRoleInfo> Roles { get; set; } = [];

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedTime { get; set; }
    }

    /// <summary>
    /// 用户角色信息（简化的角色信息）
    /// </summary>
    public class UserRoleInfo
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
    /// 创建用户请求
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 密码（明文，服务内部加密）
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 昵称
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 是否为超级管理员
        /// </summary>
        public bool IsSuperAdmin { get; set; }

        /// <summary>
        /// 要分配的角色ID列表
        /// </summary>
        public List<long> RoleIds { get; set; } = [];
    }

    /// <summary>
    /// 更新用户请求
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 昵称
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 性别：0-未知，1-男，2-女
        /// </summary>
        public int? Gender { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateOnly? Birthday { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 个人简介
        /// </summary>
        public string? Bio { get; set; }
    }

    /// <summary>
    /// 重置密码请求
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 新密码（明文，服务内部加密）
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// 分配角色请求
    /// </summary>
    public class AssignRolesRequest
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 要分配的角色ID列表（完全替换原有角色）
        /// </summary>
        public List<long> RoleIds { get; set; } = [];
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
}
