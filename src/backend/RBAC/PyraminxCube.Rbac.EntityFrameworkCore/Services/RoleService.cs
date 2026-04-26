using Microsoft.EntityFrameworkCore;
using PyraminxCube.Rbac.Core.Models.Dto;
using PyraminxCube.Rbac.Core.Services;
using PyraminxCube.Rbac.EntityFrameworkCore.Entities;
using DataScopeFlag = PyraminxCube.Rbac.Core.Models.DataScopeFlag;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Services
{
    /// <summary>
    /// 角色服务实现
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly RbacDbContext _dbContext;

        public RoleService(RbacDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region R-001 角色列表

        public async Task<PagedResult<RoleListItem>> GetPagedListAsync(
            long tenantId,
            RoleQueryParams queryParams,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Roles
                .Where(r => r.TenantId == tenantId);

            // 角色编码模糊匹配
            if (!string.IsNullOrWhiteSpace(queryParams.RoleCode))
            {
                query = query.Where(r => r.RoleCode.Contains(queryParams.RoleCode));
            }

            // 角色名称模糊匹配
            if (!string.IsNullOrWhiteSpace(queryParams.RoleName))
            {
                query = query.Where(r => r.RoleName.Contains(queryParams.RoleName));
            }

            // 获取总数
            var totalCount = await query.CountAsync(cancellationToken);

            // 分页查询
            var roles = await query
                .OrderBy(r => r.SortOrder)
                .ThenByDescending(r => r.CreateTime)
                .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(r => new RoleListItem
                {
                    Id = r.Id,
                    RoleCode = r.RoleCode,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    SortOrder = r.SortOrder,
                    UserCount = r.UserRoles.Count(u => !u.IsDeleted),
                    CreatedTime = r.CreateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RoleListItem>
            {
                Items = roles,
                TotalCount = totalCount,
                PageIndex = queryParams.PageIndex,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<RoleDetail?> GetDetailAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default)
        {
            var role = await _dbContext.Roles
                .Where(r => r.Id == roleId && r.TenantId == tenantId)
                .Select(r => new RoleDetail
                {
                    Id = r.Id,
                    RoleCode = r.RoleCode,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    SortOrder = r.SortOrder,
                    UserCount = r.UserRoles.Count(u => !u.IsDeleted),
                    CreatedTime = r.CreateTime,
                    ModifiedTime = r.ModifyTime
                })
                .FirstOrDefaultAsync(cancellationToken);

            return role;
        }

        public async Task<List<RoleSimpleInfo>> GetAllAsync(
            long tenantId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Roles
                .Where(r => r.TenantId == tenantId)
                .OrderBy(r => r.SortOrder)
                .ThenBy(r => r.RoleName)
                .Select(r => new RoleSimpleInfo
                {
                    Id = r.Id,
                    RoleCode = r.RoleCode,
                    RoleName = r.RoleName
                })
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region R-002 新增角色

        public async Task<long> CreateAsync(
            long tenantId,
            CreateRoleRequest request,
            CancellationToken cancellationToken = default)
        {
            // 检查角色编码是否已存在
            if (await _dbContext.Roles.AnyAsync(r => r.RoleCode == request.RoleCode && r.TenantId == tenantId, cancellationToken))
            {
                throw new InvalidOperationException($"角色编码 '{request.RoleCode}' 已存在");
            }

            var role = new RbacRole
            {
                TenantId = tenantId,
                RoleCode = request.RoleCode,
                RoleName = request.RoleName,
                Description = request.Description,
                SortOrder = request.SortOrder,
                CreateTime = DateTimeOffset.UtcNow,
                ModifyTime = DateTimeOffset.UtcNow
            };

            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return role.Id;
        }

        #endregion

        #region R-003 编辑角色

        public async Task UpdateAsync(
            long tenantId,
            UpdateRoleRequest request,
            CancellationToken cancellationToken = default)
        {
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == request.Id && r.TenantId == tenantId, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"角色不存在 (ID: {request.Id})");
            }

            // 更新角色信息
            role.RoleName = request.RoleName;
            role.Description = request.Description;
            role.SortOrder = request.SortOrder;
            role.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region R-004 删除角色

        public async Task DeleteAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default)
        {
            var (canDelete, userCount) = await CheckCanDeleteAsync(tenantId, roleId, cancellationToken);

            if (!canDelete)
            {
                throw new InvalidOperationException($"该角色已被 {userCount} 个用户使用，无法删除");
            }

            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"角色不存在 (ID: {roleId})");
            }

            // 软删除角色
            role.IsDeleted = true;
            role.ModifyTime = DateTimeOffset.UtcNow;

            // 软删除角色API权限关联
            var roleApiPermissions = await _dbContext.RoleApiPermissions
                .Where(rap => rap.RoleId == roleId && rap.TenantId == tenantId)
                .ToListAsync(cancellationToken);
            foreach (var rap in roleApiPermissions)
            {
                rap.IsDeleted = true;
                rap.ModifyTime = DateTimeOffset.UtcNow;
            }

            // 软删除角色功能权限关联
            var roleFeaturePermissions = await _dbContext.RoleFeaturePermissions
                .Where(rfp => rfp.RoleId == roleId && rfp.TenantId == tenantId)
                .ToListAsync(cancellationToken);
            foreach (var rfp in roleFeaturePermissions)
            {
                rfp.IsDeleted = true;
                rfp.ModifyTime = DateTimeOffset.UtcNow;
            }

            // 软删除角色数据权限关联
            var roleDataScopes = await _dbContext.RoleDataScopes
                .Where(rds => rds.RoleId == roleId && rds.TenantId == tenantId)
                .ToListAsync(cancellationToken);
            foreach (var rds in roleDataScopes)
            {
                rds.IsDeleted = true;
                rds.ModifyTime = DateTimeOffset.UtcNow;
            }

            var roleDataScopeFlags = await _dbContext.RoleDataScopeFlags
                .Where(rdsf => rdsf.RoleId == roleId && rdsf.TenantId == tenantId)
                .ToListAsync(cancellationToken);
            foreach (var rdsf in roleDataScopeFlags)
            {
                rdsf.IsDeleted = true;
                rdsf.ModifyTime = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<(bool CanDelete, int UserCount)> CheckCanDeleteAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default)
        {
            var userCount = await _dbContext.UserRoles
                .Where(ur => ur.RoleId == roleId && ur.TenantId == tenantId && !ur.IsDeleted)
                .Select(ur => ur.UserId)
                .Distinct()
                .CountAsync(cancellationToken);

            return (userCount == 0, userCount);
        }

        #endregion

        #region R-005 为角色分配API权限

        public async Task AssignApiPermissionsAsync(
            long tenantId,
            AssignApiPermissionsRequest request,
            CancellationToken cancellationToken = default)
        {
            // 验证角色是否存在
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == request.RoleId && r.TenantId == tenantId, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"角色不存在 (ID: {request.RoleId})");
            }

            // 获取现有的API权限关联
            var existingApiPermissions = await _dbContext.RoleApiPermissions
                .Where(rap => rap.RoleId == request.RoleId && rap.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            // 软删除所有旧API权限
            foreach (var rap in existingApiPermissions)
            {
                rap.IsDeleted = true;
                rap.ModifyTime = DateTimeOffset.UtcNow;
            }

            // 存储已添加的 API 权限 ID，用于避免重复
            var addedApiPermissionIds = new HashSet<long>();

            // 添加新的API权限
            if (request.ApiPermissionIds.Count > 0)
            {
                // 验证所有API权限ID存在
                var validApiPermissionIds = await _dbContext.ApiPermissions
                    .Where(ap => request.ApiPermissionIds.Contains(ap.Id))
                    .Select(ap => ap.Id)
                    .ToListAsync(cancellationToken);

                foreach (var apiPermissionId in validApiPermissionIds)
                {
                    var roleApiPermission = new RbacRoleApiPermission
                    {
                        TenantId = tenantId,
                        RoleId = request.RoleId,
                        ApiPermissionId = apiPermissionId,
                        CreateTime = DateTimeOffset.UtcNow,
                        ModifyTime = DateTimeOffset.UtcNow
                    };
                    _dbContext.RoleApiPermissions.Add(roleApiPermission);
                    addedApiPermissionIds.Add(apiPermissionId);
                }
            }

            // 处理API分组权限 - 展开分组下的所有 API 权限
            if (request.ApiGroupIds.Count > 0)
            {
                // 获取分组下所有 API 权限 ID
                var apiPermissionIdsInGroups = await _dbContext.ApiGroupMappings
                    .Where(agm => request.ApiGroupIds.Contains(agm.ApiGroupId))
                    .Select(agm => agm.ApiPermissionId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                // 添加这些 API 权限（排除已添加的）
                foreach (var apiPermissionId in apiPermissionIdsInGroups)
                {
                    if (!addedApiPermissionIds.Contains(apiPermissionId))
                    {
                        var roleApiPermission = new RbacRoleApiPermission
                        {
                            TenantId = tenantId,
                            RoleId = request.RoleId,
                            ApiPermissionId = apiPermissionId,
                            CreateTime = DateTimeOffset.UtcNow,
                            ModifyTime = DateTimeOffset.UtcNow
                        };
                        _dbContext.RoleApiPermissions.Add(roleApiPermission);
                        addedApiPermissionIds.Add(apiPermissionId);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<RoleApiPermissionInfo> GetApiPermissionsAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default)
        {
            // 获取已分配的 API 权限 ID
            var apiPermissionIds = await _dbContext.RoleApiPermissions
                .Where(rap => rap.RoleId == roleId && rap.TenantId == tenantId)
                .Select(rap => rap.ApiPermissionId)
                .ToListAsync(cancellationToken);

            // 注意：当前实现不支持按分组存储，分组 ID 列表为空
            // 如果需要支持分组权限，需要额外的关联表
            return new RoleApiPermissionInfo
            {
                ApiPermissionIds = apiPermissionIds,
                ApiGroupIds = []
            };
        }

        #endregion

        #region R-006 为角色分配功能权限

        public async Task AssignFeaturePermissionsAsync(
            long tenantId,
            AssignFeaturePermissionsRequest request,
            CancellationToken cancellationToken = default)
        {
            // 验证角色是否存在
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == request.RoleId && r.TenantId == tenantId, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"角色不存在 (ID: {request.RoleId})");
            }

            // 获取现有的功能权限关联
            var existingFeaturePermissions = await _dbContext.RoleFeaturePermissions
                .Where(rfp => rfp.RoleId == request.RoleId && rfp.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            // 软删除所有旧功能权限
            foreach (var rfp in existingFeaturePermissions)
            {
                rfp.IsDeleted = true;
                rfp.ModifyTime = DateTimeOffset.UtcNow;
            }

            // 添加新的功能权限
            if (request.FeaturePermissionIds.Count > 0)
            {
                // 验证所有功能权限ID存在
                var validFeaturePermissionIds = await _dbContext.FeaturePermissions
                    .Where(fp => request.FeaturePermissionIds.Contains(fp.Id))
                    .Select(fp => fp.Id)
                    .ToListAsync(cancellationToken);

                foreach (var featurePermissionId in validFeaturePermissionIds)
                {
                    var roleFeaturePermission = new RbacRoleFeaturePermission
                    {
                        TenantId = tenantId,
                        RoleId = request.RoleId,
                        FeaturePermissionId = featurePermissionId,
                        CreateTime = DateTimeOffset.UtcNow,
                        ModifyTime = DateTimeOffset.UtcNow
                    };
                    _dbContext.RoleFeaturePermissions.Add(roleFeaturePermission);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<long>> GetFeaturePermissionIdsAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.RoleFeaturePermissions
                .Where(rfp => rfp.RoleId == roleId && rfp.TenantId == tenantId)
                .Select(rfp => rfp.FeaturePermissionId)
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region R-007 为角��分配数据权限

        public async Task AssignDataScopesAsync(
            long tenantId,
            AssignDataScopesRequest request,
            CancellationToken cancellationToken = default)
        {
            // 验证角色是否存在
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == request.RoleId && r.TenantId == tenantId, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"角色不存在 (ID: {request.RoleId})");
            }

            // 获取现有的数据权限关联
            var existingDataScopes = await _dbContext.RoleDataScopes
                .Where(rds => rds.RoleId == request.RoleId && rds.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            var existingDataScopeFlags = await _dbContext.RoleDataScopeFlags
                .Where(rdsf => rdsf.RoleId == request.RoleId && rdsf.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            // 软删除所有旧数据权限
            foreach (var rds in existingDataScopes)
            {
                rds.IsDeleted = true;
                rds.ModifyTime = DateTimeOffset.UtcNow;
            }

            foreach (var rdsf in existingDataScopeFlags)
            {
                rdsf.IsDeleted = true;
                rdsf.ModifyTime = DateTimeOffset.UtcNow;
            }

            // 添加新的数据权限配置
            foreach (var config in request.DataScopeConfigs)
            {
                // 验证维度是否存在
                var dimension = await _dbContext.DataDimensions
                    .FirstOrDefaultAsync(d => d.Id == config.DimensionId, cancellationToken);

                if (dimension == null)
                {
                    throw new InvalidOperationException($"数据维度不存在 (ID: {config.DimensionId})");
                }

                if (!string.IsNullOrEmpty(config.ScopeFlag))
                {
                    // 处理特殊标记 ALL/SELF
                    if (config.ScopeFlag == "ALL" || config.ScopeFlag == "SELF")
                    {
                        var roleDataScopeFlag = new RbacRoleDataScopeFlag
                        {
                            TenantId = tenantId,
                            RoleId = request.RoleId,
                            DataDimensionId = config.DimensionId,
                            Flag = config.ScopeFlag == "ALL" ? DataScopeFlag.All : DataScopeFlag.Self,
                            CreateTime = DateTimeOffset.UtcNow,
                            ModifyTime = DateTimeOffset.UtcNow
                        };
                        _dbContext.RoleDataScopeFlags.Add(roleDataScopeFlag);
                    }
                }
                else if (config.ScopeIds.Count > 0)
                {
                    // 处理自定义数据范围
                    foreach (var scopeId in config.ScopeIds)
                    {
                        var roleDataScope = new RbacRoleDataScope
                        {
                            TenantId = tenantId,
                            RoleId = request.RoleId,
                            DataDimensionId = config.DimensionId,
                            ScopeId = scopeId,
                            CreateTime = DateTimeOffset.UtcNow,
                            ModifyTime = DateTimeOffset.UtcNow
                        };
                        _dbContext.RoleDataScopes.Add(roleDataScope);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<RoleDataScopeInfo>> GetDataScopesAsync(
            long tenantId,
            long roleId,
            CancellationToken cancellationToken = default)
        {
            var result = new List<RoleDataScopeInfo>();

            // 获取所有维度
            var dimensions = await _dbContext.DataDimensions.ToListAsync(cancellationToken);

            foreach (var dimension in dimensions)
            {
                var info = new RoleDataScopeInfo
                {
                    DimensionId = dimension.Id,
                    DimensionCode = dimension.DimensionCode,
                    DimensionName = dimension.DimensionName
                };

                // 检查是否有 ALL 标记
                var hasAllFlag = await _dbContext.RoleDataScopeFlags
                    .AnyAsync(rdsf => rdsf.RoleId == roleId
                        && rdsf.DataDimensionId == dimension.Id
                        && rdsf.TenantId == tenantId
                        && rdsf.Flag == DataScopeFlag.All, cancellationToken);

                if (hasAllFlag)
                {
                    info.ScopeFlag = "ALL";
                    result.Add(info);
                    continue;
                }

                // 检查是否有 SELF 标记
                var hasSelfFlag = await _dbContext.RoleDataScopeFlags
                    .AnyAsync(rdsf => rdsf.RoleId == roleId
                        && rdsf.DataDimensionId == dimension.Id
                        && rdsf.TenantId == tenantId
                        && rdsf.Flag == DataScopeFlag.Self, cancellationToken);

                // 获取具体的 ScopeIds
                var scopeIds = await _dbContext.RoleDataScopes
                    .Where(rds => rds.RoleId == roleId
                        && rds.DataDimensionId == dimension.Id
                        && rds.TenantId == tenantId)
                    .Select(rds => rds.ScopeId)
                    .ToListAsync(cancellationToken);

                if (hasSelfFlag && scopeIds.Count == 0)
                {
                    info.ScopeFlag = "SELF";
                }
                else if (scopeIds.Count > 0)
                {
                    info.ScopeFlag = null; // Custom
                    info.ScopeIds = scopeIds;
                }
                else
                {
                    // 没有配置数据权限
                    continue;
                }

                result.Add(info);
            }

            return result;
        }

        #endregion

        #region R-008 复制角色

        public async Task<long> CopyAsync(
            long tenantId,
            CopyRoleRequest request,
            CancellationToken cancellationToken = default)
        {
            // 检查源角色是否存在
            var sourceRole = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == request.SourceRoleId && r.TenantId == tenantId, cancellationToken);

            if (sourceRole == null)
            {
                throw new InvalidOperationException($"源角色不存在 (ID: {request.SourceRoleId})");
            }

            // 检查新角色编码是否已存在
            if (await _dbContext.Roles.AnyAsync(r => r.RoleCode == request.NewRoleCode && r.TenantId == tenantId, cancellationToken))
            {
                throw new InvalidOperationException($"角色编码 '{request.NewRoleCode}' 已存在");
            }

            // 创建新角色
            var newRole = new RbacRole
            {
                TenantId = tenantId,
                RoleCode = request.NewRoleCode,
                RoleName = request.NewRoleName,
                Description = request.NewDescription ?? sourceRole.Description,
                SortOrder = sourceRole.SortOrder,
                CreateTime = DateTimeOffset.UtcNow,
                ModifyTime = DateTimeOffset.UtcNow
            };

            _dbContext.Roles.Add(newRole);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 复制API权限
            if (request.CopyApiPermissions)
            {
                var apiPermissions = await _dbContext.RoleApiPermissions
                    .Where(rap => rap.RoleId == request.SourceRoleId && rap.TenantId == tenantId)
                    .ToListAsync(cancellationToken);

                foreach (var apiPermission in apiPermissions)
                {
                    var newRoleApiPermission = new RbacRoleApiPermission
                    {
                        TenantId = tenantId,
                        RoleId = newRole.Id,
                        ApiPermissionId = apiPermission.ApiPermissionId,
                        CreateTime = DateTimeOffset.UtcNow,
                        ModifyTime = DateTimeOffset.UtcNow
                    };
                    _dbContext.RoleApiPermissions.Add(newRoleApiPermission);
                }
            }

            // 复制功能权限
            if (request.CopyFeaturePermissions)
            {
                var featurePermissions = await _dbContext.RoleFeaturePermissions
                    .Where(rfp => rfp.RoleId == request.SourceRoleId && rfp.TenantId == tenantId)
                    .ToListAsync(cancellationToken);

                foreach (var featurePermission in featurePermissions)
                {
                    var newRoleFeaturePermission = new RbacRoleFeaturePermission
                    {
                        TenantId = tenantId,
                        RoleId = newRole.Id,
                        FeaturePermissionId = featurePermission.FeaturePermissionId,
                        CreateTime = DateTimeOffset.UtcNow,
                        ModifyTime = DateTimeOffset.UtcNow
                    };
                    _dbContext.RoleFeaturePermissions.Add(newRoleFeaturePermission);
                }
            }

            // 复制数据权限
            if (request.CopyDataScopes)
            {
                // 复制数据范围
                var dataScopes = await _dbContext.RoleDataScopes
                    .Where(rds => rds.RoleId == request.SourceRoleId && rds.TenantId == tenantId)
                    .ToListAsync(cancellationToken);

                foreach (var dataScope in dataScopes)
                {
                    var newRoleDataScope = new RbacRoleDataScope
                    {
                        TenantId = tenantId,
                        RoleId = newRole.Id,
                        DataDimensionId = dataScope.DataDimensionId,
                        ScopeId = dataScope.ScopeId,
                        CreateTime = DateTimeOffset.UtcNow,
                        ModifyTime = DateTimeOffset.UtcNow
                    };
                    _dbContext.RoleDataScopes.Add(newRoleDataScope);
                }

                // 复制数据范围标记
                var dataScopeFlags = await _dbContext.RoleDataScopeFlags
                    .Where(rdsf => rdsf.RoleId == request.SourceRoleId && rdsf.TenantId == tenantId)
                    .ToListAsync(cancellationToken);

                foreach (var dataScopeFlag in dataScopeFlags)
                {
                    var newRoleDataScopeFlag = new RbacRoleDataScopeFlag
                    {
                        TenantId = tenantId,
                        RoleId = newRole.Id,
                        DataDimensionId = dataScopeFlag.DataDimensionId,
                        Flag = dataScopeFlag.Flag,
                        CreateTime = DateTimeOffset.UtcNow,
                        ModifyTime = DateTimeOffset.UtcNow
                    };
                    _dbContext.RoleDataScopeFlags.Add(newRoleDataScopeFlag);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return newRole.Id;
        }

        #endregion

        #region R-009 查看角色用户

        public async Task<PagedResult<RoleUserInfo>> GetUsersAsync(
            long tenantId,
            long roleId,
            RoleUserQueryParams queryParams,
            CancellationToken cancellationToken = default)
        {
            // 验证角色是否存在
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"角色不存在 (ID: {roleId})");
            }

            var query = _dbContext.Users
                .Where(u => u.TenantId == tenantId)
                .Join(_dbContext.UserRoles
                        .Where(ur => ur.RoleId == roleId && ur.TenantId == tenantId && !ur.IsDeleted),
                    u => u.Id,
                    ur => ur.UserId,
                    (u, ur) => u);

            // 用户名模糊匹配
            if (!string.IsNullOrWhiteSpace(queryParams.Username))
            {
                query = query.Where(u => u.Username.Contains(queryParams.Username));
            }

            // 获取总数
            var totalCount = await query.CountAsync(cancellationToken);

            // 分页查询
            var users = await query
                .OrderByDescending(u => u.CreateTime)
                .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(u => new RoleUserInfo
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Nickname = u.Profile != null ? u.Profile.Nickname : null,
                    Status = u.Status,
                    CreatedTime = u.CreateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<RoleUserInfo>
            {
                Items = users,
                TotalCount = totalCount,
                PageIndex = queryParams.PageIndex,
                PageSize = queryParams.PageSize
            };
        }

        #endregion
    }
}