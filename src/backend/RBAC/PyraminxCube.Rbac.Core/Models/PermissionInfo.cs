namespace PyraminxCube.Rbac.Core.Models
{
    /// <summary>
    /// API权限信息
    /// </summary>
    public class ApiPermissionInfo
    {
        /// <summary>
        /// API编码
        /// </summary>
        public string ApiCode { get; set; } = string.Empty;

        /// <summary>
        /// API名称
        /// </summary>
        public string ApiName { get; set; } = string.Empty;

        /// <summary>
        /// 接口路径
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// HTTP方法
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// 功能权限信息
    /// </summary>
    public class FeaturePermissionInfo
    {
        /// <summary>
        /// 功能编码
        /// </summary>
        public string FeatureCode { get; set; } = string.Empty;

        /// <summary>
        /// 功能名称
        /// </summary>
        public string FeatureName { get; set; } = string.Empty;

        /// <summary>
        /// 功能类型
        /// </summary>
        public FeatureType FeatureType { get; set; }

        /// <summary>
        /// 父级编码
        /// </summary>
        public string? ParentCode { get; set; }

        /// <summary>
        /// 前端路由路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 子级功能列表
        /// </summary>
        public List<FeaturePermissionInfo> Children { get; set; } = [];
    }

    /// <summary>
    /// 功能类型
    /// </summary>
    public enum FeatureType
    {
        /// <summary>
        /// 模块
        /// </summary>
        Module = 1,

        /// <summary>
        /// 页面
        /// </summary>
        Page = 2,

        /// <summary>
        /// 按钮
        /// </summary>
        Button = 3
    }
}
