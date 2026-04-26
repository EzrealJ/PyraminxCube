using Microsoft.AspNetCore.Mvc;
using PyraminxCube.Applications.WebApp.Common;
using PyraminxCube.Rbac.Core.Models.Dto;
using PyraminxCube.Rbac.Core.Services;

namespace PyraminxCube.Applications.WebApp.Controllers;

/// <summary>
/// 数据维度管理控制器
/// </summary>
public class DataDimensionsController : BaseController
{
    private readonly IDataDimensionService _dataDimensionService;

    public DataDimensionsController(IDataDimensionService dataDimensionService)
    {
        _dataDimensionService = dataDimensionService;
    }

    #region 维度管理

    /// <summary>
    /// 获取维度列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetList(
        [FromQuery] string? dimensionCode,
        [FromQuery] string? dimensionName,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _dataDimensionService.GetPagedListAsync(new DataDimensionQueryParams
        {
            DimensionCode = dimensionCode,
            DimensionName = dimensionName,
            PageIndex = pageIndex,
            PageSize = pageSize
        });

        return Paged(result);
    }

    /// <summary>
    /// 获取所有维度（下拉选择用）
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult> GetAll()
    {
        var dimensions = await _dataDimensionService.GetAllListAsync();
        return Success(dimensions);
    }

    /// <summary>
    /// 获取维度详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(long id)
    {
        var dimension = await _dataDimensionService.GetByIdAsync(id);
        if (dimension == null)
        {
            return Fail("维度不存在");
        }
        return Success(dimension);
    }

    /// <summary>
    /// 创建维度
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateDataDimensionRequest request)
    {
        var dimensionId = await _dataDimensionService.CreateAsync(request);
        return Success(new { Id = dimensionId }, "维度创建成功");
    }

    /// <summary>
    /// 更新维度
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(long id, [FromBody] UpdateDataDimensionRequest request)
    {
        if (id != request.Id)
        {
            return Fail("ID 不匹配");
        }

        await _dataDimensionService.UpdateAsync(request);
        return Success("维度更新成功");
    }

    /// <summary>
    /// 删除维度
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        var result = await _dataDimensionService.DeleteAsync(id);
        if (result.IsInUse)
        {
            return Fail(result.WarningMessage ?? "维度正在使用中，无法删除");
        }
        return Success("维度删除成功");
    }

    /// <summary>
    /// 检查维度是否可删除
    /// </summary>
    [HttpGet("{id}/deletable")]
    public async Task<ActionResult> CheckDeletable(long id)
    {
        var result = await _dataDimensionService.CheckUsageAsync(id);
        return Success(result);
    }

    #endregion

    #region 维度值管理

    /// <summary>
    /// 获取维度值列表
    /// </summary>
    [HttpGet("{id}/scopes")]
    public async Task<ActionResult> GetScopes(
        long id,
        [FromQuery] string? scopeCode,
        [FromQuery] string? scopeName,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _dataDimensionService.GetScopePagedListAsync(new DataScopeQueryParams
        {
            DimensionId = id,
            ScopeCode = scopeCode,
            ScopeName = scopeName,
            PageIndex = pageIndex,
            PageSize = pageSize
        });

        return Paged(result);
    }

    /// <summary>
    /// 获取维度值详情
    /// </summary>
    [HttpGet("scopes/{scopeId}")]
    public async Task<ActionResult> GetScopeById(long scopeId)
    {
        var scope = await _dataDimensionService.GetScopeByIdAsync(scopeId);
        if (scope == null)
        {
            return Fail("维度值不存在");
        }
        return Success(scope);
    }

    /// <summary>
    /// 创建维度值
    /// </summary>
    [HttpPost("{id}/scopes")]
    public async Task<ActionResult> CreateScope(long id, [FromBody] CreateDataScopeRequest request)
    {
        if (id != request.DimensionId)
        {
            return Fail("ID 不匹配");
        }

        var scopeId = await _dataDimensionService.CreateScopeAsync(request);
        return Success(new { Id = scopeId }, "维度值创建成功");
    }

    /// <summary>
    /// 更新维度值
    /// </summary>
    [HttpPut("scopes/{scopeId}")]
    public async Task<ActionResult> UpdateScope(long scopeId, [FromBody] UpdateDataScopeRequest request)
    {
        if (scopeId != request.Id)
        {
            return Fail("ID 不匹配");
        }

        await _dataDimensionService.UpdateScopeAsync(request);
        return Success("维度值更新成功");
    }

    /// <summary>
    /// 删除维度值
    /// </summary>
    [HttpDelete("scopes/{scopeId}")]
    public async Task<ActionResult> DeleteScope(long scopeId)
    {
        var result = await _dataDimensionService.DeleteScopeAsync(scopeId);
        if (result.IsInUse)
        {
            return Fail(result.WarningMessage ?? "维度值正在使用中，无法删除");
        }
        return Success("维度值删除成功");
    }

    /// <summary>
    /// 检查维度值是否可删除
    /// </summary>
    [HttpGet("scopes/{scopeId}/deletable")]
    public async Task<ActionResult> CheckScopeDeletable(long scopeId)
    {
        var result = await _dataDimensionService.CheckScopeUsageAsync(scopeId);
        return Success(result);
    }

    #endregion

    #region 维度值树管理

    /// <summary>
    /// 获取维度值树形结构
    /// </summary>
    [HttpGet("{id}/scopes/tree")]
    public async Task<ActionResult> GetScopeTree(long id)
    {
        var tree = await _dataDimensionService.GetScopeTreeAsync(id);
        return Success(tree);
    }

    /// <summary>
    /// 移动维度值
    /// </summary>
    [HttpPatch("scopes/{scopeId}/move")]
    public async Task<ActionResult> MoveScope(long scopeId, [FromBody] MoveDataScopeRequest request)
    {
        if (scopeId != request.ScopeId)
        {
            return Fail("ID 不匹配");
        }

        await _dataDimensionService.MoveScopeAsync(request);
        return Success("移动成功");
    }

    /// <summary>
    /// 批量更新维度值排序
    /// </summary>
    [HttpPatch("{id}/scopes/sort")]
    public async Task<ActionResult> UpdateScopeSortOrders(long id, [FromBody] Dictionary<long, int> sortOrders)
    {
        await _dataDimensionService.UpdateScopeSortOrdersAsync(id, sortOrders);
        return Success("排序更新成功");
    }

    #endregion

    #region 维度映射配置

    /// <summary>
    /// 获取维度映射列表
    /// </summary>
    [HttpGet("{id}/mappings")]
    public async Task<ActionResult> GetMappings(long id, [FromQuery] string? tableName)
    {
        var mappings = await _dataDimensionService.GetMappingListAsync(new DataDimensionMappingQueryParams
        {
            DimensionId = id,
            TableName = tableName
        });

        return Success(mappings);
    }

    /// <summary>
    /// 获取维度映射详情
    /// </summary>
    [HttpGet("mappings/{mappingId}")]
    public async Task<ActionResult> GetMappingById(long mappingId)
    {
        var mapping = await _dataDimensionService.GetMappingByIdAsync(mappingId);
        if (mapping == null)
        {
            return Fail("映射不存在");
        }
        return Success(mapping);
    }

    /// <summary>
    /// 创建维度映射
    /// </summary>
    [HttpPost("{id}/mappings")]
    public async Task<ActionResult> CreateMapping(long id, [FromBody] CreateDataDimensionMappingRequest request)
    {
        if (id != request.DimensionId)
        {
            return Fail("ID 不匹配");
        }

        var mappingId = await _dataDimensionService.CreateMappingAsync(request);
        return Success(new { Id = mappingId }, "映射创建成功");
    }

    /// <summary>
    /// 更新维度映射
    /// </summary>
    [HttpPut("mappings/{mappingId}")]
    public async Task<ActionResult> UpdateMapping(long mappingId, [FromBody] UpdateDataDimensionMappingRequest request)
    {
        if (mappingId != request.Id)
        {
            return Fail("ID 不匹配");
        }

        await _dataDimensionService.UpdateMappingAsync(request);
        return Success("映射更新成功");
    }

    /// <summary>
    /// 删除维度映射
    /// </summary>
    [HttpDelete("mappings/{mappingId}")]
    public async Task<ActionResult> DeleteMapping(long mappingId)
    {
        await _dataDimensionService.DeleteMappingAsync(mappingId);
        return Success("映射删除成功");
    }

    #endregion
}
