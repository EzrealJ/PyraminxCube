using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PyraminxCube.Rbac.Core.Abstractions;

namespace PyraminxCube.Rbac.AspNetCore.Authorization
{
    /// <summary>
    /// API权限校验处理器
    /// </summary>
    public class ApiPermissionHandler : AuthorizationHandler<ApiPermissionRequirement>
    {
        private readonly IPermissionService _permissionService;
        private readonly ICurrentUser _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiPermissionHandler> _logger;

        public ApiPermissionHandler(
            IPermissionService permissionService,
            ICurrentUser currentUser,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApiPermissionHandler> logger)
        {
            _permissionService = permissionService;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ApiPermissionRequirement requirement)
        {
            // 未认证用户直接拒绝
            if (!_currentUser.IsAuthenticated)
            {
                _logger.LogDebug("User is not authenticated, denying access");
                context.Fail();
                return;
            }

            // 超级管理员跳过权限校验
            if (_currentUser.IsSuperAdmin)
            {
                _logger.LogDebug("User {UserId} is super admin, granting access", _currentUser.UserId);
                context.Succeed(requirement);
                return;
            }

            var userId = _currentUser.UserId!.Value;
            var tenantId = _currentUser.TenantId ?? 0;

            // 获取要校验的 API 编码
            var apiCode = await GetApiCodeAsync();
            if (string.IsNullOrEmpty(apiCode))
            {
                _logger.LogWarning("Cannot determine API code for the current request");
                context.Fail();
                return;
            }

            // 校验权限
            var hasPermission = await _permissionService.HasApiPermissionAsync(userId, tenantId, apiCode);
            if (hasPermission)
            {
                _logger.LogDebug("User {UserId} has permission {ApiCode}", userId, apiCode);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogDebug("User {UserId} does not have permission {ApiCode}", userId, apiCode);
                context.Fail();
            }
        }

        /// <summary>
        /// 获取当前请求的 API 编码
        /// </summary>
        private async Task<string?> GetApiCodeAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            // 优先从 Endpoint 元数据获取显式指定的 ApiCode
            var endpoint = httpContext.GetEndpoint();
            var apiPermissionAttribute = endpoint?.Metadata.GetMetadata<ApiPermissionAttribute>();

            if (apiPermissionAttribute != null && !string.IsNullOrEmpty(apiPermissionAttribute.ApiCode))
            {
                return apiPermissionAttribute.ApiCode;
            }

            // 如果是自动推断模式，根据路由和HTTP方法查询
            if (apiPermissionAttribute?.AutoInfer == true)
            {
                var path = httpContext.Request.Path.Value ?? string.Empty;
                var method = httpContext.Request.Method;

                return await _permissionService.GetApiCodeByEndpointAsync(path, method);
            }

            return null;
        }
    }

    /// <summary>
    /// API权限要求
    /// </summary>
    public class ApiPermissionRequirement : IAuthorizationRequirement
    {
    }
}
