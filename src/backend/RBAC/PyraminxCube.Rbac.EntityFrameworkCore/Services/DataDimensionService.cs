using Microsoft.EntityFrameworkCore;
using PyraminxCube.Rbac.Core.Models.Dto;
using PyraminxCube.Rbac.Core.Services;
using PyraminxCube.Rbac.EntityFrameworkCore.Entities;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Services
{
    /// <summary>
    /// 数据维度服务实现
    /// </summary>
    public class DataDimensionService : IDataDimensionService
    {
        private readonly RbacDbContext _dbContext;

        public DataDimensionService(RbacDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region D-001 维度列表

        public async Task<PagedResult<DataDimensionListItem>> GetPagedListAsync(
            DataDimensionQueryParams queryParams,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.DataDimensions.AsQueryable();

            // 维度编码模糊匹配
            if (!string.IsNullOrWhiteSpace(queryParams.DimensionCode))
            {
                query = query.Where(d => d.DimensionCode.Contains(queryParams.DimensionCode));
            }

            // 维度名称模糊匹配
            if (!string.IsNullOrWhiteSpace(queryParams.DimensionName))
            {
                query = query.Where(d => d.DimensionName.Contains(queryParams.DimensionName));
            }

            // 获取总数
            var totalCount = await query.CountAsync(cancellationToken);

            // 分页查询
            var dimensions = await query
                .OrderBy(d => d.SortOrder)
                .ThenBy(d => d.DimensionName)
                .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(d => new DataDimensionListItem
                {
                    Id = d.Id,
                    DimensionCode = d.DimensionCode,
                    DimensionName = d.DimensionName,
                    Description = d.Description,
                    SortOrder = d.SortOrder,
                    CreatedTime = d.CreateTime
                })
                .ToListAsync(cancellationToken);

            // 获取每个维度的值数量（通过 DimensionCode 关联）
            var dimensionCodes = dimensions.Select(d => d.DimensionCode).ToList();
            var scopeCounts = await _dbContext.DataScopes
                .Where(ds => dimensionCodes.Contains(ds.DimensionCode))
                .GroupBy(ds => ds.DimensionCode)
                .Select(g => new { DimensionCode = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            foreach (var dimension in dimensions)
            {
                dimension.ScopeCount = scopeCounts.FirstOrDefault(sc => sc.DimensionCode == dimension.DimensionCode)?.Count ?? 0;
            }

            return new PagedResult<DataDimensionListItem>
            {
                Items = dimensions,
                TotalCount = totalCount,
                PageIndex = queryParams.PageIndex,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<List<DataDimensionListItem>> GetAllListAsync(
            CancellationToken cancellationToken = default)
        {
            var dimensions = await _dbContext.DataDimensions
                .OrderBy(d => d.SortOrder)
                .ThenBy(d => d.DimensionName)
                .Select(d => new DataDimensionListItem
                {
                    Id = d.Id,
                    DimensionCode = d.DimensionCode,
                    DimensionName = d.DimensionName,
                    Description = d.Description,
                    SortOrder = d.SortOrder,
                    CreatedTime = d.CreateTime
                })
                .ToListAsync(cancellationToken);

            // 获取每个维度的值数量（通过 DimensionCode 关联）
            var dimensionCodes = dimensions.Select(d => d.DimensionCode).ToList();
            var scopeCounts = await _dbContext.DataScopes
                .Where(ds => dimensionCodes.Contains(ds.DimensionCode))
                .GroupBy(ds => ds.DimensionCode)
                .Select(g => new { DimensionCode = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            foreach (var dimension in dimensions)
            {
                dimension.ScopeCount = scopeCounts.FirstOrDefault(sc => sc.DimensionCode == dimension.DimensionCode)?.Count ?? 0;
            }

            return dimensions;
        }

        public async Task<DataDimensionListItem?> GetByIdAsync(
            long dimensionId,
            CancellationToken cancellationToken = default)
        {
            var dimension = await _dbContext.DataDimensions
                .Where(d => d.Id == dimensionId)
                .Select(d => new DataDimensionListItem
                {
                    Id = d.Id,
                    DimensionCode = d.DimensionCode,
                    DimensionName = d.DimensionName,
                    Description = d.Description,
                    SortOrder = d.SortOrder,
                    CreatedTime = d.CreateTime
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (dimension != null)
            {
                dimension.ScopeCount = await _dbContext.DataScopes
                    .CountAsync(ds => ds.DimensionCode == dimension.DimensionCode, cancellationToken);
            }

            return dimension;
        }

        #endregion

        #region D-002 新增维度

        public async Task<long> CreateAsync(
            CreateDataDimensionRequest request,
            CancellationToken cancellationToken = default)
        {
            // 检查维度编码是否已存在
            if (await _dbContext.DataDimensions.AnyAsync(d => d.DimensionCode == request.DimensionCode, cancellationToken))
            {
                throw new InvalidOperationException($"维度编码 '{request.DimensionCode}' 已存在");
            }

            var dimension = new RbacDataDimension
            {
                DimensionCode = request.DimensionCode,
                DimensionName = request.DimensionName,
                Description = request.Description,
                SortOrder = request.SortOrder,
                CreateTime = DateTimeOffset.UtcNow,
                ModifyTime = DateTimeOffset.UtcNow
            };

            _dbContext.DataDimensions.Add(dimension);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return dimension.Id;
        }

        #endregion

        #region D-003 编辑维度

        public async Task UpdateAsync(
            UpdateDataDimensionRequest request,
            CancellationToken cancellationToken = default)
        {
            var dimension = await _dbContext.DataDimensions
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (dimension == null)
            {
                throw new InvalidOperationException($"维度不存在 (ID: {request.Id})");
            }

            dimension.DimensionName = request.DimensionName;
            dimension.Description = request.Description;
            dimension.SortOrder = request.SortOrder;
            dimension.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region D-004 删除维度

        public async Task<DimensionUsageCheckResult> DeleteAsync(
            long dimensionId,
            CancellationToken cancellationToken = default)
        {
            var usageResult = await CheckUsageAsync(dimensionId, cancellationToken);

            if (usageResult.IsInUse)
            {
                return usageResult;
            }

            var dimension = await _dbContext.DataDimensions
                .FirstOrDefaultAsync(d => d.Id == dimensionId, cancellationToken);

            if (dimension == null)
            {
                throw new InvalidOperationException($"维度不存在 (ID: {dimensionId})");
            }

            // 软删除维度
            dimension.IsDeleted = true;
            dimension.ModifyTime = DateTimeOffset.UtcNow;

            // 软删除所有维度值
            var scopes = await _dbContext.DataScopes
                .Where(ds => ds.DimensionCode == dimension.DimensionCode)
                .ToListAsync(cancellationToken);
            foreach (var scope in scopes)
            {
                scope.IsDeleted = true;
                scope.ModifyTime = DateTimeOffset.UtcNow;
            }

            // 软删除维度映射
            var mappings = await _dbContext.DataDimensionMappings
                .Where(m => m.DataDimensionId == dimensionId)
                .ToListAsync(cancellationToken);
            foreach (var mapping in mappings)
            {
                mapping.IsDeleted = true;
                mapping.ModifyTime = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return usageResult;
        }

        public async Task<DimensionUsageCheckResult> CheckUsageAsync(
            long dimensionId,
            CancellationToken cancellationToken = default)
        {
            var dimension = await _dbContext.DataDimensions
                .FirstOrDefaultAsync(d => d.Id == dimensionId, cancellationToken);

            if (dimension == null)
            {
                return new DimensionUsageCheckResult { IsInUse = false };
            }

            var result = new DimensionUsageCheckResult();

            // 检查是否有角色使用了该维度（通过维度ID）
            result.RoleCount = await _dbContext.RoleDataScopes
                .Where(rds => rds.DataDimensionId == dimensionId)
                .Select(rds => rds.RoleId)
                .Distinct()
                .CountAsync(cancellationToken);

            result.RoleCount += await _dbContext.RoleDataScopeFlags
                .Where(rdsf => rdsf.DataDimensionId == dimensionId)
                .Select(rdsf => rdsf.RoleId)
                .Distinct()
                .CountAsync(cancellationToken);

            // 获取维度值数量（通过 DimensionCode）
            result.ScopeCount = await _dbContext.DataScopes
                .CountAsync(ds => ds.DimensionCode == dimension.DimensionCode, cancellationToken);

            result.IsInUse = result.RoleCount > 0;

            if (result.IsInUse)
            {
                result.WarningMessage = $"该维度已被 {result.RoleCount} 个角色使用";
            }

            return result;
        }

        #endregion

        #region D-005 维度值列表

        public async Task<PagedResult<DataScopeListItem>> GetScopePagedListAsync(
            DataScopeQueryParams queryParams,
            CancellationToken cancellationToken = default)
        {
            // 获取维度信息
            var dimension = await _dbContext.DataDimensions
                .FirstOrDefaultAsync(d => d.Id == queryParams.DimensionId, cancellationToken);

            if (dimension == null)
            {
                throw new InvalidOperationException($"维度不存在 (ID: {queryParams.DimensionId})");
            }

            var query = _dbContext.DataScopes
                .Where(ds => ds.DimensionCode == dimension.DimensionCode);

            // 范围编码模糊匹配
            if (!string.IsNullOrWhiteSpace(queryParams.ScopeCode))
            {
                query = query.Where(ds => ds.DimensionCode.Contains(queryParams.ScopeCode));
            }

            // 范围名称模糊匹配
            if (!string.IsNullOrWhiteSpace(queryParams.ScopeName))
            {
                query = query.Where(ds => ds.ScopeName.Contains(queryParams.ScopeName));
            }

            // 获取总数
            var totalCount = await query.CountAsync(cancellationToken);

            // 分页查询
            var scopes = await query
                .OrderBy(ds => ds.SortOrder)
                .ThenBy(ds => ds.ScopeName)
                .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(ds => new DataScopeListItem
                {
                    Id = ds.Id,
                    DimensionId = queryParams.DimensionId,
                    ScopeCode = ds.DimensionCode,
                    ScopeName = ds.ScopeName,
                    ParentId = ds.ParentScopeId,
                    SortOrder = ds.SortOrder,
                    CreatedTime = ds.CreateTime
                })
                .ToListAsync(cancellationToken);

            // 获取父级名称
            var parentIds = scopes.Where(s => s.ParentId.HasValue).Select(s => s.ParentId!.Value).ToList();
            var parentNames = await _dbContext.DataScopes
                .Where(ds => parentIds.Contains(ds.Id))
                .Select(ds => new { ds.Id, ds.ScopeName })
                .ToListAsync(cancellationToken);

            foreach (var scope in scopes)
            {
                if (scope.ParentId.HasValue)
                {
                    scope.ParentName = parentNames.FirstOrDefault(pn => pn.Id == scope.ParentId.Value)?.ScopeName;
                }
            }

            return new PagedResult<DataScopeListItem>
            {
                Items = scopes,
                TotalCount = totalCount,
                PageIndex = queryParams.PageIndex,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<DataScopeListItem?> GetScopeByIdAsync(
            long scopeId,
            CancellationToken cancellationToken = default)
        {
            var scope = await _dbContext.DataScopes
                .Where(ds => ds.Id == scopeId)
                .Select(ds => new DataScopeListItem
                {
                    Id = ds.Id,
                    ScopeCode = ds.DimensionCode,
                    ScopeName = ds.ScopeName,
                    ParentId = ds.ParentScopeId,
                    SortOrder = ds.SortOrder,
                    CreatedTime = ds.CreateTime
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (scope == null) return null;

            // 获取维度ID
            var dimension = await _dbContext.DataDimensions
                .FirstOrDefaultAsync(d => d.DimensionCode == scope.ScopeCode, cancellationToken);
            if (dimension != null)
            {
                scope.DimensionId = dimension.Id;
            }

            if (scope.ParentId.HasValue)
            {
                scope.ParentName = await _dbContext.DataScopes
                    .Where(ds => ds.Id == scope.ParentId.Value)
                    .Select(ds => ds.ScopeName)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return scope;
        }

        #endregion

        #region D-006 新增维度值

        public async Task<long> CreateScopeAsync(
            CreateDataScopeRequest request,
            CancellationToken cancellationToken = default)
        {
            // 验证维度是否存在
            var dimension = await _dbContext.DataDimensions
                .FirstOrDefaultAsync(d => d.Id == request.DimensionId, cancellationToken);

            if (dimension == null)
            {
                throw new InvalidOperationException($"维度不存在 (ID: {request.DimensionId})");
            }

            // 检查同一维度下范围编码是否已存在（通过 DimensionCode）
            if (await _dbContext.DataScopes.AnyAsync(ds => ds.DimensionCode == dimension.DimensionCode && ds.ScopeId == long.Parse(request.ScopeCode), cancellationToken))
            {
                throw new InvalidOperationException($"范围值 '{request.ScopeCode}' 在该维度下已存在");
            }

            // 验证父级是否存在且在同一维度下
            if (request.ParentId.HasValue)
            {
                var parent = await _dbContext.DataScopes
                    .FirstOrDefaultAsync(ds => ds.Id == request.ParentId.Value && ds.DimensionCode == dimension.DimensionCode, cancellationToken);

                if (parent == null)
                {
                    throw new InvalidOperationException($"父级范围不存在或不属于同一维度");
                }
            }

            var scopeId = await GenerateScopeIdAsync(dimension.DimensionCode, cancellationToken);

            var scope = new RbacDataScope
            {
                TenantId = 0, // 数据维度值是全局的，使用 0 表示
                DimensionCode = dimension.DimensionCode,
                ScopeId = scopeId,
                ScopeName = request.ScopeName,
                ParentScopeId = request.ParentId,
                SortOrder = request.SortOrder,
                CreateTime = DateTimeOffset.UtcNow,
                ModifyTime = DateTimeOffset.UtcNow
            };

            _dbContext.DataScopes.Add(scope);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return scope.Id;
        }

        /// <summary>
        /// 生成 ScopeId（使用雪花算法或自增）
        /// </summary>
        private async Task<long> GenerateScopeIdAsync(string dimensionCode, CancellationToken cancellationToken)
        {
            // 简单实现：获取当前维度下最大的 ScopeId + 1
            var maxScopeId = await _dbContext.DataScopes
                .Where(ds => ds.DimensionCode == dimensionCode)
                .MaxAsync(ds => (long?)ds.ScopeId, cancellationToken) ?? 0;

            return maxScopeId + 1;
        }

        #endregion

        #region D-007 编辑维度值

        public async Task UpdateScopeAsync(
            UpdateDataScopeRequest request,
            CancellationToken cancellationToken = default)
        {
            var scope = await _dbContext.DataScopes
                .FirstOrDefaultAsync(ds => ds.Id == request.Id, cancellationToken);

            if (scope == null)
            {
                throw new InvalidOperationException($"范围值不存在 (ID: {request.Id})");
            }

            // 验证父级是否有效
            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == request.Id)
                {
                    throw new InvalidOperationException("不能将自己设置为父级");
                }

                var parent = await _dbContext.DataScopes
                    .FirstOrDefaultAsync(ds => ds.Id == request.ParentId.Value && ds.DimensionCode == scope.DimensionCode, cancellationToken);

                if (parent == null)
                {
                    throw new InvalidOperationException($"父级范围不存在或不属于同一维度");
                }

                // 检查是否会造成循环引用
                if (await IsDescendantOf(request.ParentId.Value, request.Id, cancellationToken))
                {
                    throw new InvalidOperationException("不能将子级设置为父级");
                }
            }

            scope.ScopeName = request.ScopeName;
            scope.ParentScopeId = request.ParentId;
            scope.SortOrder = request.SortOrder;
            scope.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 检查 targetId 是否是 ancestorId 的后代
        /// </summary>
        private async Task<bool> IsDescendantOf(long targetId, long ancestorId, CancellationToken cancellationToken)
        {
            var currentId = targetId;

            while (currentId != 0)
            {
                var scope = await _dbContext.DataScopes
                    .FirstOrDefaultAsync(ds => ds.Id == currentId, cancellationToken);

                if (scope == null) break;

                if (scope.ParentScopeId == ancestorId)
                {
                    return true;
                }

                currentId = scope.ParentScopeId ?? 0;
            }

            return false;
        }

        #endregion

        #region D-008 删除维度值

        public async Task<DataScopeUsageCheckResult> DeleteScopeAsync(
            long scopeId,
            CancellationToken cancellationToken = default)
        {
            var usageResult = await CheckScopeUsageAsync(scopeId, cancellationToken);

            if (usageResult.IsInUse)
            {
                return usageResult;
            }

            var scope = await _dbContext.DataScopes
                .FirstOrDefaultAsync(ds => ds.Id == scopeId, cancellationToken);

            if (scope == null)
            {
                throw new InvalidOperationException($"范围值不存在 (ID: {scopeId})");
            }

            // 软删除
            scope.IsDeleted = true;
            scope.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return usageResult;
        }

        public async Task<DataScopeUsageCheckResult> CheckScopeUsageAsync(
            long scopeId,
            CancellationToken cancellationToken = default)
        {
            var result = new DataScopeUsageCheckResult();

            // 检查是否有角色使用了该范围值
            result.RoleCount = await _dbContext.RoleDataScopes
                .Where(rds => rds.ScopeId == scopeId)
                .Select(rds => rds.RoleId)
                .Distinct()
                .CountAsync(cancellationToken);

            // 检查是否有子节点
            result.ChildCount = await _dbContext.DataScopes
                .CountAsync(ds => ds.ParentScopeId == scopeId, cancellationToken);

            result.IsInUse = result.RoleCount > 0 || result.ChildCount > 0;

            if (result.IsInUse)
            {
                var messages = new List<string>();
                if (result.RoleCount > 0)
                {
                    messages.Add($"被 {result.RoleCount} 个角色使用");
                }
                if (result.ChildCount > 0)
                {
                    messages.Add($"有 {result.ChildCount} 个子节点");
                }
                result.WarningMessage = string.Join("，", messages);
            }

            return result;
        }

        #endregion

        #region D-009 维度值树管理

        public async Task<List<DataScopeTreeNode>> GetScopeTreeAsync(
            long dimensionId,
            CancellationToken cancellationToken = default)
        {
            var dimension = await _dbContext.DataDimensions
                .FirstOrDefaultAsync(d => d.Id == dimensionId, cancellationToken);

            if (dimension == null)
            {
                throw new InvalidOperationException($"维度不存在 (ID: {dimensionId})");
            }

            var scopes = await _dbContext.DataScopes
                .Where(ds => ds.DimensionCode == dimension.DimensionCode)
                .OrderBy(ds => ds.SortOrder)
                .ThenBy(ds => ds.ScopeName)
                .Select(ds => new DataScopeTreeNode
                {
                    Id = ds.Id,
                    DimensionId = dimensionId,
                    ScopeCode = ds.DimensionCode,
                    ScopeName = ds.ScopeName,
                    ParentId = ds.ParentScopeId,
                    SortOrder = ds.SortOrder
                })
                .ToListAsync(cancellationToken);

            // 构建树形结构
            var lookup = scopes.ToLookup(s => s.ParentId);
            var roots = scopes.Where(s => !s.ParentId.HasValue).ToList();

            foreach (var root in roots)
            {
                BuildScopeTree(root, lookup);
            }

            return roots;
        }

        private static void BuildScopeTree(DataScopeTreeNode parent, ILookup<long?, DataScopeTreeNode> lookup)
        {
            parent.Children = lookup[parent.Id].ToList();
            foreach (var child in parent.Children)
            {
                BuildScopeTree(child, lookup);
            }
        }

        public async Task MoveScopeAsync(
            MoveDataScopeRequest request,
            CancellationToken cancellationToken = default)
        {
            var scope = await _dbContext.DataScopes
                .FirstOrDefaultAsync(ds => ds.Id == request.ScopeId, cancellationToken);

            if (scope == null)
            {
                throw new InvalidOperationException($"范围值不存在 (ID: {request.ScopeId})");
            }

            // 验证新的父级
            if (request.NewParentId.HasValue)
            {
                if (request.NewParentId.Value == request.ScopeId)
                {
                    throw new InvalidOperationException("不能将自己设置为父级");
                }

                var newParent = await _dbContext.DataScopes
                    .FirstOrDefaultAsync(ds => ds.Id == request.NewParentId.Value, cancellationToken);

                if (newParent == null)
                {
                    throw new InvalidOperationException($"新的父级不存在");
                }

                if (newParent.DimensionCode != scope.DimensionCode)
                {
                    throw new InvalidOperationException("父级必须在同一维度下");
                }

                // 检查是否会造成循环引用
                if (await IsDescendantOf(request.NewParentId.Value, request.ScopeId, cancellationToken))
                {
                    throw new InvalidOperationException("不能将子级设置为父级");
                }
            }

            scope.ParentScopeId = request.NewParentId;
            scope.SortOrder = request.NewSortOrder;
            scope.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateScopeSortOrdersAsync(
            long dimensionId,
            Dictionary<long, int> sortOrders,
            CancellationToken cancellationToken = default)
        {
            var dimension = await _dbContext.DataDimensions
                .FirstOrDefaultAsync(d => d.Id == dimensionId, cancellationToken);

            if (dimension == null) return;

            foreach (var (scopeId, sortOrder) in sortOrders)
            {
                var scope = await _dbContext.DataScopes
                    .FirstOrDefaultAsync(ds => ds.Id == scopeId && ds.DimensionCode == dimension.DimensionCode, cancellationToken);

                if (scope != null)
                {
                    scope.SortOrder = sortOrder;
                    scope.ModifyTime = DateTimeOffset.UtcNow;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region D-010 维度映射配置

        public async Task<List<DataDimensionMappingListItem>> GetMappingListAsync(
            DataDimensionMappingQueryParams queryParams,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.DataDimensionMappings
                .Where(m => m.DataDimensionId == queryParams.DimensionId);

            if (!string.IsNullOrWhiteSpace(queryParams.TableName))
            {
                query = query.Where(m => m.EntityTypeName.Contains(queryParams.TableName));
            }

            var mappings = await query
                .OrderBy(m => m.EntityTypeName)
                .ThenBy(m => m.PropertyName)
                .Join(_dbContext.DataDimensions,
                    m => m.DataDimensionId,
                    d => d.Id,
                    (m, d) => new DataDimensionMappingListItem
                    {
                        Id = m.Id,
                        DimensionId = m.DataDimensionId,
                        DimensionName = d.DimensionName,
                        TableName = m.EntityTypeName,
                        ColumnName = m.PropertyName,
                        CreatedTime = m.CreateTime
                    })
                .ToListAsync(cancellationToken);

            return mappings;
        }

        public async Task<long> CreateMappingAsync(
            CreateDataDimensionMappingRequest request,
            CancellationToken cancellationToken = default)
        {
            // 验证维度是否存在
            if (!await _dbContext.DataDimensions.AnyAsync(d => d.Id == request.DimensionId, cancellationToken))
            {
                throw new InvalidOperationException($"维度不存在 (ID: {request.DimensionId})");
            }

            // 检查是否已存在相同的映射
            if (await _dbContext.DataDimensionMappings.AnyAsync(m =>
                m.DataDimensionId == request.DimensionId &&
                m.EntityTypeName == request.TableName &&
                m.PropertyName == request.ColumnName, cancellationToken))
            {
                throw new InvalidOperationException($"映射 '{request.TableName}.{request.ColumnName}' 已存在");
            }

            var mapping = new RbacDataDimensionMapping
            {
                DataDimensionId = request.DimensionId,
                EntityTypeName = request.TableName,
                PropertyName = request.ColumnName,
                CreateTime = DateTimeOffset.UtcNow,
                ModifyTime = DateTimeOffset.UtcNow
            };

            _dbContext.DataDimensionMappings.Add(mapping);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return mapping.Id;
        }

        public async Task UpdateMappingAsync(
            UpdateDataDimensionMappingRequest request,
            CancellationToken cancellationToken = default)
        {
            var mapping = await _dbContext.DataDimensionMappings
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (mapping == null)
            {
                throw new InvalidOperationException($"映射不存在 (ID: {request.Id})");
            }

            mapping.EntityTypeName = request.TableName;
            mapping.PropertyName = request.ColumnName;
            mapping.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteMappingAsync(
            long mappingId,
            CancellationToken cancellationToken = default)
        {
            var mapping = await _dbContext.DataDimensionMappings
                .FirstOrDefaultAsync(m => m.Id == mappingId, cancellationToken);

            if (mapping == null)
            {
                throw new InvalidOperationException($"映射不存在 (ID: {mappingId})");
            }

            mapping.IsDeleted = true;
            mapping.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<DataDimensionMappingListItem?> GetMappingByIdAsync(
            long mappingId,
            CancellationToken cancellationToken = default)
        {
            var mapping = await _dbContext.DataDimensionMappings
                .Where(m => m.Id == mappingId)
                .Join(_dbContext.DataDimensions,
                    m => m.DataDimensionId,
                    d => d.Id,
                    (m, d) => new DataDimensionMappingListItem
                    {
                        Id = m.Id,
                        DimensionId = m.DataDimensionId,
                        DimensionName = d.DimensionName,
                        TableName = m.EntityTypeName,
                        ColumnName = m.PropertyName,
                        CreatedTime = m.CreateTime
                    })
                .FirstOrDefaultAsync(cancellationToken);

            return mapping;
        }

        #endregion
    }
}
