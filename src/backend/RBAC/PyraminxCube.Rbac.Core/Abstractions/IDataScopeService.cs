using PyraminxCube.Rbac.Core.Models;

namespace PyraminxCube.Rbac.Core.Abstractions
{
    /// <summary>
    /// 数据权限服务接口
    /// </summary>
    public interface IDataScopeService
    {
        /// <summary>
        /// 获取当前用户在指定维度的数据权限范围
        /// </summary>
        /// <param name="dimensionCode">维度编码，如 "DEPARTMENT"、"REGION"</param>
        /// <returns>数据权限范围值</returns>
        DataScopeValue GetCurrentUserDataScope(string dimensionCode);

        /// <summary>
        /// 获取当前用户的所有数据权限范围
        /// </summary>
        /// <returns>所有维度的数据权限范围</returns>
        Dictionary<string, DataScopeValue> GetCurrentUserDataScopes();

        /// <summary>
        /// 获取指定维度与实体类型的字段映射
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="dimensionCode">维度编码</param>
        /// <returns>实体属性名，如果未配置则返回null</returns>
        string? GetDimensionPropertyName<TEntity>(string dimensionCode) where TEntity : class;

        /// <summary>
        /// 获取实体类型配置的所有维度映射
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>维度编码与属性名的映射字典</returns>
        Dictionary<string, string> GetEntityDimensionMappings<TEntity>() where TEntity : class;
    }
}
