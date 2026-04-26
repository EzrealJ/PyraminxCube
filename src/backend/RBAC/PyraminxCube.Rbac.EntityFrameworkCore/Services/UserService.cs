using Microsoft.EntityFrameworkCore;
using PyraminxCube.Rbac.Core.Models;
using PyraminxCube.Rbac.Core.Models.Dto;
using PyraminxCube.Rbac.Core.Services;
using PyraminxCube.Rbac.EntityFrameworkCore.Entities;
using System.Security.Cryptography;
using System.Text;
using DataScopeFlag = PyraminxCube.Rbac.Core.Models.DataScopeFlag;
using DataScopeValue = PyraminxCube.Rbac.Core.Models.DataScopeValue;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Services
{
    /// <summary>
    /// 用户服务实现
    /// </summary>
    public class UserService : IUserService
    {
        private readonly RbacDbContext _dbContext;

        public UserService(RbacDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region U-001 用户列表

        public async Task<PagedResult<UserListItem>> GetPagedListAsync(
            long tenantId,
            UserQueryParams queryParams,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Users
                .Where(u => u.TenantId == tenantId);

            // 应用过滤条件
            if (!string.IsNullOrWhiteSpace(queryParams.Username))
            {
                query = query.Where(u => u.Username.Contains(queryParams.Username));
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Email))
            {
                query = query.Where(u => u.Email.Contains(queryParams.Email));
            }

            if (queryParams.Status.HasValue)
            {
                query = query.Where(u => u.Status == queryParams.Status.Value);
            }

            if (queryParams.CreatedTimeStart.HasValue)
            {
                query = query.Where(u => u.CreateTime >= queryParams.CreatedTimeStart.Value);
            }

            if (queryParams.CreatedTimeEnd.HasValue)
            {
                query = query.Where(u => u.CreateTime <= queryParams.CreatedTimeEnd.Value);
            }

            // 获取总数
            var totalCount = await query.CountAsync(cancellationToken);

            // 分页查询
            var users = await query
                .OrderByDescending(u => u.CreateTime)
                .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(u => new UserListItem
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Status = u.Status,
                    IsSuperAdmin = u.IsSuperAdmin,
                    Nickname = u.Profile != null ? u.Profile.Nickname : null,
                    Avatar = u.Profile != null ? u.Profile.Avatar : null,
                    CreatedTime = u.CreateTime
                })
                .ToListAsync(cancellationToken);

            // 获取用户角色
            var userIds = users.Select(u => u.Id).ToList();
            var userRoles = await _dbContext.UserRoles
                .Where(ur => userIds.Contains(ur.UserId) && ur.TenantId == tenantId)
                .Join(_dbContext.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, Role = r })
                .ToListAsync(cancellationToken);

            // 组装角色信息
            foreach (var user in users)
            {
                user.Roles = userRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Select(ur => new UserRoleInfo
                    {
                        Id = ur.Role.Id,
                        RoleCode = ur.Role.RoleCode,
                        RoleName = ur.Role.RoleName
                    })
                    .ToList();
            }

            return new PagedResult<UserListItem>
            {
                Items = users,
                TotalCount = totalCount,
                PageIndex = queryParams.PageIndex,
                PageSize = queryParams.PageSize
            };
        }

        #endregion

        #region U-002 新增用户

        public async Task<long> CreateAsync(
            long tenantId,
            CreateUserRequest request,
            CancellationToken cancellationToken = default)
        {
            // 检查用户名是否已存在
            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username && u.TenantId == tenantId, cancellationToken))
            {
                throw new InvalidOperationException($"用户名 '{request.Username}' 已存在");
            }

            // 检查邮箱是否已存在
            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email && u.TenantId == tenantId, cancellationToken))
            {
                throw new InvalidOperationException($"邮箱 '{request.Email}' 已存在");
            }

            // 创建用户
            var user = new RbacUser
            {
                TenantId = tenantId,
                Username = request.Username,
                Email = request.Email,
                Password = HashPassword(request.Password),
                Status = 1,
                IsSuperAdmin = request.IsSuperAdmin,
                CreateTime = DateTimeOffset.UtcNow,
                ModifyTime = DateTimeOffset.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 创建用户扩展信息
            var profile = new RbacUserProfile
            {
                TenantId = tenantId,
                UserId = user.Id,
                Nickname = request.Nickname,
                PhoneNumber = request.PhoneNumber,
                CreateTime = DateTimeOffset.UtcNow,
                ModifyTime = DateTimeOffset.UtcNow
            };

            _dbContext.UserProfiles.Add(profile);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 分配角色
            if (request.RoleIds.Count > 0)
            {
                foreach (var roleId in request.RoleIds)
                {
                    var userRole = new RbacUserRole
                    {
                        TenantId = tenantId,
                        UserId = user.Id,
                        RoleId = roleId,
                        CreateTime = DateTimeOffset.UtcNow,
                        ModifyTime = DateTimeOffset.UtcNow
                    };
                    _dbContext.UserRoles.Add(userRole);
                }
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return user.Id;
        }

        #endregion

        #region U-003 编辑用户

        public async Task UpdateAsync(
            long tenantId,
            UpdateUserRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id && u.TenantId == tenantId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"用户不存在 (ID: {request.Id})");
            }

            // 检查邮箱是否被其他用户使用
            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email && u.Id != request.Id && u.TenantId == tenantId, cancellationToken))
            {
                throw new InvalidOperationException($"邮箱 '{request.Email}' 已被其他用户使用");
            }

            // 更新用户基本信息
            user.Email = request.Email;
            user.ModifyTime = DateTimeOffset.UtcNow;

            // 更新用户扩展信息
            var profile = await _dbContext.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.TenantId == tenantId, cancellationToken);

            if (profile == null)
            {
                profile = new RbacUserProfile
                {
                    TenantId = tenantId,
                    UserId = user.Id,
                    CreateTime = DateTimeOffset.UtcNow
                };
                _dbContext.UserProfiles.Add(profile);
            }

            profile.Nickname = request.Nickname;
            profile.PhoneNumber = request.PhoneNumber;
            profile.Gender = request.Gender ?? 0;
            profile.Birthday = request.Birthday;
            profile.Address = request.Address;
            profile.Bio = request.Bio;
            profile.ModifyTime = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region U-004 删除用户

        public async Task DeleteAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"用户不存在 (ID: {userId})");
            }

            if (user.IsSuperAdmin)
            {
                throw new InvalidOperationException("不能删除超级管理员");
            }

            // 软删除用户
            user.IsDeleted = true;
            user.ModifyTime = DateTimeOffset.UtcNow;

            // 软删除用户扩展信息
            var profile = await _dbContext.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId && p.TenantId == tenantId, cancellationToken);
            if (profile != null)
            {
                profile.IsDeleted = true;
                profile.ModifyTime = DateTimeOffset.UtcNow;
            }

            // 软删除用户角色关联
            var userRoles = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
                .ToListAsync(cancellationToken);
            foreach (var userRole in userRoles)
            {
                userRole.IsDeleted = true;
                userRole.ModifyTime = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region U-005 启用/禁用用户

        public async Task EnableAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"用户不存在 (ID: {userId})");
            }

            user.Status = 1;
            user.ModifyTime = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DisableAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"用户不存在 (ID: {userId})");
            }

            if (user.IsSuperAdmin)
            {
                throw new InvalidOperationException("不能禁用超级管理员");
            }

            user.Status = 0;
            user.ModifyTime = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ToggleStatusAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"用户不存在 (ID: {userId})");
            }

            if (user.IsSuperAdmin && user.Status == 1)
            {
                throw new InvalidOperationException("不能禁用超级管理员");
            }

            user.Status = user.Status == 1 ? 0 : 1;
            user.ModifyTime = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return user.Status == 1;
        }

        #endregion

        #region U-006 重置密码

        public async Task ResetPasswordAsync(
            long tenantId,
            ResetPasswordRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.TenantId == tenantId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"用户不存在 (ID: {request.UserId})");
            }

            user.Password = HashPassword(request.NewPassword);
            user.ModifyTime = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region U-007 分配角色

        public async Task AssignRolesAsync(
            long tenantId,
            AssignRolesRequest request,
            CancellationToken cancellationToken = default)
        {
            // 验证用户是否存在
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.TenantId == tenantId, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"用户不存在 (ID: {request.UserId})");
            }

            // 获取用户当前的角色
            var existingRoles = await _dbContext.UserRoles
                .Where(ur => ur.UserId == request.UserId && ur.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            // 软删除所有旧角色
            foreach (var role in existingRoles)
            {
                role.IsDeleted = true;
                role.ModifyTime = DateTimeOffset.UtcNow;
            }

            // 添加新角色
            if (request.RoleIds.Count > 0)
            {
                foreach (var roleId in request.RoleIds)
                {
                    // 验证角色是否存在
                    if (!await _dbContext.Roles.AnyAsync(r => r.Id == roleId && r.TenantId == tenantId, cancellationToken))
                    {
                        throw new InvalidOperationException($"角色不存在 (ID: {roleId})");
                    }

                    var userRole = new RbacUserRole
                    {
                        TenantId = tenantId,
                        UserId = request.UserId,
                        RoleId = roleId,
                        CreateTime = DateTimeOffset.UtcNow,
                        ModifyTime = DateTimeOffset.UtcNow
                    };
                    _dbContext.UserRoles.Add(userRole);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<long>> GetUserRoleIdsAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region U-008 查看用户权限

        public async Task<UserPermissions> GetUserPermissionsAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default)
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

        public async Task<List<FeaturePermissionInfo>> GetUserFeatureTreeAsync(
            long tenantId,
            long userId,
            CancellationToken cancellationToken = default)
        {
            var permissions = await GetUserPermissionsAsync(tenantId, userId, cancellationToken);

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

        #endregion

        #region 私有方法

        /// <summary>
        /// 使用 PBKDF2 加密密码
        /// </summary>
        private static string HashPassword(string password)
        {
            // 生成随机盐
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // 使用 PBKDF2 生成哈希
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32);

            // 合并盐和哈希
            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        private static bool VerifyPassword(string password, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32);

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
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
        /// 合并多个角色的数据权限（并集合并）
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

        #endregion
    }
}
