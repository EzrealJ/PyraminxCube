using Microsoft.EntityFrameworkCore;
using PyraminxCube.Rbac.Core.Models.Dto;
using PyraminxCube.Rbac.Core.Services;
using PyraminxCube.Rbac.EntityFrameworkCore.Entities;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Services
{
    /// <summary>
    /// API权限服务实现
    /// </summary>
    public class ApiPermissionService : IApiPermissionService
    {
        private readonly RbacDbContext _dbContext;

        public ApiPermissionService(RbacDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region A-001 API列表

        public async Task<PagedResult<ApiPermissionListItem>> GetPagedListAsync(
            ApiPermissionQueryParams queryParams,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.ApiPermissions.AsQueryable();

            // 关键字模糊匹配（名称或路径）
            if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
            {
                query = query.Where(ap => ap.ApiName.Contains(queryParams.Keyword) || ap.Endpoint.Contains(queryParams.Keyword));
            }

            // HTTP方法过滤
            if (!string.IsNullOrWhiteSpace(queryParams.HttpMethod))
            {
                query = query.Where(ap => ap.HttpMethod == queryParams.HttpMethod);
            }

            // 分组过滤
            if (queryParams.GroupId.HasValue)
            {
                var apiIdsInGroup = await _dbContext.ApiGroupMappings
                    .Where(agm => agm.ApiGroupId == queryParams.GroupId.Value)
                    .Select(agm => agm.ApiPermissionId)
                    .ToListAsync(cancellationToken);

                query = query.Where(ap => apiIdsInGroup.Contains(ap.Id));
            }

            // 获取总数
            var totalCount = await query.CountAsync(cancellationToken);

            // 分页查询
            var apis = await query
                .OrderByDescending(ap => ap.CreateTime)
                .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(ap => new ApiPermissionListItem
                {
                    Id = ap.Id,
                    ApiCode = ap.ApiCode,
                    ApiName = ap.ApiName,
                    Endpoint = ap.Endpoint,
                    HttpMethod = ap.HttpMethod,
                    Description = ap.Description,
                    CreatedTime = ap.CreateTime
                })
                .ToListAsync(cancellationToken);

            // 获取分组信息
            var apiIds = apis.Select(a => a.Id).ToList();
            var apiGroupMappings = await _dbContext.ApiGroupMappings
                .Where(agm => apiIds.Contains(agm.ApiPermissionId))
                .Join(_dbContext.ApiGroups,
                    agm => agm.ApiGroupId,
                    g => g.Id,
                    (agm, g) => new { agm.ApiPermissionId, GroupId = g.Id, GroupName = g.GroupName })
                .ToListAsync(cancellationToken);

            // 组装分组信息
            foreach (var api in apis)
            {
                var mapping = apiGroupMappings.FirstOrDefault(agm => agm.ApiPermissionId == api.Id);
                if (mapping != null)
                {
                    api.GroupId = mapping.GroupId;
                    api.GroupName = mapping.GroupName;
                }
            }

            return new PagedResult<ApiPermissionListItem>
            {
                Items = apis,
                TotalCount = totalCount,
                PageIndex = queryParams.PageIndex,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<List<ApiPermissionSimpleInfo>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ApiPermissions
                .OrderBy(ap => ap.ApiName)
                .Select(ap => new ApiPermissionSimpleInfo
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

        #region A-002 新增API

        public async Task<long> CreateAsync(
            CreateApiPermissionRequest request,
            CancellationToken cancellationToken = default)
        {
            // 检查API编码是否已存在
            if (await _dbContext.ApiPermissions.AnyAsync(ap => ap.ApiCode == request.ApiCode, cancellationToken))
            {
                throw new InvalidOperationException($"API编码 '{request.ApiCode}' 已存在");
            }

            // 检查接口路径和方法是否已存在
            if (await _dbContext.ApiPermissions.AnyAsync(ap => ap.Endpoint == request.Endpoint && ap.HttpMethod == request.HttpMethod, cancellationToken))
            {
                throw new InvalidOperationException($"接口 '{request.HttpMethod} {request.Endpoint}' 已存在");
            }

            var apiPermission = new RbacApiPermission
            {
                ApiCode = request.ApiCode,
                ApiName = request.ApiName,
                Endpoint = request.Endpoint,
                HttpMethod = request.HttpMethod,
                Description = request.Description,
                RequireAuth = true,
                RequirePermission = true,
                CreateTime = DateTimeOffset.UtcNow,
                ModifyTime = DateTimeOffset.UtcNow
            };

            _dbContext.ApiPermissions.Add(apiPermission);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 如果指定了分组，添加分组映射
            if (request.GroupId.HasValue)
            {
                var groupMapping = new RbacApiGroupMapping
                {
                    ApiGroupId = request.GroupId.Value,
                    ApiPermissionId = apiPermission.Id,
                    CreateTime = DateTimeOffset.UtcNow,
                    ModifyTime = DateTimeOffset.UtcNow
                };
                _dbContext.ApiGroupMappings.Add(groupMapping);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return apiPermission.Id;
        }

        #endregion

        #region A-003 编辑API

        public async Task UpdateAsync(
            UpdateApiPermissionRequest request,
            CancellationToken cancellationToken = default)
        {
            var api = await _dbContext.ApiPermissions
                .FirstOrDefaultAsync(ap => ap.Id == request.Id, cancellationToken);

            if (api == null)
            {
                throw new InvalidOperationException($"API不存在 (ID: {request.Id})");
            }

            // 检查接口路径和方法是否被其他API使用
            if (await _dbContext.ApiPermissions.AnyAsync(ap => ap.Endpoint == request.Endpoint && ap.HttpMethod == request.HttpMethod && ap.Id != request.Id, cancellationToken))
            {
                throw new InvalidOperationException($"接口 '{request.HttpMethod} {request.Endpoint}' 已被其他API使用");
            }

            // 更新API信息
            api.ApiName = request.ApiName;
            api.Endpoint = request.Endpoint;
            api.HttpMethod = request.HttpMethod;
            api.Description = request.Description;
            api.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region A-004 删除API

        public async Task DeleteAsync(
            long apiId,
            CancellationToken cancellationToken = default)
        {
            var api = await _dbContext.ApiPermissions
                .FirstOrDefaultAsync(ap => ap.Id == apiId, cancellationToken);

            if (api == null)
            {
                throw new InvalidOperationException($"API不存在 (ID: {apiId})");
            }

            // 软删除API
            api.IsDeleted = true;
            api.ModifyTime = DateTimeOffset.UtcNow;

            // 软删除分组映射
            var groupMappings = await _dbContext.ApiGroupMappings
                .Where(agm => agm.ApiPermissionId == apiId)
                .ToListAsync(cancellationToken);
            foreach (var mapping in groupMappings)
            {
                mapping.IsDeleted = true;
                mapping.ModifyTime = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteBatchAsync(
            List<long> apiIds,
            CancellationToken cancellationToken = default)
        {
            foreach (var apiId in apiIds)
            {
                await DeleteAsync(apiId, cancellationToken);
            }
        }

        #endregion

        #region A-005 API分组列表

        public async Task<List<ApiGroupListItem>> GetGroupsAsync(
            CancellationToken cancellationToken = default)
        {
            var groups = await _dbContext.ApiGroups
                .OrderBy(g => g.SortOrder)
                .ThenBy(g => g.GroupName)
                .Select(g => new ApiGroupListItem
                {
                    Id = g.Id,
                    GroupCode = g.GroupCode,
                    GroupName = g.GroupName,
                    Description = g.Description,
                    SortOrder = g.SortOrder,
                    CreatedTime = g.CreateTime
                })
                .ToListAsync(cancellationToken);

            // 获取每个分组的API数量
            var groupIds = groups.Select(g => g.Id).ToList();
            var apiCounts = await _dbContext.ApiGroupMappings
                .Where(agm => groupIds.Contains(agm.ApiGroupId))
                .GroupBy(agm => agm.ApiGroupId)
                .Select(g => new { GroupId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            foreach (var group in groups)
            {
                group.ApiCount = apiCounts.FirstOrDefault(ac => ac.GroupId == group.Id)?.Count ?? 0;
            }

            return groups;
        }

        public async Task<List<ApiGroupWithApis>> GetGroupsWithApisAsync(
            CancellationToken cancellationToken = default)
        {
            var groups = await _dbContext.ApiGroups
                .OrderBy(g => g.SortOrder)
                .ThenBy(g => g.GroupName)
                .Select(g => new ApiGroupWithApis
                {
                    Id = g.Id,
                    GroupCode = g.GroupCode,
                    GroupName = g.GroupName,
                    SortOrder = g.SortOrder
                })
                .ToListAsync(cancellationToken);

            // 获取所有分组下的API
            var allGroupMappings = await _dbContext.ApiGroupMappings
                .Join(_dbContext.ApiPermissions,
                    agm => agm.ApiPermissionId,
                    ap => ap.Id,
                    (agm, ap) => new { agm.ApiGroupId, Api = ap })
                .ToListAsync(cancellationToken);

            foreach (var group in groups)
            {
                group.Apis = allGroupMappings
                    .Where(agm => agm.ApiGroupId == group.Id)
                    .Select(agm => new ApiPermissionSimpleInfo
                    {
                        Id = agm.Api.Id,
                        ApiCode = agm.Api.ApiCode,
                        ApiName = agm.Api.ApiName,
                        Endpoint = agm.Api.Endpoint,
                        HttpMethod = agm.Api.HttpMethod
                    })
                    .ToList();
            }

            return groups;
        }

        #endregion

        #region A-006 新增API分组

        public async Task<long> CreateGroupAsync(
            CreateApiGroupRequest request,
            CancellationToken cancellationToken = default)
        {
            // 检查分组编码是否已存在
            if (await _dbContext.ApiGroups.AnyAsync(g => g.GroupCode == request.GroupCode, cancellationToken))
            {
                throw new InvalidOperationException($"分组编码 '{request.GroupCode}' 已存在");
            }

            var group = new RbacApiGroup
            {
                GroupCode = request.GroupCode,
                GroupName = request.GroupName,
                Description = request.Description,
                SortOrder = request.SortOrder,
                CreateTime = DateTimeOffset.UtcNow,
                ModifyTime = DateTimeOffset.UtcNow
            };

            _dbContext.ApiGroups.Add(group);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return group.Id;
        }

        #endregion

        #region A-007 编辑API分组

        public async Task UpdateGroupAsync(
            UpdateApiGroupRequest request,
            CancellationToken cancellationToken = default)
        {
            var group = await _dbContext.ApiGroups
                .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

            if (group == null)
            {
                throw new InvalidOperationException($"分组不存在 (ID: {request.Id})");
            }

            // 更新分组信息
            group.GroupName = request.GroupName;
            group.Description = request.Description;
            group.SortOrder = request.SortOrder;
            group.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region A-008 删除API分组

        public async Task DeleteGroupAsync(
            long groupId,
            CancellationToken cancellationToken = default)
        {
            var (canDelete, apiCount) = await CheckGroupCanDeleteAsync(groupId, cancellationToken);

            if (!canDelete)
            {
                throw new InvalidOperationException($"该分组下有 {apiCount} 个API，无法删除");
            }

            var group = await _dbContext.ApiGroups
                .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

            if (group == null)
            {
                throw new InvalidOperationException($"分组不存在 (ID: {groupId})");
            }

            // 软删除分组
            group.IsDeleted = true;
            group.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<(bool CanDelete, int ApiCount)> CheckGroupCanDeleteAsync(
            long groupId,
            CancellationToken cancellationToken = default)
        {
            var apiCount = await _dbContext.ApiGroupMappings
                .Where(agm => agm.ApiGroupId == groupId)
                .CountAsync(cancellationToken);

            return (apiCount == 0, apiCount);
        }

        #endregion

        #region A-009 API分配到分组

        public async Task AssignToGroupAsync(
            AssignApiToGroupRequest request,
            CancellationToken cancellationToken = default)
        {
            // 验证API是否存在
            var api = await _dbContext.ApiPermissions
                .FirstOrDefaultAsync(ap => ap.Id == request.ApiId, cancellationToken);

            if (api == null)
            {
                throw new InvalidOperationException($"API不存在 (ID: {request.ApiId})");
            }

            // 删除现有的分组映射
            var existingMappings = await _dbContext.ApiGroupMappings
                .Where(agm => agm.ApiPermissionId == request.ApiId)
                .ToListAsync(cancellationToken);

            foreach (var mapping in existingMappings)
            {
                mapping.IsDeleted = true;
                mapping.ModifyTime = DateTimeOffset.UtcNow;
            }

            // 添加新的分组映射
            if (request.GroupId.HasValue)
            {
                // 验证分组是否存在
                var group = await _dbContext.ApiGroups
                    .FirstOrDefaultAsync(g => g.Id == request.GroupId.Value, cancellationToken);

                if (group == null)
                {
                    throw new InvalidOperationException($"分组不存在 (ID: {request.GroupId.Value})");
                }

                var newMapping = new RbacApiGroupMapping
                {
                    ApiGroupId = request.GroupId.Value,
                    ApiPermissionId = request.ApiId,
                    CreateTime = DateTimeOffset.UtcNow,
                    ModifyTime = DateTimeOffset.UtcNow
                };
                _dbContext.ApiGroupMappings.Add(newMapping);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AssignBatchToGroupAsync(
            AssignBatchApiToGroupRequest request,
            CancellationToken cancellationToken = default)
        {
            foreach (var apiId in request.ApiIds)
            {
                await AssignToGroupAsync(new AssignApiToGroupRequest
                {
                    ApiId = apiId,
                    GroupId = request.GroupId
                }, cancellationToken);
            }
        }

        #endregion

        #region A-010 自动扫描API

        public Task<ApiScanResult> ScanFromCodeAsync(
            CancellationToken cancellationToken = default)
        {
            // TODO: 实现从代码注解自动扫描API的功能
            // 这需要：
            // 1. 扫描所有Controller
            // 2. 读取 [ApiPermission] 特性
            // 3. 提取API信息
            // 目前返回空结果
            return Task.FromResult(new ApiScanResult
            {
                TotalCount = 0,
                NewCount = 0,
                ExistingCount = 0,
                Apis = []
            });
        }

        public async Task<ApiImportResult> ImportScannedApisAsync(
            ImportScannedApisRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = new ApiImportResult();

            foreach (var api in request.Apis)
            {
                // 检查是否已存在（通过ApiCode或Endpoint+HttpMethod）
                var existingApi = await _dbContext.ApiPermissions
                    .FirstOrDefaultAsync(ap => ap.ApiCode == api.ApiCode || (ap.Endpoint == api.Endpoint && ap.HttpMethod == api.HttpMethod), cancellationToken);

                if (existingApi != null)
                {
                    api.ExistingId = existingApi.Id;

                    if (request.Strategy == ImportStrategy.UpdateExisting)
                    {
                        // 更新已存在的API
                        existingApi.ApiName = api.ApiName;
                        existingApi.Description = api.Description;
                        existingApi.ModifyTime = DateTimeOffset.UtcNow;
                        result.UpdatedCount++;
                    }
                    else
                    {
                        // 跳过已存在的API
                        result.SkippedCount++;
                    }

                    api.IsNew = false;
                }
                else
                {
                    // 创建新的API
                    var newApi = new RbacApiPermission
                    {
                        ApiCode = api.ApiCode,
                        ApiName = api.ApiName,
                        Endpoint = api.Endpoint,
                        HttpMethod = api.HttpMethod,
                        Description = api.Description,
                        RequireAuth = true,
                        RequirePermission = true,
                        CreateTime = DateTimeOffset.UtcNow,
                        ModifyTime = DateTimeOffset.UtcNow
                    };

                    _dbContext.ApiPermissions.Add(newApi);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    api.IsNew = true;
                    api.ExistingId = newApi.Id;
                    result.AddedCount++;

                    // 自动创建分组（如果需要）
                    if (request.AutoCreateGroup && !string.IsNullOrEmpty(api.GroupName))
                    {
                        var group = await _dbContext.ApiGroups
                            .FirstOrDefaultAsync(g => g.GroupName == api.GroupName, cancellationToken);

                        if (group == null)
                        {
                            group = new RbacApiGroup
                            {
                                GroupCode = api.GroupName.ToUpperInvariant().Replace(" ", "_"),
                                GroupName = api.GroupName,
                                CreateTime = DateTimeOffset.UtcNow,
                                ModifyTime = DateTimeOffset.UtcNow
                            };
                            _dbContext.ApiGroups.Add(group);
                            await _dbContext.SaveChangesAsync(cancellationToken);
                            result.CreatedGroupCount++;
                        }

                        // 添加分组映射
                        var mapping = new RbacApiGroupMapping
                        {
                            ApiGroupId = group.Id,
                            ApiPermissionId = newApi.Id,
                            CreateTime = DateTimeOffset.UtcNow,
                            ModifyTime = DateTimeOffset.UtcNow
                        };
                        _dbContext.ApiGroupMappings.Add(mapping);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return result;
        }

        #endregion
    }
}
