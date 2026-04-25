using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PyraminxCube.Rbac.EntityFrameworkCore.Entities;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Configurations
{
    /// <summary>
    /// 用户表配置
    /// </summary>
    public class RbacUserConfiguration : IEntityTypeConfiguration<RbacUser>
    {
        public void Configure(EntityTypeBuilder<RbacUser> builder)
        {
            builder.ToTable("rbac_users");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Username).HasMaxLength(64).IsRequired();
            builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
            builder.Property(e => e.Password).HasMaxLength(256).IsRequired();

            builder.HasIndex(e => new { e.TenantId, e.Username }).IsUnique();
            builder.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();

            builder.HasOne(e => e.Profile)
                .WithOne(e => e.User)
                .HasForeignKey<RbacUserProfile>(e => e.UserId);
        }
    }

    /// <summary>
    /// 用户扩展表配置
    /// </summary>
    public class RbacUserProfileConfiguration : IEntityTypeConfiguration<RbacUserProfile>
    {
        public void Configure(EntityTypeBuilder<RbacUserProfile> builder)
        {
            builder.ToTable("rbac_user_profiles");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Avatar).HasMaxLength(512);
            builder.Property(e => e.Nickname).HasMaxLength(64);
            builder.Property(e => e.PhoneNumber).HasMaxLength(32);
            builder.Property(e => e.Address).HasMaxLength(512);
            builder.Property(e => e.Bio).HasMaxLength(1024);

            builder.HasIndex(e => e.UserId).IsUnique();
        }
    }

    /// <summary>
    /// 角色表配置
    /// </summary>
    public class RbacRoleConfiguration : IEntityTypeConfiguration<RbacRole>
    {
        public void Configure(EntityTypeBuilder<RbacRole> builder)
        {
            builder.ToTable("rbac_roles");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.RoleCode).HasMaxLength(64).IsRequired();
            builder.Property(e => e.RoleName).HasMaxLength(128).IsRequired();
            builder.Property(e => e.Description).HasMaxLength(512);

            builder.HasIndex(e => new { e.TenantId, e.RoleCode }).IsUnique();
        }
    }

    /// <summary>
    /// 用户角色关联表配置
    /// </summary>
    public class RbacUserRoleConfiguration : IEntityTypeConfiguration<RbacUserRole>
    {
        public void Configure(EntityTypeBuilder<RbacUserRole> builder)
        {
            builder.ToTable("rbac_user_roles");
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.TenantId, e.UserId, e.RoleId }).IsUnique();

            builder.HasOne(e => e.User)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Role)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
