using Microsoft.EntityFrameworkCore;
using PyraminxCube.Rbac.Core.Models;
using PyraminxCube.Rbac.Core.Services;
using PyraminxCube.Rbac.EntityFrameworkCore.Entities;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Services
{
    /// <summary>
    /// 功能权限服务实现
    /// </summary>
    public class FeaturePermissionService : IFeaturePermissionService
    {
        private readonly RbacDbContext _dbContext;

        public FeaturePermissionService(RbacDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region F-001 功能权限树

        public async Task<List<FeaturePermissionTreeNode>> GetTreeAsync(
            CancellationToken cancellationToken = default)
        {
            var features = await _dbContext.FeaturePermissions
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.FeatureName)
                .Select(f => new FeaturePermissionTreeNode
                {
                    Id = f.Id,
                    FeatureCode = f.FeatureCode,
                    FeatureName = f.FeatureName,
                    FeatureType = f.FeatureType,
                    Path = f.Path,
                    Icon = f.Icon,
                    SortOrder = f.SortOrder,
                    Status = 1 // 默认启用，如果实体有 Status 字段则使用
                })
                .ToListAsync(cancellationToken);

            // 获取所有功能按钮关联的API
            var featureApiMappings = await _dbContext.FeatureApiMappings
                .Join(_dbContext.ApiPermissions,
                    fam => fam.ApiPermissionId,
                    ap => ap.Id,
                    (fam, ap) => new { fam.FeaturePermissionId, ap.ApiCode })
                .ToListAsync(cancellationToken);

            // 组装API编码
            foreach (var feature in features)
            {
                feature.ApiCodes = featureApiMappings
                    .Where(fam => fam.FeaturePermissionId == feature.Id)
                    .Select(fam => fam.ApiCode)
                    .ToList();
            }

            // 构建树形结构
            return BuildTree(features);
        }

        public async Task<List<FeaturePermissionTreeNode>> GetEnabledTreeAsync(
            CancellationToken cancellationToken = default)
        {
            // 当前实体没有 Status 字段，返回所有
            return await GetTreeAsync(cancellationToken);
        }

        public async Task<List<FeaturePermissionSimpleInfo>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var features = await _dbContext.FeaturePermissions
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.FeatureName)
                .Select(f => new FeaturePermissionSimpleInfo
                {
                    Id = f.Id,
                    FeatureCode = f.FeatureCode,
                    FeatureName = f.FeatureName,
                    FeatureType = f.FeatureType,
                    ParentId = null // 需要通过 ParentCode 查询
                })
                .ToListAsync(cancellationToken);

            // 构建路径映射
            var codeToId = features.ToDictionary(f => f.FeatureCode, f => f.Id);
            var codeToName = features.ToDictionary(f => f.FeatureCode, f => f.FeatureName);

            // 获取 ParentCode 信息
            var featureCodes = features.Select(f => f.FeatureCode).ToList();
            var parentInfo = await _dbContext.FeaturePermissions
                .Where(f => featureCodes.Contains(f.FeatureCode))
                .Select(f => new { f.FeatureCode, f.ParentCode })
                .ToListAsync(cancellationToken);

            var parentCodeDict = parentInfo.ToDictionary(f => f.FeatureCode, f => f.ParentCode);

            // 设置 ParentId 和计算路径
            foreach (var feature in features)
            {
                if (parentCodeDict.TryGetValue(feature.FeatureCode, out var parentCode) && !string.IsNullOrEmpty(parentCode))
                {
                    feature.ParentId = codeToId.GetValueOrDefault(parentCode);
                }
            }

            // 计算完整路径
            foreach (var feature in features)
            {
                feature.FullPath = BuildFullPath(feature.FeatureCode, codeToName, parentCodeDict);
                feature.Level = CalculateLevel(feature.FeatureCode, parentCodeDict);
            }

            return features;
        }

        #endregion

        #region F-002 新增功能节点

        public async Task<long> CreateAsync(
            CreateFeaturePermissionRequest request,
            CancellationToken cancellationToken = default)
        {
            // 检查功能编码是否已存在
            if (await _dbContext.FeaturePermissions.AnyAsync(f => f.FeatureCode == request.FeatureCode, cancellationToken))
            {
                throw new InvalidOperationException($"功能编码 '{request.FeatureCode}' 已存在");
            }

            // 验证父级是否存在
            string? parentCode = null;
            if (request.ParentId.HasValue)
            {
                var parent = await _dbContext.FeaturePermissions
                    .FirstOrDefaultAsync(f => f.Id == request.ParentId.Value, cancellationToken);

                if (parent == null)
                {
                    throw new InvalidOperationException($"父级功能不存在 (ID: {request.ParentId.Value})");
                }

                parentCode = parent.FeatureCode;

                // 验证类型层级：模块 -> 页面 -> 按钮
                if (parent.FeatureType == FeatureType.Button)
                {
                    throw new InvalidOperationException("按钮类型不能作为父级");
                }

                if (request.FeatureType == FeatureType.Module && parent.FeatureType != FeatureType.Module)
                {
                    throw new InvalidOperationException("模块只能作为模块的子级");
                }

                if (request.FeatureType == FeatureType.Page && parent.FeatureType == FeatureType.Button)
                {
                    throw new InvalidOperationException("页面不能作为按钮的子级");
                }
            }
            else
            {
                // 根节点必须是模块
                if (request.FeatureType != FeatureType.Module)
                {
                    throw new InvalidOperationException("根节点必须是模块类型");
                }
            }

            var feature = new RbacFeaturePermission
            {
                FeatureCode = request.FeatureCode,
                FeatureName = request.FeatureName,
                FeatureType = request.FeatureType,
                ParentCode = parentCode,
                Path = request.Path,
                Icon = request.Icon,
                SortOrder = request.SortOrder,
                Description = request.Description,
                CreateTime = DateTimeOffset.UtcNow,
                ModifyTime = DateTimeOffset.UtcNow
            };

            _dbContext.FeaturePermissions.Add(feature);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return feature.Id;
        }

        #endregion

        #region F-003 编辑功能节点

        public async Task UpdateAsync(
            UpdateFeaturePermissionRequest request,
            CancellationToken cancellationToken = default)
        {
            var feature = await _dbContext.FeaturePermissions
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

            if (feature == null)
            {
                throw new InvalidOperationException($"功能节点不存在 (ID: {request.Id})");
            }

            // 更新功能信息
            feature.FeatureName = request.FeatureName;
            feature.Path = request.Path;
            feature.Icon = request.Icon;
            feature.SortOrder = request.SortOrder;
            feature.Description = request.Description;
            feature.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region F-004 删除功能节点

        public async Task DeleteAsync(
            long featureId,
            CancellationToken cancellationToken = default)
        {
            var (canDelete, childCount) = await CheckCanDeleteAsync(featureId, cancellationToken);

            if (!canDelete)
            {
                throw new InvalidOperationException($"该功能节点下有 {childCount} 个子节点，无法删除");
            }

            var feature = await _dbContext.FeaturePermissions
                .FirstOrDefaultAsync(f => f.Id == featureId, cancellationToken);

            if (feature == null)
            {
                throw new InvalidOperationException($"功能节点不存在 (ID: {featureId})");
            }

            // 软删除功能节点
            feature.IsDeleted = true;
            feature.ModifyTime = DateTimeOffset.UtcNow;

            // 软删除API关联
            var apiMappings = await _dbContext.FeatureApiMappings
                .Where(fam => fam.FeaturePermissionId == featureId)
                .ToListAsync(cancellationToken);
            foreach (var mapping in apiMappings)
            {
                mapping.IsDeleted = true;
                mapping.ModifyTime = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<(bool CanDelete, int ChildCount)> CheckCanDeleteAsync(
            long featureId,
            CancellationToken cancellationToken = default)
        {
            var feature = await _dbContext.FeaturePermissions
                .FirstOrDefaultAsync(f => f.Id == featureId, cancellationToken);

            if (feature == null)
            {
                return (false, 0);
            }

            var childCount = await _dbContext.FeaturePermissions
                .Where(f => f.ParentCode == feature.FeatureCode)
                .CountAsync(cancellationToken);

            return (childCount == 0, childCount);
        }

        #endregion

        #region F-005 调整节点顺序

        public async Task UpdateSortOrderAsync(
            UpdateFeatureSortOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var feature = await _dbContext.FeaturePermissions
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

            if (feature == null)
            {
                throw new InvalidOperationException($"功能节点不存在 (ID: {request.Id})");
            }

            // 如果指定了新的父级
            if (request.ParentId.HasValue)
            {
                var newParent = await _dbContext.FeaturePermissions
                    .FirstOrDefaultAsync(f => f.Id == request.ParentId.Value, cancellationToken);

                if (newParent == null)
                {
                    throw new InvalidOperationException($"父级功能不存在 (ID: {request.ParentId.Value})");
                }

                // 验证不能移动到自己或自己的子级下
                if (newParent.FeatureCode == feature.FeatureCode ||
                    await IsChildOf(newParent.FeatureCode, feature.FeatureCode))
                {
                    throw new InvalidOperationException("不能移动到自己或自己的子级下");
                }

                feature.ParentCode = newParent.FeatureCode;
            }

            feature.SortOrder = request.SortOrder;
            feature.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateSortOrderBatchAsync(
            List<UpdateFeatureSortOrderRequest> requests,
            CancellationToken cancellationToken = default)
        {
            foreach (var request in requests)
            {
                await UpdateSortOrderAsync(request, cancellationToken);
            }
        }

        #endregion

        #region F-006 关联API

        public async Task AssignApiAsync(
            AssignApiToFeatureRequest request,
            CancellationToken cancellationToken = default)
        {
            // 验证功能是否存在且为按钮类型
            var feature = await _dbContext.FeaturePermissions
                .FirstOrDefaultAsync(f => f.Id == request.FeatureId, cancellationToken);

            if (feature == null)
            {
                throw new InvalidOperationException($"功能节点不存在 (ID: {request.FeatureId})");
            }

            if (feature.FeatureType != FeatureType.Button)
            {
                throw new InvalidOperationException("只有按钮类型才能关联API");
            }

            // 验证API是否存在
            var api = await _dbContext.ApiPermissions
                .FirstOrDefaultAsync(ap => ap.Id == request.ApiId, cancellationToken);

            if (api == null)
            {
                throw new InvalidOperationException($"API不存在 (ID: {request.ApiId})");
            }

            // 检查是否已关联
            if (await _dbContext.FeatureApiMappings.AnyAsync(fam => fam.FeaturePermissionId == request.FeatureId && fam.ApiPermissionId == request.ApiId, cancellationToken))
            {
                return; // 已存在，不需要重复添加
            }

            var mapping = new RbacFeatureApiMapping
            {
                FeaturePermissionId = request.FeatureId,
                ApiPermissionId = request.ApiId,
                CreateTime = DateTimeOffset.UtcNow,
                ModifyTime = DateTimeOffset.UtcNow
            };

            _dbContext.FeatureApiMappings.Add(mapping);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveApiAsync(
            long featureId,
            long apiId,
            CancellationToken cancellationToken = default)
        {
            var mapping = await _dbContext.FeatureApiMappings
                .FirstOrDefaultAsync(fam => fam.FeaturePermissionId == featureId && fam.ApiPermissionId == apiId, cancellationToken);

            if (mapping != null)
            {
                mapping.IsDeleted = true;
                mapping.ModifyTime = DateTimeOffset.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<ApiPermissionSimpleInfo>> GetAssignedApisAsync(
            long featureId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.FeatureApiMappings
                .Where(fam => fam.FeaturePermissionId == featureId)
                .Join(_dbContext.ApiPermissions,
                    fam => fam.ApiPermissionId,
                    ap => ap.Id,
                    (fam, ap) => new ApiPermissionSimpleInfo
                    {
                        Id = ap.Id,
                        ApiCode = ap.ApiCode,
                        ApiName = ap.ApiName,
                        Endpoint = ap.Endpoint,
                        HttpMethod = ap.HttpMethod
                    })
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region F-007 批量关联API

        public async Task AssignBatchApisAsync(
            BatchAssignApisToFeaturesRequest request,
            CancellationToken cancellationToken = default)
        {
            // 验证功能是否存在且为按钮类型
            var feature = await _dbContext.FeaturePermissions
                .FirstOrDefaultAsync(f => f.Id == request.FeatureId, cancellationToken);

            if (feature == null)
            {
                throw new InvalidOperationException($"功能节点不存在 (ID: {request.FeatureId})");
            }

            if (feature.FeatureType != FeatureType.Button)
            {
                throw new InvalidOperationException("只有按钮类型才能关联API");
            }

            if (request.IsReplace)
            {
                // 替换模式：先删除所有现有关联
                var existingMappings = await _dbContext.FeatureApiMappings
                    .Where(fam => fam.FeaturePermissionId == request.FeatureId)
                    .ToListAsync(cancellationToken);

                foreach (var mapping in existingMappings)
                {
                    mapping.IsDeleted = true;
                    mapping.ModifyTime = DateTimeOffset.UtcNow;
                }
            }

            // 添加新的关联
            foreach (var apiId in request.ApiIds)
            {
                // 验证API是否存在
                if (!await _dbContext.ApiPermissions.AnyAsync(ap => ap.Id == apiId, cancellationToken))
                {
                    continue; // 跳过不存在的API
                }

                // 检查是否已存在（追加模式下需要检查）
                var exists = await _dbContext.FeatureApiMappings
                    .AnyAsync(fam => fam.FeaturePermissionId == request.FeatureId && fam.ApiPermissionId == apiId, cancellationToken);

                if (!exists)
                {
                    var mapping = new RbacFeatureApiMapping
                    {
                        FeaturePermissionId = request.FeatureId,
                        ApiPermissionId = apiId,
                        CreateTime = DateTimeOffset.UtcNow,
                        ModifyTime = DateTimeOffset.UtcNow
                    };
                    _dbContext.FeatureApiMappings.Add(mapping);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ClearAllApisAsync(
            long featureId,
            CancellationToken cancellationToken = default)
        {
            var mappings = await _dbContext.FeatureApiMappings
                .Where(fam => fam.FeaturePermissionId == featureId)
                .ToListAsync(cancellationToken);

            foreach (var mapping in mappings)
            {
                mapping.IsDeleted = true;
                mapping.ModifyTime = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 构建树形结构
        /// </summary>
        private List<FeaturePermissionTreeNode> BuildTree(List<FeaturePermissionTreeNode> features)
        {
            // 获取所有 ParentCode 映射
            var codeToFeature = features.ToDictionary(f => f.FeatureCode);
            var featureCodes = features.Select(f => f.FeatureCode).ToList();

            // 需要从数据库获取 ParentCode
            // 这里简化处理，使用 ParentId 方式构建
            var result = new List<FeaturePermissionTreeNode>();

            // 找出根节点（没有 ParentCode 或 ParentCode 为空的）
            // 由于我们使用 ParentCode 而不是 ParentId，需要额外查询
            // 暂时返回平铺结构，树形结构需要改进
            return features;
        }

        /// <summary>
        /// 构建完整路径
        /// </summary>
        private static string BuildFullPath(
            string featureCode,
            Dictionary<string, string> codeToName,
            Dictionary<string, string?> parentCodeDict)
        {
            var pathParts = new List<string>();
            var currentCode = featureCode;

            while (!string.IsNullOrEmpty(currentCode))
            {
                if (codeToName.TryGetValue(currentCode, out var name))
                {
                    pathParts.Insert(0, name);
                }

                currentCode = parentCodeDict.GetValueOrDefault(currentCode) ?? string.Empty;
            }

            return string.Join(" / ", pathParts);
        }

        /// <summary>
        /// 计算层级深度
        /// </summary>
        private static int CalculateLevel(string featureCode, Dictionary<string, string?> parentCodeDict)
        {
            var level = 1;
            var currentCode = parentCodeDict.GetValueOrDefault(featureCode);

            while (!string.IsNullOrEmpty(currentCode))
            {
                level++;
                currentCode = parentCodeDict.GetValueOrDefault(currentCode) ?? string.Empty;
            }

            return level;
        }

        /// <summary>
        /// 检查 targetCode 是否是 parentCode 的子级
        /// </summary>
        private async Task<bool> IsChildOf(string targetCode, string parentCode)
        {
            var currentCode = targetCode;

            while (!string.IsNullOrEmpty(currentCode))
            {
                var feature = await _dbContext.FeaturePermissions
                    .FirstOrDefaultAsync(f => f.FeatureCode == currentCode);

                if (feature == null) break;

                if (feature.ParentCode == parentCode)
                {
                    return true;
                }

                currentCode = feature.ParentCode ?? string.Empty;
            }

            return false;
        }

        #endregion
    }
}
