using System.Net;
using System.Text.Json;
using PyraminxCube.Applications.WebApp.Common;

namespace PyraminxCube.Applications.WebApp.Middleware;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "请求处理发生异常: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/json; charset=utf-8";

        var (statusCode, errorCode, message) = exception switch
        {
            ArgumentNullException argEx => (HttpStatusCode.BadRequest, "ARGUMENT_NULL", argEx.Message),
            ArgumentException argEx => (HttpStatusCode.BadRequest, "INVALID_ARGUMENT", argEx.Message),
            InvalidOperationException opEx => (HttpStatusCode.BadRequest, "INVALID_OPERATION", opEx.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", "未授权访问"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", "资源不存在"),
            OperationCanceledException => (HttpStatusCode.BadRequest, "CANCELLED", "操作已取消"),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "服务器内部错误")
        };

        response.StatusCode = (int)statusCode;

        var apiResponse = ApiResponse.Fail(message, errorCode);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsync(JsonSerializer.Serialize(apiResponse, options));
    }
}

/// <summary>
/// 中间件扩展
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    /// 使用全局异常处理中间件
    /// </summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        => builder.UseMiddleware<ExceptionHandlingMiddleware>();
}
