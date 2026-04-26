namespace PyraminxCube.Applications.WebApp.Common;

/// <summary>
/// 分页响应数据
/// </summary>
/// <typeparam name="T">数据项类型</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> Items { get; init; } = [];

    /// <summary>
    /// 总记录数
    /// </summary>
    public long TotalCount { get; init; }

    /// <summary>
    /// 当前页码（从1开始）
    /// </summary>
    public int PageIndex { get; init; }

    /// <summary>
    /// 每页条数
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;
}
