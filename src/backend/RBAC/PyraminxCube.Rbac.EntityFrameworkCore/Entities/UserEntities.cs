namespace PyraminxCube.Rbac.EntityFrameworkCore.Entities
{
    /// <summary>
    /// 用户表
    /// </summary>
    public class RbacUser : RbacEntity
    {
        /// <summary>
        /// 用户名（唯一）
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱（唯一）
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 加密后的密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 状态：0-禁用，1-启用
        /// </summary>
        public int Status { get; set; } = 1;

        /// <summary>
        /// 是否为超级管理员
        /// </summary>
        public bool IsSuperAdmin { get; set; }

        /// <summary>
        /// 用户扩展信息
        /// </summary>
        public RbacUserProfile? Profile { get; set; }

        /// <summary>
        /// 用户角色关联
        /// </summary>
        public ICollection<RbacUserRole> UserRoles { get; set; } = [];
    }

    /// <summary>
    /// 用户扩展表
    /// </summary>
    public class RbacUserProfile : RbacEntity
    {
        /// <summary>
        /// 关联用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 头像URL
        /// </summary>
        public string? Avatar { get; set; }

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
        public int Gender { get; set; }

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

        /// <summary>
        /// 关联用户
        /// </summary>
        public RbacUser? User { get; set; }
    }
}
