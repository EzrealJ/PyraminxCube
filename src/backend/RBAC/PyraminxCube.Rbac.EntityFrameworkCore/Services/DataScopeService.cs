using PyraminxCube.Rbac.Core.Abstractions;
using PyraminxCube.Rbac.Core.Models;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Services
{
    /// <summary>
    /// 数据权限服务实现
    /// </summary>
    public class DataScopeService : IDataScopeService
    {
        private readonly ICurrentUser _currentUser;
        private readonly IPermissionService _permissionService;
        private readonly IDataDimensionMapping _dimensionMapping;

        // 缓存当前用户的权限（请求范围内）
        private UserPermissions? _cachedPermissions;

        public DataScopeService(
            ICurrentUser currentUser,
            IPermissionService permissionService,
            IDataDimensionMapping dimensionMapping)
        {
            _currentUser = currentUser;
            _permissionService = permissionService;
            _dimensionMapping = dimensionMapping;
        }

        public DataScopeValue GetCurrentUserDataScope(string dimensionCode)
        {
            var scopes = GetCurrentUserDataScopes();

            if (scopes.TryGetValue(dimensionCode, out var scopeValue))
            {
                return scopeValue;
            }

            // 默认返回空（无权限）
            return new DataScopeValue
            {
                Flag = DataScopeFlag.Custom,
                ScopeIds = []
            };
        }

        public Dictionary<string, DataScopeValue> GetCurrentUserDataScopes()
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return [];
            }

            // 从缓存或权限服务获取
            var permissions = GetCurrentUserPermissions();
            return permissions?.DataScopes ?? [];
        }

        public string? GetDimensionPropertyName<TEntity>(string dimensionCode) where TEntity : class
        {
            var mappings = _dimensionMapping.GetMappings(typeof(TEntity));
            return mappings.TryGetValue(dimensionCode, out var propertyName) ? propertyName : null;
        }

        public Dictionary<string, string> GetEntityDimensionMappings<TEntity>() where TEntity : class
        {
            return _dimensionMapping.GetMappings(typeof(TEntity));
        }

        /// <summary>
        /// 获取当前用户权限（请求范围内缓存）
        /// </summary>
        private UserPermissions? GetCurrentUserPermissions()
        {
            if (_cachedPermissions != null)
            {
                return _cachedPermissions;
            }

            if (!_currentUser.UserId.HasValue)
            {
                return null;
            }

            var userId = _currentUser.UserId.Value;
            var tenantId = _currentUser.TenantId ?? 1;

            // 同步调用异步方法（在请求范围内应该已经有缓存）
            _cachedPermissions = _permissionService
                .GetUserPermissionsAsync(userId, tenantId)
                .GetAwaiter()
                .GetResult();

            return _cachedPermissions;
        }
    }
}
