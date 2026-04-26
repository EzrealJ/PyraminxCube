using Microsoft.AspNetCore.Mvc;
using PyraminxCube.Applications.WebApp.Common;
using PyraminxCube.Rbac.Core.Models.Dto;
using PyraminxCube.Rbac.Core.Services;

namespace PyraminxCube.Applications.WebApp.Controllers;

/// <summary>
/// API 权限管理控制器
/// </summary>
public class ApiPermissionsController : BaseController
{
    private readonly IApiPermissionService _apiPermissionService;

    public ApiPermissionsController(IApiPermissionService apiPermissionService)
    {
        _apiPermissionService = apiPermissionService;
    }

    /// <summary>
    /// 获取 API 列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetList(
        [FromQuery] string? keyword,
        [FromQuery] string? httpMethod,
        [FromQuery] long? groupId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _apiPermissionService.GetPagedListAsync(new ApiPermissionQueryParams
        {
            Keyword = keyword,
            HttpMethod = httpMethod,
            GroupId = groupId,
            PageIndex = pageIndex,
            PageSize = pageSize
        });

        return Paged(result);
    }

    /// <summary>
    /// 获取所有 API（下拉选择用）
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult> GetAll()
    {
        var apis = await _apiPermissionService.GetAllAsync();
        return Success(apis);
    }

    /// <summary>
    /// 创建 API
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateApiPermissionRequest request)
    {
        var apiId = await _apiPermissionService.CreateAsync(request);
        return Success(new { Id = apiId }, "API 创建成功");
    }

    /// <summary>
    /// 更新 API
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(long id, [FromBody] UpdateApiPermissionRequest request)
    {
        if (id != request.Id)
        {
            return Fail("ID 不匹配");
        }

        await _apiPermissionService.UpdateAsync(request);
        return Success("API 更新成功");
    }

    /// <summary>
    /// 删除 API
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        await _apiPermissionService.DeleteAsync(id);
        return Success("API 删除成功");
    }

    /// <summary>
    /// 批量删除 API
    /// </summary>
    [HttpDelete("batch")]
    public async Task<ActionResult> DeleteBatch([FromBody] List<long> apiIds)
    {
        await _apiPermissionService.DeleteBatchAsync(apiIds);
        return Success("批量删除成功");
    }

    #region API 分组管理

    /// <summary>
    /// 获取 API 分组列表
    /// </summary>
    [HttpGet("groups")]
    public async Task<ActionResult> GetGroups()
    {
        var groups = await _apiPermissionService.GetGroupsAsync();
        return Success(groups);
    }

    /// <summary>
    /// 获取 API 分组及其下的 API 列表
    /// </summary>
    [HttpGet("groups-with-apis")]
    public async Task<ActionResult> GetGroupsWithApis()
    {
        var groups = await _apiPermissionService.GetGroupsWithApisAsync();
        return Success(groups);
    }

    /// <summary>
    /// 创建 API 分组
    /// </summary>
    [HttpPost("groups")]
    public async Task<ActionResult> CreateGroup([FromBody] CreateApiGroupRequest request)
    {
        var groupId = await _apiPermissionService.CreateGroupAsync(request);
        return Success(new { Id = groupId }, "分组创建成功");
    }

    /// <summary>
    /// 更新 API 分组
    /// </summary>
    [HttpPut("groups/{id}")]
    public async Task<ActionResult> UpdateGroup(long id, [FromBody] UpdateApiGroupRequest request)
    {
        if (id != request.Id)
        {
            return Fail("ID 不匹配");
        }

        await _apiPermissionService.UpdateGroupAsync(request);
        return Success("分组更新成功");
    }

    /// <summary>
    /// 删除 API 分组
    /// </summary>
    [HttpDelete("groups/{id}")]
    public async Task<ActionResult> DeleteGroup(long id)
    {
        var (canDelete, apiCount) = await _apiPermissionService.CheckGroupCanDeleteAsync(id);
        if (!canDelete)
        {
            return Fail($"该分组下有 {apiCount} 个 API，无法删除");
        }

        await _apiPermissionService.DeleteGroupAsync(id);
        return Success("分组删除成功");
    }

    /// <summary>
    /// 检查分组是否可删除
    /// </summary>
    [HttpGet("groups/{id}/deletable")]
    public async Task<ActionResult> CheckGroupDeletable(long id)
    {
        var (canDelete, apiCount) = await _apiPermissionService.CheckGroupCanDeleteAsync(id);
        return Success(new { CanDelete = canDelete, ApiCount = apiCount });
    }

    #endregion

    #region API 分组分配

    /// <summary>
    /// 将 API 分配到分组
    /// </summary>
    [HttpPatch("{id}/group")]
    public async Task<ActionResult> AssignToGroup(long id, [FromBody] AssignApiToGroupRequest request)
    {
        if (id != request.ApiId)
        {
            return Fail("ID 不匹配");
        }

        await _apiPermissionService.AssignToGroupAsync(request);
        return Success("API 分组分配成功");
    }

    /// <summary>
    /// 批量将 API 分配到分组
    /// </summary>
    [HttpPatch("batch/group")]
    public async Task<ActionResult> AssignBatchToGroup([FromBody] AssignBatchApiToGroupRequest request)
    {
        await _apiPermissionService.AssignBatchToGroupAsync(request);
        return Success("批量分组分配成功");
    }

    #endregion

    #region API 扫描

    /// <summary>
    /// 扫描 API（从代码注解）
    /// </summary>
    [HttpPost("scan")]
    public async Task<ActionResult> Scan()
    {
        var result = await _apiPermissionService.ScanFromCodeAsync();
        return Success(result);
    }

    /// <summary>
    /// 导入扫描的 API
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult> Import([FromBody] ImportScannedApisRequest request)
    {
        var result = await _apiPermissionService.ImportScannedApisAsync(request);
        return Success(result);
    }

    #endregion
}
