using Microsoft.AspNetCore.Mvc;
using PyraminxCube.Applications.WebApp.Common;
using PyraminxCube.Rbac.Core.Services;

namespace PyraminxCube.Applications.WebApp.Controllers;

/// <summary>
/// 功能权限管理控制器
/// </summary>
public class FeaturePermissionsController : BaseController
{
    private readonly IFeaturePermissionService _featurePermissionService;

    public FeaturePermissionsController(IFeaturePermissionService featurePermissionService)
    {
        _featurePermissionService = featurePermissionService;
    }

    /// <summary>
    /// 获取功能权限树
    /// </summary>
    [HttpGet("tree")]
    public async Task<ActionResult> GetTree()
    {
        var tree = await _featurePermissionService.GetTreeAsync();
        return Success(tree);
    }

    /// <summary>
    /// 获取启用的功能权限树
    /// </summary>
    [HttpGet("tree/enabled")]
    public async Task<ActionResult> GetEnabledTree()
    {
        var tree = await _featurePermissionService.GetEnabledTreeAsync();
        return Success(tree);
    }

    /// <summary>
    /// 获取所有功能权限（平铺列表）
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult> GetAll()
    {
        var features = await _featurePermissionService.GetAllAsync();
        return Success(features);
    }

    /// <summary>
    /// 创建功能节点
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateFeaturePermissionRequest request)
    {
        var featureId = await _featurePermissionService.CreateAsync(request);
        return Success(new { Id = featureId }, "功能节点创建成功");
    }

    /// <summary>
    /// 更新功能节点
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(long id, [FromBody] UpdateFeaturePermissionRequest request)
    {
        if (id != request.Id)
        {
            return Fail("ID 不匹配");
        }

        await _featurePermissionService.UpdateAsync(request);
        return Success("功能节点更新成功");
    }

    /// <summary>
    /// 删除功能节点
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        var (canDelete, childCount) = await _featurePermissionService.CheckCanDeleteAsync(id);
        if (!canDelete)
        {
            return Fail($"该节点下有 {childCount} 个子节点，无法删除");
        }

        await _featurePermissionService.DeleteAsync(id);
        return Success("功能节点删除成功");
    }

    /// <summary>
    /// 检查节点是否可删除
    /// </summary>
    [HttpGet("{id}/deletable")]
    public async Task<ActionResult> CheckDeletable(long id)
    {
        var (canDelete, childCount) = await _featurePermissionService.CheckCanDeleteAsync(id);
        return Success(new { CanDelete = canDelete, ChildCount = childCount });
    }

    /// <summary>
    /// 调整节点顺序
    /// </summary>
    [HttpPatch("{id}/sort")]
    public async Task<ActionResult> UpdateSortOrder(long id, [FromBody] UpdateFeatureSortOrderRequest request)
    {
        if (id != request.Id)
        {
            return Fail("ID 不匹配");
        }

        await _featurePermissionService.UpdateSortOrderAsync(request);
        return Success("顺序调整成功");
    }

    /// <summary>
    /// 批量调整节点顺序
    /// </summary>
    [HttpPatch("sort/batch")]
    public async Task<ActionResult> UpdateSortOrderBatch([FromBody] List<UpdateFeatureSortOrderRequest> requests)
    {
        await _featurePermissionService.UpdateSortOrderBatchAsync(requests);
        return Success("批量顺序调整成功");
    }

    #region API 关联

    /// <summary>
    /// 获取功能按钮关联的 API 列表
    /// </summary>
    [HttpGet("{id}/apis")]
    public async Task<ActionResult> GetAssignedApis(long id)
    {
        var apis = await _featurePermissionService.GetAssignedApisAsync(id);
        return Success(apis);
    }

    /// <summary>
    /// 为功能按钮关联 API
    /// </summary>
    [HttpPost("{id}/apis")]
    public async Task<ActionResult> AssignApi(long id, [FromBody] AssignApiToFeatureRequest request)
    {
        if (id != request.FeatureId)
        {
            return Fail("ID 不匹配");
        }

        await _featurePermissionService.AssignApiAsync(request);
        return Success("API 关联成功");
    }

    /// <summary>
    /// 移除功能按钮的 API 关联
    /// </summary>
    [HttpDelete("{id}/apis/{apiId}")]
    public async Task<ActionResult> RemoveApi(long id, long apiId)
    {
        await _featurePermissionService.RemoveApiAsync(id, apiId);
        return Success("API 关联移除成功");
    }

    /// <summary>
    /// 批量设置功能按钮与 API 的关联
    /// </summary>
    [HttpPut("{id}/apis")]
    public async Task<ActionResult> AssignBatchApis(long id, [FromBody] BatchAssignApisToFeaturesRequest request)
    {
        if (id != request.FeatureId)
        {
            return Fail("ID 不匹配");
        }

        await _featurePermissionService.AssignBatchApisAsync(request);
        return Success("批量 API 关联成功");
    }

    /// <summary>
    /// 清除功能按钮的所有 API 关联
    /// </summary>
    [HttpDelete("{id}/apis")]
    public async Task<ActionResult> ClearAllApis(long id)
    {
        await _featurePermissionService.ClearAllApisAsync(id);
        return Success("所有 API 关联已清除");
    }

    #endregion
}
