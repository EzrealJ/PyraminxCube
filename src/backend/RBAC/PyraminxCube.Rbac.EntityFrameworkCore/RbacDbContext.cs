using Microsoft.EntityFrameworkCore;
using PyraminxCube.Rbac.EntityFrameworkCore.Entities;

namespace PyraminxCube.Rbac.EntityFrameworkCore
{
    /// <summary>
    /// RBAC数据库上下文
    /// </summary>
    public class RbacDbContext : DbContext
    {
        public RbacDbContext(DbContextOptions<RbacDbContext> options) : base(options)
        {
        }

        #region 用户模块
        public DbSet<RbacUser> Users => Set<RbacUser>();
        public DbSet<RbacUserProfile> UserProfiles => Set<RbacUserProfile>();
        #endregion

        #region 角色模块
        public DbSet<RbacRole> Roles => Set<RbacRole>();
        public DbSet<RbacUserRole> UserRoles => Set<RbacUserRole>();
        #endregion

        #region API权限模块
        public DbSet<RbacApiGroup> ApiGroups => Set<RbacApiGroup>();
        public DbSet<RbacApiPermission> ApiPermissions => Set<RbacApiPermission>();
        public DbSet<RbacApiGroupMapping> ApiGroupMappings => Set<RbacApiGroupMapping>();
        public DbSet<RbacRoleApiPermission> RoleApiPermissions => Set<RbacRoleApiPermission>();
        #endregion

        #region 功能权限模块
        public DbSet<RbacFeaturePermission> FeaturePermissions => Set<RbacFeaturePermission>();
        public DbSet<RbacRoleFeaturePermission> RoleFeaturePermissions => Set<RbacRoleFeaturePermission>();
        public DbSet<RbacFeatureApiMapping> FeatureApiMappings => Set<RbacFeatureApiMapping>();
        #endregion

        #region 数据权限模块
        public DbSet<RbacDataDimension> DataDimensions => Set<RbacDataDimension>();
        public DbSet<RbacDataScope> DataScopes => Set<RbacDataScope>();
        public DbSet<RbacRoleDataScope> RoleDataScopes => Set<RbacRoleDataScope>();
        public DbSet<RbacRoleDataScopeFlag> RoleDataScopeFlags => Set<RbacRoleDataScopeFlag>();
        public DbSet<RbacDataDimensionMapping> DataDimensionMappings => Set<RbacDataDimensionMapping>();
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 应用所有实体配置
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RbacDbContext).Assembly);

            // 全局查询过滤器：软删除
            ConfigureSoftDeleteFilter(modelBuilder);
        }

        private static void ConfigureSoftDeleteFilter(ModelBuilder modelBuilder)
        {
            // 租户实体的软删除过滤
            modelBuilder.Entity<RbacUser>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacUserProfile>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacRole>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacUserRole>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacRoleApiPermission>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacRoleFeaturePermission>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacDataScope>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacRoleDataScope>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacRoleDataScopeFlag>().HasQueryFilter(e => !e.IsDeleted);

            // 全局实体的软删除过滤
            modelBuilder.Entity<RbacApiGroup>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacApiPermission>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacApiGroupMapping>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacFeaturePermission>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacFeatureApiMapping>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacDataDimension>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RbacDataDimensionMapping>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
