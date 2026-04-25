using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PyraminxCube.Rbac.Core.Abstractions;
using PyraminxCube.Rbac.EntityFrameworkCore.Services;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// RBAC EntityFrameworkCore 服务注册扩展
    /// </summary>
    public static class RbacEfCoreServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 RBAC EntityFrameworkCore 服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">配置委托</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddRbacEntityFrameworkCore(
            this IServiceCollection services,
            Action<RbacDbOptions> configure)
        {
            var options = new RbacDbOptions();
            configure(options);

            // 注册 DbContext
            services.AddDbContext<RbacDbContext>(dbContextOptions =>
            {
                ConfigureDbContext(dbContextOptions, options);
            });

            // 注册服务实现
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IDataScopeService, DataScopeService>();
            services.AddScoped<IPermissionCache, PermissionCache>();

            // 注册维度映射配置
            services.AddSingleton<IDataDimensionMapping, DataDimensionMapping>();

            return services;
        }

        /// <summary>
        /// 添加 RBAC EntityFrameworkCore 服务（使用已存在的 DbContext 配置）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddRbacEntityFrameworkCore(this IServiceCollection services)
        {
            // 注册服务实现
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IDataScopeService, DataScopeService>();
            services.AddScoped<IPermissionCache, PermissionCache>();

            // 注册维度映射配置
            services.AddSingleton<IDataDimensionMapping, DataDimensionMapping>();

            return services;
        }

        /// <summary>
        /// 配置 DbContext
        /// </summary>
        private static void ConfigureDbContext(DbContextOptionsBuilder builder, RbacDbOptions options)
        {
            switch (options.DbType)
            {
                case RbacDbType.Sqlite:
                    builder.UseSqlite(options.ConnectionString);
                    break;

                case RbacDbType.MySql:
                    var serverVersion = options.MySqlVersion != null
                        ? ServerVersion.Parse(options.MySqlVersion)
                        : ServerVersion.AutoDetect(options.ConnectionString);
                    builder.UseMySql(options.ConnectionString, serverVersion);
                    break;

                case RbacDbType.SqlServer:
                    builder.UseSqlServer(options.ConnectionString);
                    break;

                case RbacDbType.PostgreSql:
                    builder.UseNpgsql(options.ConnectionString);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(options.DbType), options.DbType, "不支持的数据库类型");
            }
        }
    }
}
