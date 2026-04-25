using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PyraminxCube.Rbac.EntityFrameworkCore
{
    /// <summary>
    /// 设计时 DbContext 工厂（用于 EF Core 迁移）
    /// </summary>
    /// <remarks>
    /// 使用方法：
    /// <code>
    /// cd src/backend/RBAC/PyraminxCube.Rbac.EntityFrameworkCore
    /// dotnet ef migrations add InitRbac
    /// dotnet ef database update
    /// </code>
    /// </remarks>
    public class RbacDbContextDesignTimeFactory : IDesignTimeDbContextFactory<RbacDbContext>
    {
        public RbacDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RbacDbContext>();

            // 设计时默认使用 SQLite
            optionsBuilder.UseSqlite("Data Source=rbac.db");

            return new RbacDbContext(optionsBuilder.Options);
        }
    }
}
