namespace PyraminxCube.Rbac.Core.Abstractions
{
    /// <summary>
    /// 数据权限维度映射配置接口
    /// </summary>
    public interface IDataDimensionMapping
    {
        /// <summary>
        /// 注册实体类型与维度的映射关系
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="dimensionCode">维度编码</param>
        /// <param name="propertyName">实体属性名</param>
        void Register<TEntity>(string dimensionCode, string propertyName) where TEntity : class;

        /// <summary>
        /// 获取实体类型的维度映射
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <returns>维度编码与属性名的映射字典</returns>
        Dictionary<string, string> GetMappings(Type entityType);
    }

    /// <summary>
    /// 数据权限维度映射配置的默认实现
    /// </summary>
    public class DataDimensionMapping : IDataDimensionMapping
    {
        private readonly Dictionary<Type, Dictionary<string, string>> _mappings = [];

        public void Register<TEntity>(string dimensionCode, string propertyName) where TEntity : class
        {
            var entityType = typeof(TEntity);
            if (!_mappings.TryGetValue(entityType, out var dimensionMappings))
            {
                dimensionMappings = [];
                _mappings[entityType] = dimensionMappings;
            }
            dimensionMappings[dimensionCode] = propertyName;
        }

        public Dictionary<string, string> GetMappings(Type entityType)
        {
            return _mappings.TryGetValue(entityType, out var mappings)
                ? new Dictionary<string, string>(mappings)
                : [];
        }
    }
}
