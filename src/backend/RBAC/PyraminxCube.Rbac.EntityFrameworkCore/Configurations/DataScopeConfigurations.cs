using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PyraminxCube.Rbac.EntityFrameworkCore.Entities;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Configurations
{
    /// <summary>
    /// 数据权限维度表配置
    /// </summary>
    public class RbacDataDimensionConfiguration : IEntityTypeConfiguration<RbacDataDimension>
    {
        public void Configure(EntityTypeBuilder<RbacDataDimension> builder)
        {
            builder.ToTable("rbac_data_dimensions");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.DimensionCode).HasMaxLength(64).IsRequired();
            builder.Property(e => e.DimensionName).HasMaxLength(128).IsRequired();
            builder.Property(e => e.Description).HasMaxLength(512);
            builder.Property(e => e.SourceTable).HasMaxLength(128);
            builder.Property(e => e.SourceIdField).HasMaxLength(64);
            builder.Property(e => e.SourceNameField).HasMaxLength(64);

            builder.HasIndex(e => e.DimensionCode).IsUnique();
        }
    }

    /// <summary>
    /// 数据权限范围值表配置
    /// </summary>
    public class RbacDataScopeConfiguration : IEntityTypeConfiguration<RbacDataScope>
    {
        public void Configure(EntityTypeBuilder<RbacDataScope> builder)
        {
            builder.ToTable("rbac_data_scopes");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.DimensionCode).HasMaxLength(64).IsRequired();
            builder.Property(e => e.ScopeName).HasMaxLength(256).IsRequired();
            builder.Property(e => e.Path).HasMaxLength(512);

            builder.HasIndex(e => new { e.TenantId, e.DimensionCode, e.ScopeId }).IsUnique();
            builder.HasIndex(e => e.Path);
        }
    }

    /// <summary>
    /// 角色数据权限关联表配置
    /// </summary>
    public class RbacRoleDataScopeConfiguration : IEntityTypeConfiguration<RbacRoleDataScope>
    {
        public void Configure(EntityTypeBuilder<RbacRoleDataScope> builder)
        {
            builder.ToTable("rbac_role_data_scopes");
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.TenantId, e.RoleId, e.DataDimensionId, e.ScopeId }).IsUnique();

            builder.HasOne(e => e.Role)
                .WithMany(e => e.RoleDataScopes)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.DataDimension)
                .WithMany(e => e.RoleDataScopes)
                .HasForeignKey(e => e.DataDimensionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// 角色数据权限标记表配置
    /// </summary>
    public class RbacRoleDataScopeFlagConfiguration : IEntityTypeConfiguration<RbacRoleDataScopeFlag>
    {
        public void Configure(EntityTypeBuilder<RbacRoleDataScopeFlag> builder)
        {
            builder.ToTable("rbac_role_data_scope_flags");
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.TenantId, e.RoleId, e.DataDimensionId }).IsUnique();

            builder.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.DataDimension)
                .WithMany(e => e.RoleDataScopeFlags)
                .HasForeignKey(e => e.DataDimensionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// 数据维度映射配置表配置
    /// </summary>
    public class RbacDataDimensionMappingConfiguration : IEntityTypeConfiguration<RbacDataDimensionMapping>
    {
        public void Configure(EntityTypeBuilder<RbacDataDimensionMapping> builder)
        {
            builder.ToTable("rbac_data_dimension_mappings");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.EntityTypeName).HasMaxLength(512).IsRequired();
            builder.Property(e => e.PropertyName).HasMaxLength(128).IsRequired();

            builder.HasIndex(e => new { e.DataDimensionId, e.EntityTypeName }).IsUnique();

            builder.HasOne(e => e.DataDimension)
                .WithMany(e => e.DimensionMappings)
                .HasForeignKey(e => e.DataDimensionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
