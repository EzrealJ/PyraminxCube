using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using PyraminxCube.Rbac.Core.Abstractions;
using PyraminxCube.Rbac.Core.Models;

namespace PyraminxCube.Rbac.Core.Extensions
{
    /// <summary>
    /// 数据权限过滤扩展方法
    /// </summary>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// // 应用所有维度的数据权限过滤
    /// var orders = await _dbContext.Orders
    ///     .WithDataScope(dataScopeService, currentUser)
    ///     .Where(o => o.Status == "pending")
    ///     .ToListAsync();
    /// 
    /// // 只应用指定维度
    /// var orders = await _dbContext.Orders
    ///     .WithDataScope(dataScopeService, currentUser, "DEPARTMENT")
    ///     .ToListAsync();
    /// 
    /// // 不应用数据权限（特殊场景）
    /// var allOrders = await _dbContext.Orders.ToListAsync();
    /// </code>
    /// </remarks>
    public static class DataScopeExtensions
    {
        /// <summary>
        /// 应用当前用户的数据权限过滤（所有维度）
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="query">查询对象</param>
        /// <param name="dataScopeService">数据权限服务</param>
        /// <param name="currentUser">当前用户</param>
        /// <returns>过滤后的查询对象</returns>
        public static IQueryable<TEntity> WithDataScope<TEntity>(
            this IQueryable<TEntity> query,
            IDataScopeService dataScopeService,
            ICurrentUser currentUser)
            where TEntity : class
        {
            // 超级管理员跳过数据权限过滤
            if (currentUser.IsSuperAdmin || !currentUser.IsAuthenticated)
            {
                return query;
            }

            var mappings = dataScopeService.GetEntityDimensionMappings<TEntity>();
            if (mappings.Count == 0)
            {
                return query;
            }

            var dataScopes = dataScopeService.GetCurrentUserDataScopes();

            foreach (var mapping in mappings)
            {
                var dimensionCode = mapping.Key;
                var propertyName = mapping.Value;

                if (dataScopes.TryGetValue(dimensionCode, out var scopeValue))
                {
                    query = ApplyDimensionFilter(query, propertyName, scopeValue, currentUser.UserId);
                }
            }

            return query;
        }

        /// <summary>
        /// 应用当前用户指定维度的数据权限过滤
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="query">查询对象</param>
        /// <param name="dataScopeService">数据权限服务</param>
        /// <param name="currentUser">当前用户</param>
        /// <param name="dimensionCodes">要过滤的维度编码列表</param>
        /// <returns>过滤后的查询对象</returns>
        public static IQueryable<TEntity> WithDataScope<TEntity>(
            this IQueryable<TEntity> query,
            IDataScopeService dataScopeService,
            ICurrentUser currentUser,
            params string[] dimensionCodes)
            where TEntity : class
        {
            if (dimensionCodes.Length == 0)
            {
                return WithDataScope(query, dataScopeService, currentUser);
            }

            // 超级管理员跳过数据权限过滤
            if (currentUser.IsSuperAdmin || !currentUser.IsAuthenticated)
            {
                return query;
            }

            var mappings = dataScopeService.GetEntityDimensionMappings<TEntity>();
            if (mappings.Count == 0)
            {
                return query;
            }

            var dataScopes = dataScopeService.GetCurrentUserDataScopes();

            foreach (var dimensionCode in dimensionCodes)
            {
                if (mappings.TryGetValue(dimensionCode, out var propertyName) &&
                    dataScopes.TryGetValue(dimensionCode, out var scopeValue))
                {
                    query = ApplyDimensionFilter(query, propertyName, scopeValue, currentUser.UserId);
                }
            }

            return query;
        }

        /// <summary>
        /// 应用单个维度的过滤条件
        /// </summary>
        private static IQueryable<TEntity> ApplyDimensionFilter<TEntity>(
            IQueryable<TEntity> query,
            string propertyName,
            DataScopeValue scopeValue,
            long? currentUserId)
            where TEntity : class
        {
            // ALL 标记：不过滤
            if (scopeValue.Flag == DataScopeFlag.All)
            {
                return query;
            }

            // SELF 标记：只看自己创建的数据
            if (scopeValue.Flag == DataScopeFlag.Self)
            {
                return ApplySelfFilter(query, currentUserId);
            }

            // CUSTOM：根据具体的 ScopeIds 过滤
            if (scopeValue.ScopeIds.Count == 0)
            {
                // 没有配置任何权限，返回空结果
                return query.Where(_ => false);
            }

            return ApplyContainsFilter(query, propertyName, scopeValue.ScopeIds);
        }

        /// <summary>
        /// 应用"仅自己"过滤（基于 CreateUserId 字段）
        /// </summary>
        private static IQueryable<TEntity> ApplySelfFilter<TEntity>(
            IQueryable<TEntity> query,
            long? currentUserId)
            where TEntity : class
        {
            if (!currentUserId.HasValue)
            {
                return query.Where(_ => false);
            }

            var entityType = typeof(TEntity);
            var createUserIdProperty = entityType.GetProperty("CreateUserId");

            if (createUserIdProperty == null)
            {
                // 实体没有 CreateUserId 字段，跳过此过滤
                return query;
            }

            // 构建: x => x.CreateUserId == currentUserId
            var parameter = Expression.Parameter(entityType, "x");
            var property = Expression.Property(parameter, createUserIdProperty);
            var constant = Expression.Constant(currentUserId.Value, typeof(long));

            Expression comparison;
            if (createUserIdProperty.PropertyType == typeof(long))
            {
                comparison = Expression.Equal(property, constant);
            }
            else if (createUserIdProperty.PropertyType == typeof(long?))
            {
                comparison = Expression.Equal(property, Expression.Convert(constant, typeof(long?)));
            }
            else
            {
                // CreateUserId 类型不兼容，跳过此过滤
                return query;
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
            return query.Where(lambda);
        }

        /// <summary>
        /// 应用 Contains 过滤（基于指定属性）
        /// </summary>
        private static IQueryable<TEntity> ApplyContainsFilter<TEntity>(
            IQueryable<TEntity> query,
            string propertyName,
            HashSet<long> scopeIds)
            where TEntity : class
        {
            var entityType = typeof(TEntity);
            var property = entityType.GetProperty(propertyName);

            if (property == null)
            {
                return query;
            }

            // 构建: x => scopeIds.Contains(x.PropertyName)
            var parameter = Expression.Parameter(entityType, "x");
            var propertyAccess = Expression.Property(parameter, property);

            // 处理可空类型
            Expression valueExpression = propertyAccess;
            if (property.PropertyType == typeof(long?))
            {
                valueExpression = Expression.Property(propertyAccess, "Value");
            }
            else if (property.PropertyType != typeof(long))
            {
                // 属性类型不兼容，跳过此过滤
                return query;
            }

            var scopeIdsList = scopeIds.ToList();
            var containsMethod = typeof(List<long>).GetMethod("Contains", [typeof(long)])!;
            var containsCall = Expression.Call(
                Expression.Constant(scopeIdsList),
                containsMethod,
                valueExpression);

            // 如果是可空类型，需要加上 HasValue 检查
            Expression body;
            if (property.PropertyType == typeof(long?))
            {
                var hasValue = Expression.Property(propertyAccess, "HasValue");
                body = Expression.AndAlso(hasValue, containsCall);
            }
            else
            {
                body = containsCall;
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
            return query.Where(lambda);
        }
    }
}
