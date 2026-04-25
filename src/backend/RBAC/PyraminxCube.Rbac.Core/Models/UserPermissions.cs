namespace PyraminxCube.Rbac.Core.Models
{
    /// <summary>
    /// 用户权限信息（合并所有角色后的有效权限）
    /// </summary>
    public class UserPermissions
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        public long TenantId { get; set; }

        /// <summary>
        /// API权限编码列表
        /// </summary>
        public HashSet<string> ApiPermissions { get; set; } = [];

        /// <summary>
        /// 功能权限编码列表
        /// </summary>
        public HashSet<string> FeaturePermissions { get; set; } = [];

        /// <summary>
        /// 数据权限范围（按维度分组）
        /// Key: 维度编码，Value: 该维度下的权限范围ID列表
        /// </summary>
        public Dictionary<string, DataScopeValue> DataScopes { get; set; } = [];
    }

    /// <summary>
    /// 数据权限范围值
    /// </summary>
    public class DataScopeValue
    {
        /// <summary>
        /// 特殊标记：ALL-全部，SELF-仅自己，CUSTOM-自定义
        /// </summary>
        public DataScopeFlag Flag { get; set; } = DataScopeFlag.Custom;

        /// <summary>
        /// 具体的范围ID列表（当 Flag 为 Custom 时有效）
        /// </summary>
        public HashSet<long> ScopeIds { get; set; } = [];
    }

    /// <summary>
    /// 数据权限特殊标记
    /// </summary>
    public enum DataScopeFlag
    {
        /// <summary>
        /// 自定义（使用具体的 ScopeIds）
        /// </summary>
        Custom = 0,

        /// <summary>
        /// 全部数据
        /// </summary>
        All = 1,

        /// <summary>
        /// 仅自己创建的数据
        /// </summary>
        Self = 2
    }
}
