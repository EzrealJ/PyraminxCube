using Microsoft.EntityFrameworkCore;
using PyraminxCube.Rbac.Core.Abstractions;
using PyraminxCube.Rbac.Core.Models;
using PyraminxCube.Rbac.EntityFrameworkCore.Entities;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Services
{
    /// <summary>
    /// 权限服务实现
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly RbacDbContext _dbContext;
        private readonly IPermissionCache _cache;

        public PermissionService(RbacDbContext dbContext, IPermissionCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<UserPermissions> GetUserPermissionsAsync(long userId, long tenantId, CancellationToken cancellationToken = default)
        {
            // 先从缓存获取
            var cached = await _cache.GetAsync(userId, tenantId, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            // 从数据库加载
            var permissions = await LoadUserPermissionsFromDbAsync(userId, tenantId, cancellationToken);

            // 写入缓存
            await _cache.SetAsync(permissions, null, cancellationToken);

            return permissions;
        }

        public async Task<bool> HasApiPermissionAsync(long userId, long tenantId, string apiCode, CancellationToken cancellationToken = default)
        {
            var permissions = await GetUserPermissionsAsync(userId, tenantId, cancellationToken);
            return permissions.ApiPermissions.Contains(apiCode);
        }

        public async Task<bool> HasFeaturePermissionAsync(long userId, long tenantId, string featureCode, CancellationToken cancellationToken = default)
        {
            var permissions = await GetUserPermissionsAsync(userId, tenantId, cancellationToken);
            return permissions.FeaturePermissions.Contains(featureCode);
        }

        public async Task<string?> GetApiCodeByEndpointAsync(string endpoint, string httpMethod, CancellationToken cancellationToken = default)
        {
            // 移除查询字符串和尾部斜杠
            var normalizedEndpoint = endpoint.Split('?')[0].TrimEnd('/');

            var api = await _dbContext.ApiPermissions
                .Where(a => a.Endpoint == normalizedEndpoint && a.HttpMethod == httpMethod.ToUpper())
                .FirstOrDefaultAsync(cancellationToken);

            return api?.ApiCode;
        }

        public async Task<List<FeaturePermissionInfo>> GetUserFeatureTreeAsync(long userId, long tenantId, CancellationToken cancellationToken = default)
        {
            var permissions = await GetUserPermissionsAsync(userId, tenantId, cancellationToken);

            // 获取用户有权限的功能
            var featureCodes = permissions.FeaturePermissions;

            var features = await _dbContext.FeaturePermissions
                .Where(f => featureCodes.Contains(f.FeatureCode))
                .OrderBy(f => f.SortOrder)
                .Select(f => new FeaturePermissionInfo
                {
                    FeatureCode = f.FeatureCode,
                    FeatureName = f.FeatureName,
                    FeatureType = f.FeatureType,
                    ParentCode = f.ParentCode,
                    Path = f.Path,
                    Icon = f.Icon,
                    SortOrder = f.SortOrder
                })
                .ToListAsync(cancellationToken);

            // 构建树形结构
            return BuildFeatureTree(features);
        }

        public async Task RefreshUserPermissionCacheAsync(long userId, long tenantId, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(userId, tenantId, cancellationToken);
        }

        public async Task RefreshTenantPermissionCacheAsync(long tenantId, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveByTenantAsync(tenantId, cancellationToken);
        }

        /// <summary>
        /// 从数据库加载用户权限
        /// </summary>
        private async Task<UserPermissions> LoadUserPermissionsFromDbAsync(long userId, long tenantId, CancellationToken cancellationToken)
        {
            var permissions = new UserPermissions
            {
                UserId = userId,
                TenantId = tenantId
            };

            // 获取用户的所有角色ID
            var roleIds = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken);

            if (roleIds.Count == 0)
            {
                return permissions;
            }

            // 获取所有角色的 API 权限（并集）
            var apiCodes = await _dbContext.RoleApiPermissions
                .Where(rap => roleIds.Contains(rap.RoleId) && rap.TenantId == tenantId)
                .Join(_dbContext.ApiPermissions,
                    rap => rap.ApiPermissionId,
                    ap => ap.Id,
                    (rap, ap) => ap.ApiCode)
                .Distinct()
                .ToListAsync(cancellationToken);

            permissions.ApiPermissions = [.. apiCodes];

            // 获取所有角色的功能权限（并集）
            var featureCodes = await _dbContext.RoleFeaturePermissions
                .Where(rfp => roleIds.Contains(rfp.RoleId) && rfp.TenantId == tenantId)
                .Join(_dbContext.FeaturePermissions,
                    rfp => rfp.FeaturePermissionId,
                    fp => fp.Id,
                    (rfp, fp) => fp.FeatureCode)
                .Distinct()
                .ToListAsync(cancellationToken);

            permissions.FeaturePermissions = [.. featureCodes];

            // 获取数据权限
            permissions.DataScopes = await LoadDataScopesAsync(roleIds, tenantId, cancellationToken);

            return permissions;
        }

        /// <summary>
        /// 加载数据权限（多角色合并逻辑）
        /// </summary>
        private async Task<Dictionary<string, DataScopeValue>> LoadDataScopesAsync(
            List<long> roleIds,
            long tenantId,
            CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, DataScopeValue>();

            // 获取所有维度
            var dimensions = await _dbContext.DataDimensions
                .Select(d => d.DimensionCode)
                .ToListAsync(cancellationToken);

            foreach (var dimensionCode in dimensions)
            {
                var scopeValue = await MergeRoleDataScopesAsync(roleIds, dimensionCode, tenantId, cancellationToken);
                if (scopeValue != null)
                {
                    result[dimensionCode] = scopeValue;
                }
            }

            return result;
        }

        /// <summary>
        /// 合并多个角色的数据权限（文档规则：并集合并，但查询时取交集）
        /// </summary>
        private async Task<DataScopeValue?> MergeRoleDataScopesAsync(
            List<long> roleIds,
            string dimensionCode,
            long tenantId,
            CancellationToken cancellationToken)
        {
            var dimension = await _dbContext.DataDimensions
                .FirstOrDefaultAsync(d => d.DimensionCode == dimensionCode, cancellationToken);

            if (dimension == null)
            {
                return null;
            }

            // 检查是否有 ALL 标记（任意角色有 ALL 则最终为 ALL）
            var hasAllFlag = await _dbContext.RoleDataScopeFlags
                .AnyAsync(f => roleIds.Contains(f.RoleId)
                    && f.DataDimensionId == dimension.Id
                    && f.TenantId == tenantId
                    && f.Flag == DataScopeFlag.All, cancellationToken);

            if (hasAllFlag)
            {
                return new DataScopeValue { Flag = DataScopeFlag.All };
            }

            // 检查是否有 SELF 标记
            var hasSelfFlag = await _dbContext.RoleDataScopeFlags
                .AnyAsync(f => roleIds.Contains(f.RoleId)
                    && f.DataDimensionId == dimension.Id
                    && f.TenantId == tenantId
                    && f.Flag == DataScopeFlag.Self, cancellationToken);

            // 获取所有具体的 ScopeIds（并集）
            var scopeIds = await _dbContext.RoleDataScopes
                .Where(rds => roleIds.Contains(rds.RoleId)
                    && rds.DataDimensionId == dimension.Id
                    && rds.TenantId == tenantId)
                .Select(rds => rds.ScopeId)
                .Distinct()
                .ToListAsync(cancellationToken);

            // 如果有 SELF 且无具体 ScopeIds，则返回 SELF
            if (hasSelfFlag && scopeIds.Count == 0)
            {
                return new DataScopeValue { Flag = DataScopeFlag.Self };
            }

            // 否则返回 Custom，包含具体的 ScopeIds
            // 注：如果同时有 SELF 标记，查询时需要在业务层处理
            return new DataScopeValue
            {
                Flag = DataScopeFlag.Custom,
                ScopeIds = [.. scopeIds]
            };
        }

        /// <summary>
        /// 构建功能树
        /// </summary>
        private static List<FeaturePermissionInfo> BuildFeatureTree(List<FeaturePermissionInfo> features)
        {
            var lookup = features.ToLookup(f => f.ParentCode);
            var roots = features.Where(f => string.IsNullOrEmpty(f.ParentCode)).ToList();

            foreach (var root in roots)
            {
                BuildChildren(root, lookup);
            }

            return roots;
        }

        private static void BuildChildren(FeaturePermissionInfo parent, ILookup<string?, FeaturePermissionInfo> lookup)
        {
            parent.Children = lookup[parent.FeatureCode].ToList();
            foreach (var child in parent.Children)
            {
                BuildChildren(child, lookup);
            }
        }
    }
}
