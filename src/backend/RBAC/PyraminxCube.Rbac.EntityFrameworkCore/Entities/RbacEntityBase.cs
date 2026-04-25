using PyraminxCube.Models.Abstractions.DataBase;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Entities
{
    /// <summary>
    /// RBAC实体基类（带租户隔离）
    /// </summary>
    public abstract class RbacEntity : DbNormalEntity<long, DateTimeOffset>
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        public long TenantId { get; set; }
    }

    /// <summary>
    /// RBAC全局实体基类（不带租户隔离，如API定义、功能定义）
    /// </summary>
    public abstract class RbacGlobalEntity : DbNormalEntity<long, DateTimeOffset>
    {
    }
}
