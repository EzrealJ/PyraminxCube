using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PyraminxCube.Rbac.EntityFrameworkCore.Entities;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Configurations
{
    /// <summary>
    /// API分组表配置
    /// </summary>
    public class RbacApiGroupConfiguration : IEntityTypeConfiguration<RbacApiGroup>
    {
        public void Configure(EntityTypeBuilder<RbacApiGroup> builder)
        {
            builder.ToTable("rbac_api_groups");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.GroupCode).HasMaxLength(64).IsRequired();
            builder.Property(e => e.GroupName).HasMaxLength(128).IsRequired();
            builder.Property(e => e.Description).HasMaxLength(512);

            builder.HasIndex(e => e.GroupCode).IsUnique();
        }
    }

    /// <summary>
    /// API权限表配置
    /// </summary>
    public class RbacApiPermissionConfiguration : IEntityTypeConfiguration<RbacApiPermission>
    {
        public void Configure(EntityTypeBuilder<RbacApiPermission> builder)
        {
            builder.ToTable("rbac_api_permissions");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.ApiCode).HasMaxLength(128).IsRequired();
            builder.Property(e => e.ApiName).HasMaxLength(256).IsRequired();
            builder.Property(e => e.Endpoint).HasMaxLength(512).IsRequired();
            builder.Property(e => e.HttpMethod).HasMaxLength(16).IsRequired();
            builder.Property(e => e.Description).HasMaxLength(512);

            builder.HasIndex(e => e.ApiCode).IsUnique();
            builder.HasIndex(e => new { e.Endpoint, e.HttpMethod });
        }
    }

    /// <summary>
    /// API分组映射表配置
    /// </summary>
    public class RbacApiGroupMappingConfiguration : IEntityTypeConfiguration<RbacApiGroupMapping>
    {
        public void Configure(EntityTypeBuilder<RbacApiGroupMapping> builder)
        {
            builder.ToTable("rbac_api_group_mappings");
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.ApiGroupId, e.ApiPermissionId }).IsUnique();

            builder.HasOne(e => e.ApiGroup)
                .WithMany(e => e.ApiGroupMappings)
                .HasForeignKey(e => e.ApiGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.ApiPermission)
                .WithMany(e => e.ApiGroupMappings)
                .HasForeignKey(e => e.ApiPermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// 角色API权限关联表配置
    /// </summary>
    public class RbacRoleApiPermissionConfiguration : IEntityTypeConfiguration<RbacRoleApiPermission>
    {
        public void Configure(EntityTypeBuilder<RbacRoleApiPermission> builder)
        {
            builder.ToTable("rbac_role_api_permissions");
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.TenantId, e.RoleId, e.ApiPermissionId }).IsUnique();

            builder.HasOne(e => e.Role)
                .WithMany(e => e.RoleApiPermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.ApiPermission)
                .WithMany(e => e.RoleApiPermissions)
                .HasForeignKey(e => e.ApiPermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// 功能权限表配置
    /// </summary>
    public class RbacFeaturePermissionConfiguration : IEntityTypeConfiguration<RbacFeaturePermission>
    {
        public void Configure(EntityTypeBuilder<RbacFeaturePermission> builder)
        {
            builder.ToTable("rbac_feature_permissions");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.FeatureCode).HasMaxLength(128).IsRequired();
            builder.Property(e => e.FeatureName).HasMaxLength(256).IsRequired();
            builder.Property(e => e.ParentCode).HasMaxLength(128);
            builder.Property(e => e.Path).HasMaxLength(512);
            builder.Property(e => e.Icon).HasMaxLength(128);
            builder.Property(e => e.Description).HasMaxLength(512);

            builder.HasIndex(e => e.FeatureCode).IsUnique();
            builder.HasIndex(e => e.ParentCode);
        }
    }

    /// <summary>
    /// 角色功能权限关联表配置
    /// </summary>
    public class RbacRoleFeaturePermissionConfiguration : IEntityTypeConfiguration<RbacRoleFeaturePermission>
    {
        public void Configure(EntityTypeBuilder<RbacRoleFeaturePermission> builder)
        {
            builder.ToTable("rbac_role_feature_permissions");
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.TenantId, e.RoleId, e.FeaturePermissionId }).IsUnique();

            builder.HasOne(e => e.Role)
                .WithMany(e => e.RoleFeaturePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.FeaturePermission)
                .WithMany(e => e.RoleFeaturePermissions)
                .HasForeignKey(e => e.FeaturePermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// 功能API映射表配置
    /// </summary>
    public class RbacFeatureApiMappingConfiguration : IEntityTypeConfiguration<RbacFeatureApiMapping>
    {
        public void Configure(EntityTypeBuilder<RbacFeatureApiMapping> builder)
        {
            builder.ToTable("rbac_feature_api_mappings");
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.FeaturePermissionId, e.ApiPermissionId }).IsUnique();

            builder.HasOne(e => e.FeaturePermission)
                .WithMany(e => e.FeatureApiMappings)
                .HasForeignKey(e => e.FeaturePermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.ApiPermission)
                .WithMany(e => e.FeatureApiMappings)
                .HasForeignKey(e => e.ApiPermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
