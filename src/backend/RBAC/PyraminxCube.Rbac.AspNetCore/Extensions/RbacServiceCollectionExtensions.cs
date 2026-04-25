using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PyraminxCube.Rbac.AspNetCore.Authorization;
using PyraminxCube.Rbac.Core.Abstractions;

namespace PyraminxCube.Rbac.AspNetCore.Extensions
{
    /// <summary>
    /// RBAC ASP.NET Core 服务注册扩展
    /// </summary>
    public static class RbacServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 RBAC ASP.NET Core 服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddRbacAspNetCore(this IServiceCollection services)
        {
            // 注册 HttpContextAccessor
            services.AddHttpContextAccessor();

            // 注册当前用户
            services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

            // 注册授权处理器
            services.AddScoped<IAuthorizationHandler, ApiPermissionHandler>();

            // 配置授权策略
            services.AddAuthorization(options =>
            {
                options.AddPolicy(ApiPermissionDefaults.PolicyName, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new ApiPermissionRequirement());
                });
            });

            return services;
        }

        /// <summary>
        /// 添加 RBAC ASP.NET Core 服务（带自定义配置）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">配置委托</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddRbacAspNetCore(
            this IServiceCollection services,
            Action<RbacAspNetCoreOptions> configure)
        {
            var options = new RbacAspNetCoreOptions();
            configure(options);

            services.AddRbacAspNetCore();

            // 应用自定义配置
            if (options.CurrentUserFactory != null)
            {
                services.AddScoped(options.CurrentUserFactory);
            }

            return services;
        }
    }

    /// <summary>
    /// RBAC ASP.NET Core 配置选项
    /// </summary>
    public class RbacAspNetCoreOptions
    {
        /// <summary>
        /// 自定义当前用户工厂
        /// </summary>
        public Func<IServiceProvider, ICurrentUser>? CurrentUserFactory { get; set; }
    }
}
