using Microsoft.AspNetCore.Authorization;

namespace PyraminxCube.Rbac.AspNetCore.Authorization
{
    /// <summary>
    /// API权限特性
    /// </summary>
    /// <remarks>
    /// 用法示例：
    /// <code>
    /// [ApiPermission("user:create")]
    /// public async Task&lt;IActionResult&gt; CreateUser(CreateUserRequest request)
    /// {
    ///     // ...
    /// }
    /// 
    /// // 或者使用自动推断（根据路由和HTTP方法）
    /// [ApiPermission]
    /// public async Task&lt;IActionResult&gt; CreateUser(CreateUserRequest request)
    /// {
    ///     // ...
    /// }
    /// </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ApiPermissionAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// API权限编码
        /// </summary>
        public string? ApiCode { get; }

        /// <summary>
        /// 是否自动推断权限编码（根据路由和HTTP方法）
        /// </summary>
        public bool AutoInfer { get; }

        /// <summary>
        /// 创建一个自动推断权限编码的特性
        /// </summary>
        public ApiPermissionAttribute() : base(ApiPermissionDefaults.PolicyName)
        {
            AutoInfer = true;
        }

        /// <summary>
        /// 创建一个指定权限编码的特性
        /// </summary>
        /// <param name="apiCode">API权限编码</param>
        public ApiPermissionAttribute(string apiCode) : base(ApiPermissionDefaults.PolicyName)
        {
            ApiCode = apiCode;
            AutoInfer = false;
        }
    }

    /// <summary>
    /// API权限默认值
    /// </summary>
    public static class ApiPermissionDefaults
    {
        /// <summary>
        /// 策略名称
        /// </summary>
        public const string PolicyName = "ApiPermission";
    }
}
