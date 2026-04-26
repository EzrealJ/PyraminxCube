namespace PyraminxCube.Applications.WebApp.Common;

/// <summary>
/// 统一 API 响应格式
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// 错误代码（失败时）
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResponse<T> Ok(T? data = default, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static ApiResponse<T> Fail(string message, string? errorCode = null)
        => new() { Success = false, Message = message, ErrorCode = errorCode };
}

/// <summary>
/// 无数据的 API 响应
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResponse Ok(string? message = null)
        => new() { Success = true, Message = message };

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public new static ApiResponse Fail(string message, string? errorCode = null)
        => new() { Success = false, Message = message, ErrorCode = errorCode };
}
