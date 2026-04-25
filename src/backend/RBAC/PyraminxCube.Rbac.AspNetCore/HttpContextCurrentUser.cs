using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PyraminxCube.Rbac.Core.Abstractions;

namespace PyraminxCube.Rbac.AspNetCore
{
    /// <summary>
    /// 基于 HttpContext 的当前用户实现
    /// </summary>
    public class HttpContextCurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ClaimsPrincipal? _principal;

        public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? Principal => _principal ??= _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

        public long? UserId
        {
            get
            {
                var userIdClaim = Principal?.FindFirst(ClaimTypes.NameIdentifier)
                    ?? Principal?.FindFirst("sub")
                    ?? Principal?.FindFirst("user_id");

                if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }
                return null;
            }
        }

        public long? TenantId
        {
            get
            {
                var tenantIdClaim = Principal?.FindFirst("tenant_id")
                    ?? Principal?.FindFirst("tid");

                if (tenantIdClaim != null && long.TryParse(tenantIdClaim.Value, out var tenantId))
                {
                    return tenantId;
                }
                // 默认租户ID为1（单租户模式）
                return 1;
            }
        }

        public string? UserName
        {
            get
            {
                return Principal?.FindFirst(ClaimTypes.Name)?.Value
                    ?? Principal?.FindFirst("name")?.Value
                    ?? Principal?.FindFirst("username")?.Value;
            }
        }

        public bool IsSuperAdmin
        {
            get
            {
                var superAdminClaim = Principal?.FindFirst("is_super_admin")
                    ?? Principal?.FindFirst("super_admin");

                if (superAdminClaim != null)
                {
                    return superAdminClaim.Value.Equals("true", StringComparison.OrdinalIgnoreCase)
                        || superAdminClaim.Value == "1";
                }

                // 也可以通过角色判断
                return Principal?.IsInRole("SuperAdmin") ?? false;
            }
        }

        public IReadOnlyList<long> RoleIds
        {
            get
            {
                var roleIdsClaim = Principal?.FindFirst("role_ids");
                if (roleIdsClaim != null && !string.IsNullOrEmpty(roleIdsClaim.Value))
                {
                    return roleIdsClaim.Value
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => long.TryParse(id.Trim(), out var roleId) ? roleId : 0)
                        .Where(id => id > 0)
                        .ToList();
                }
                return [];
            }
        }
    }
}
