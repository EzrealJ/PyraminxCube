using Microsoft.AspNetCore.Mvc;
using PyraminxCube.Applications.WebApp.Common;
using PyraminxCube.Rbac.Core.Models;
using PyraminxCube.Rbac.Core.Models.Dto;
using PyraminxCube.Rbac.Core.Services;

namespace PyraminxCube.Applications.WebApp.Controllers;

/// <summary>
/// 用户管理控制器
/// </summary>
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetList(
        [FromQuery] string? username,
        [FromQuery] string? email,
        [FromQuery] int? status,
        [FromQuery] DateTimeOffset? createdTimeStart,
        [FromQuery] DateTimeOffset? createdTimeEnd,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _userService.GetPagedListAsync(CurrentTenantId, new UserQueryParams
        {
            Username = username,
            Email = email,
            Status = status,
            CreatedTimeStart = createdTimeStart,
            CreatedTimeEnd = createdTimeEnd,
            PageIndex = pageIndex,
            PageSize = pageSize
        });

        return Paged(result);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateUserRequest request)
    {
        var userId = await _userService.CreateAsync(CurrentTenantId, request);
        return Success(new { Id = userId }, "用户创建成功");
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(long id, [FromBody] UpdateUserRequest request)
    {
        if (id != request.Id)
        {
            return Fail("ID 不匹配");
        }

        await _userService.UpdateAsync(CurrentTenantId, request);
        return Success("用户更新成功");
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        await _userService.DeleteAsync(CurrentTenantId, id);
        return Success("用户删除成功");
    }

    /// <summary>
    /// 启用/禁用用户
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult> ToggleStatus(long id)
    {
        var isEnabled = await _userService.ToggleStatusAsync(CurrentTenantId, id);
        return Success(new { IsEnabled = isEnabled }, isEnabled ? "用户已启用" : "用户已禁用");
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    [HttpPatch("{id}/password/reset")]
    public async Task<ActionResult> ResetPassword(long id, [FromBody] ResetPasswordRequest request)
    {
        if (id != request.UserId)
        {
            return Fail("ID 不匹配");
        }

        await _userService.ResetPasswordAsync(CurrentTenantId, request);
        return Success("密码重置成功");
    }

    /// <summary>
    /// 分配角色
    /// </summary>
    [HttpPut("{id}/roles")]
    public async Task<ActionResult> AssignRoles(long id, [FromBody] AssignRolesRequest request)
    {
        if (id != request.UserId)
        {
            return Fail("ID 不匹配");
        }

        await _userService.AssignRolesAsync(CurrentTenantId, request);
        return Success("角色分配成功");
    }

    /// <summary>
    /// 获取用户的角色ID列表
    /// </summary>
    [HttpGet("{id}/roles")]
    public async Task<ActionResult> GetUserRoles(long id)
    {
        var roleIds = await _userService.GetUserRoleIdsAsync(CurrentTenantId, id);
        return Success(new { RoleIds = roleIds });
    }

    /// <summary>
    /// 获取用户权限
    /// </summary>
    [HttpGet("{id}/permissions")]
    public async Task<ActionResult> GetUserPermissions(long id)
    {
        var permissions = await _userService.GetUserPermissionsAsync(CurrentTenantId, id);
        return Success(permissions);
    }

    /// <summary>
    /// 获取用户功能权限树
    /// </summary>
    [HttpGet("{id}/permissions/features")]
    public async Task<ActionResult> GetUserFeatureTree(long id)
    {
        var features = await _userService.GetUserFeatureTreeAsync(CurrentTenantId, id);
        return Success(features);
    }
}
