using Microsoft.AspNetCore.Mvc;
using PyraminxCube.Applications.WebApp.Common;
using PyraminxCube.Rbac.Core.Models.Dto;
using PyraminxCube.Rbac.Core.Services;

namespace PyraminxCube.Applications.WebApp.Controllers;

/// <summary>
/// 角色管理控制器
/// </summary>
public class RolesController : BaseController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetList(
        [FromQuery] string? roleCode,
        [FromQuery] string? roleName,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _roleService.GetPagedListAsync(CurrentTenantId, new RoleQueryParams
        {
            RoleCode = roleCode,
            RoleName = roleName,
            PageIndex = pageIndex,
            PageSize = pageSize
        });

        return Paged(result);
    }

    /// <summary>
    /// 获取所有角色（下拉选择用）
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult> GetAll()
    {
        var roles = await _roleService.GetAllAsync(CurrentTenantId);
        return Success(roles);
    }

    /// <summary>
    /// 获取角色详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(long id)
    {
        var role = await _roleService.GetDetailAsync(CurrentTenantId, id);
        if (role == null)
        {
            return Fail("角色不存在");
        }
        return Success(role);
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var roleId = await _roleService.CreateAsync(CurrentTenantId, request);
        return Success(new { Id = roleId }, "角色创建成功");
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(long id, [FromBody] UpdateRoleRequest request)
    {
        if (id != request.Id)
        {
            return Fail("ID 不匹配");
        }

        await _roleService.UpdateAsync(CurrentTenantId, request);
        return Success("角色更新成功");
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        var (canDelete, userCount) = await _roleService.CheckCanDeleteAsync(CurrentTenantId, id);
        if (!canDelete)
        {
            return Fail($"该角色已被 {userCount} 个用户使用，无法删除");
        }

        await _roleService.DeleteAsync(CurrentTenantId, id);
        return Success("角色删除成功");
    }

    /// <summary>
    /// 检查角色是否可删除
    /// </summary>
    [HttpGet("{id}/deletable")]
    public async Task<ActionResult> CheckDeletable(long id)
    {
        var (canDelete, userCount) = await _roleService.CheckCanDeleteAsync(CurrentTenantId, id);
        return Success(new { CanDelete = canDelete, UserCount = userCount });
    }

    #region API 权限管理

    /// <summary>
    /// 获取角色的 API 权限
    /// </summary>
    [HttpGet("{id}/api-permissions")]
    public async Task<ActionResult> GetApiPermissions(long id)
    {
        var permissions = await _roleService.GetApiPermissionsAsync(CurrentTenantId, id);
        return Success(permissions);
    }

    /// <summary>
    /// 分配 API 权限
    /// </summary>
    [HttpPut("{id}/api-permissions")]
    public async Task<ActionResult> AssignApiPermissions(long id, [FromBody] AssignApiPermissionsRequest request)
    {
        if (id != request.RoleId)
        {
            return Fail("ID 不匹配");
        }

        await _roleService.AssignApiPermissionsAsync(CurrentTenantId, request);
        return Success("API 权限分配成功");
    }

    #endregion

    #region 功能权限管理

    /// <summary>
    /// 获取角色的功能权限ID列表
    /// </summary>
    [HttpGet("{id}/feature-permissions")]
    public async Task<ActionResult> GetFeaturePermissions(long id)
    {
        var permissionIds = await _roleService.GetFeaturePermissionIdsAsync(CurrentTenantId, id);
        return Success(new { FeaturePermissionIds = permissionIds });
    }

    /// <summary>
    /// 分配功能权限
    /// </summary>
    [HttpPut("{id}/feature-permissions")]
    public async Task<ActionResult> AssignFeaturePermissions(long id, [FromBody] AssignFeaturePermissionsRequest request)
    {
        if (id != request.RoleId)
        {
            return Fail("ID 不匹配");
        }

        await _roleService.AssignFeaturePermissionsAsync(CurrentTenantId, request);
        return Success("功能权限分配成功");
    }

    #endregion

    #region 数据权限管理

    /// <summary>
    /// 获取角色的数据权限配置
    /// </summary>
    [HttpGet("{id}/data-scopes")]
    public async Task<ActionResult> GetDataScopes(long id)
    {
        var scopes = await _roleService.GetDataScopesAsync(CurrentTenantId, id);
        return Success(scopes);
    }

    /// <summary>
    /// 分配数据权限
    /// </summary>
    [HttpPut("{id}/data-scopes")]
    public async Task<ActionResult> AssignDataScopes(long id, [FromBody] AssignDataScopesRequest request)
    {
        if (id != request.RoleId)
        {
            return Fail("ID 不匹配");
        }

        await _roleService.AssignDataScopesAsync(CurrentTenantId, request);
        return Success("数据权限分配成功");
    }

    #endregion

    /// <summary>
    /// 复制角色
    /// </summary>
    [HttpPost("{id}/copy")]
    public async Task<ActionResult> Copy(long id, [FromBody] CopyRoleRequest request)
    {
        if (id != request.SourceRoleId)
        {
            return Fail("ID 不匹配");
        }

        var newRoleId = await _roleService.CopyAsync(CurrentTenantId, request);
        return Success(new { Id = newRoleId }, "角色复制成功");
    }

    /// <summary>
    /// 获取角色的用户列表
    /// </summary>
    [HttpGet("{id}/users")]
    public async Task<ActionResult> GetUsers(
        long id,
        [FromQuery] string? username,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _roleService.GetUsersAsync(CurrentTenantId, id, new RoleUserQueryParams
        {
            Username = username,
            PageIndex = pageIndex,
            PageSize = pageSize
        });

        return Paged(result);
    }
}
