using Microsoft.AspNetCore.Mvc;
using PyraminxCube.Applications.WebApp.Common;

namespace PyraminxCube.Applications.WebApp.Controllers;

/// <summary>
/// 基础控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// 返回成功响应
    /// </summary>
    protected ActionResult Success<T>(T? data = default, string? message = null)
    {
        var response = ApiResponse<T>.Ok(data, message);
        return Ok(response);
    }

    /// <summary>
    /// 返回成功响应（无数据）
    /// </summary>
    protected ActionResult Success(string? message = null)
    {
        var response = ApiResponse.Ok(message);
        return Ok(response);
    }

    /// <summary>
    /// 返回失败响应
    /// </summary>
    protected ActionResult Fail<T>(string message, string? errorCode = null)
    {
        var response = ApiResponse<T>.Fail(message, errorCode);
        return BadRequest(response);
    }

    /// <summary>
    /// 返回失败响应（无数据）
    /// </summary>
    protected ActionResult Fail(string message, string? errorCode = null)
    {
        var response = ApiResponse.Fail(message, errorCode);
        return BadRequest(response);
    }

    /// <summary>
    /// 返回分页响应
    /// </summary>
    protected ActionResult Paged<T>(List<T> items, long totalCount, int pageIndex, int pageSize)
    {
        var pagedData = new PagedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
        var response = ApiResponse<PagedResponse<T>>.Ok(pagedData);
        return Ok(response);
    }

    /// <summary>
    /// 从 Core.Models.Dto.PagedResult 转换为 PagedResponse
    /// </summary>
    protected ActionResult Paged<T>(PyraminxCube.Rbac.Core.Models.Dto.PagedResult<T> result)
        => Paged(result.Items, result.TotalCount, result.PageIndex, result.PageSize);

    /// <summary>
    /// 获取当前租户ID（从请求头或用户上下文中获取）
    /// </summary>
    protected long CurrentTenantId
    {
        get
        {
            // 尝试从请求头获取
            if (Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValue)
                && long.TryParse(tenantIdValue, out var tenantId))
            {
                return tenantId;
            }

            // 默认返回 1（开发环境）
            // 生产环境应该从认证信息中获取
            return 1;
        }
    }
}
