# RBAC C# 实现选型

> 本文档作为使用 C# 实现 RBAC 权限库的技术选型索引

---

## 1. 技术栈概览

```
┌─────────────────────────────────────────────────────────────┐
│                    RBAC 库技术栈                            │
├─────────────────────────────────────────────────────────────┤
│  目标框架    │  .NET 10（不支持 .NET Standard）             │
├─────────────────────────────────────────────────────────────┤
│  数据访问    │  Entity Framework Core + Dapper             │
├─────────────────────────────────────────────────────────────┤
│  API权限     │  ASP.NET Core Authorization                 │
│             │  - IAuthorizationHandler                     │
│             │  - Policy-based Authorization                │
├─────────────────────────────────────────────────────────────┤
│  数据权限    │  显式调用扩展方法（非自动注入）               │
│             │  - IQueryable 扩展方法                       │
│             │  - 表达式树动态拼接                          │
├─────────────────────────────────────────────────────────────┤
│  缓存       │  IMemoryCache / IDistributedCache            │
│             │  StackExchange.Redis（分布式场景）           │
├─────────────────────────────────────────────────────────────┤
│  依赖注入    │  Microsoft.Extensions.DependencyInjection   │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. 核心框架选择

### 2.1 目标框架：.NET 10

| 选择 | 理由 |
|------|------|
| ✅ .NET 10 | LTS 版本，长期支持，性能最优 |
| ❌ .NET Standard 2.1 | 不再支持，减少维护成本 |

**说明**：直接基于 .NET 10，不考虑向下兼容老版本，保持代码简洁。

### 2.2 依赖注入

使用 `Microsoft.Extensions.DependencyInjection`，这是 .NET 官方标准，所有 ASP.NET Core 项目都能无缝集成。

---

## 3. 数据访问层

### 3.1 主力：Entity Framework Core

| 用途 | 说明 |
|------|------|
| 基础 CRUD | 权限表的增删改查 |
| 复杂查询 | 权限关联查询 |
| 数据权限过滤 | 通过扩展方法动态拼接表达式 |

### 3.2 辅助：Dapper

| 用途 | 说明 |
|------|------|
| 性能敏感查询 | 权限校验时的高频查询 |
| 复杂 SQL | EF Core 不好表达的场景 |

### 3.3 数据权限实现方式

**核心原则：显式调用，非自动注入**

```csharp
// ❌ 不采用：Global Query Filter（自动附加，可能帮倒忙）
modelBuilder.Entity<Order>()
    .HasQueryFilter(o => ...);  // 不用这种方式

// ✅ 采用：显式调用扩展方法
var orders = await _dbContext.Orders
    .WithDataScope()  // 明确：这里控制了数据权限
    .Where(o => o.Status == "pending")
    .ToListAsync();

// ✅ 可以选择不控制
var allOrders = await _dbContext.Orders.ToListAsync();  // 不加就是不过滤
```

---

## 4. API 权限实现

### 4.1 基于 ASP.NET Core 内置授权框架

| 组件 | 用途 |
|------|------|
| `IAuthorizationHandler` | 自定义权限校验逻辑 |
| `IAuthorizationPolicyProvider` | 动态策略提供 |
| `AuthorizeAttribute` | 声明式权限标记 |

### 4.2 自定义特性

```csharp
/// <summary>
/// API 权限特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiPermissionAttribute : AuthorizeAttribute
{
    public ApiPermissionAttribute(string permissionCode)
    {
        Policy = $"ApiPermission:{permissionCode}";
    }
}

// 使用方式
[ApiPermission("user:list")]
[HttpGet("/api/users")]
public IActionResult GetUsers() { }

[ApiPermission("user:create")]
[HttpPost("/api/users")]
public IActionResult CreateUser() { }
```

### 4.3 权限校验 Handler

```csharp
public class ApiPermissionHandler : AuthorizationHandler<ApiPermissionRequirement>
{
    private readonly IPermissionService _permissionService;
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApiPermissionRequirement requirement)
    {
        var userId = context.User.GetUserId();
        var hasPermission = await _permissionService
            .HasApiPermissionAsync(userId, requirement.PermissionCode);
        
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
```

---

## 5. 数据权限实现

### 5.1 核心原则

```
┌─────────────────────────────────────────────────────────────┐
│                    数据权限设计原则                          │
├─────────────────────────────────────────────────────────────┤
│  显式调用    │  调用 .WithDataScope() 才过滤               │
│  代码可读    │  一眼看出"这里控制了数据权限"                │
│  灵活自由    │  不调用就是不过滤，特殊场景可绕过            │
│  避免意外    │  不自动附加，避免在复杂业务中"帮倒忙"        │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 扩展方法设计

```csharp
public static class DataScopeExtensions
{
    /// <summary>
    /// 应用当前用户的数据权限过滤（所有维度）
    /// </summary>
    public static IQueryable<T> WithDataScope<T>(this IQueryable<T> query) 
        where T : class
    {
        var scopeService = GetDataScopeService();
        return scopeService.Apply(query);
    }
    
    /// <summary>
    /// 应用指定维度的数据权限过滤
    /// </summary>
    public static IQueryable<T> WithDataScope<T>(
        this IQueryable<T> query, 
        params string[] dimensions) where T : class
    {
        var scopeService = GetDataScopeService();
        return scopeService.Apply(query, dimensions);
    }
}
```

### 5.3 使用示例

```csharp
// 应用所有维度的数据权限
var orders = await _dbContext.Orders
    .WithDataScope()
    .Where(o => o.Status == OrderStatus.Pending)
    .ToListAsync();

// 只应用部门维度
var orders = await _dbContext.Orders
    .WithDataScope("DEPARTMENT")
    .ToListAsync();

// 不应用数据权限（特殊场景）
var allOrders = await _dbContext.Orders.ToListAsync();
```

### 5.4 内部实现思路

```csharp
public class DataScopeService : IDataScopeService
{
    private readonly ICurrentUser _currentUser;
    private readonly IDataScopeMappingProvider _mappingProvider;
    
    public IQueryable<T> Apply<T>(IQueryable<T> query, params string[] dimensions) 
        where T : class
    {
        // 1. 获取当前用户的数据权限范围
        var userScopes = _currentUser.GetDataScopes(dimensions);
        
        // 2. 获取实体类型与维度的映射配置
        var mappings = _mappingProvider.GetMappings<T>();
        
        // 3. 动态构建表达式树
        foreach (var mapping in mappings)
        {
            if (dimensions.Length == 0 || dimensions.Contains(mapping.DimensionCode))
            {
                var scopeValues = userScopes[mapping.DimensionCode];
                query = query.Where(BuildContainsExpression<T>(mapping.PropertyName, scopeValues));
            }
        }
        
        return query;
    }
    
    private Expression<Func<T, bool>> BuildContainsExpression<T>(
        string propertyName, 
        IEnumerable<object> values)
    {
        // 动态构建 x => values.Contains(x.PropertyName)
        // ...
    }
}
```

---

## 6. 缓存策略

### 6.1 缓存接口抽象

```csharp
public interface IPermissionCache
{
    Task<UserPermissions?> GetUserPermissionsAsync(long userId);
    Task SetUserPermissionsAsync(long userId, UserPermissions permissions);
    Task InvalidateAsync(long userId);
    Task InvalidateAllAsync();
}
```

### 6.2 实现选择

| 场景 | 实现 | NuGet 包 |
|------|------|---------|
| 单机/小规模 | `MemoryPermissionCache` | Microsoft.Extensions.Caching.Memory |
| 分布式/集群 | `RedisPermissionCache` | StackExchange.Redis |

---

## 7. 库结构设计

```
YourRbac/
├── src/
│   ├── YourRbac.Core/                    # 核心抽象层
│   │   ├── Abstractions/                 # 接口定义
│   │   │   ├── IPermissionService.cs
│   │   │   ├── IDataScopeService.cs
│   │   │   ├── IPermissionCache.cs
│   │   │   └── ICurrentUser.cs
│   │   ├── Models/                       # 权限模型
│   │   │   ├── UserPermissions.cs
│   │   │   ├── ApiPermission.cs
│   │   │   ├── FeaturePermission.cs
│   │   │   └── DataScope.cs
│   │   └── Extensions/                   # 扩展方法
│   │       └── DataScopeExtensions.cs
│   │
│   ├── YourRbac.EntityFrameworkCore/     # EF Core 实现
│   │   ├── DbContext/
│   │   │   └── RbacDbContext.cs
│   │   ├── Entities/                     # 数据库实体
│   │   ├── Repositories/                 # 仓储实现
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   ├── YourRbac.AspNetCore/              # ASP.NET Core 集成
│   │   ├── Authorization/                # 授权相关
│   │   │   ├── ApiPermissionAttribute.cs
│   │   │   ├── ApiPermissionHandler.cs
│   │   │   └── ApiPermissionPolicyProvider.cs
│   │   ├── Middleware/
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   └── YourRbac.Caching.Redis/           # Redis 缓存实现（可选）
│       ├── RedisPermissionCache.cs
│       └── Extensions/
│           └── ServiceCollectionExtensions.cs
│
└── samples/
    └── YourRbac.Sample/                  # 示例项目
```

---

## 8. 使用者集成方式

### 8.1 安装 NuGet 包

```bash
dotnet add package YourRbac.EntityFrameworkCore
dotnet add package YourRbac.AspNetCore
dotnet add package YourRbac.Caching.Redis  # 可选
```

### 8.2 配置服务

```csharp
// Program.cs
builder.Services.AddYourRbac(options =>
{
    // 使用 EF Core
    options.UseEntityFrameworkCore<AppDbContext>();
    
    // 缓存配置（二选一）
    options.UseMemoryCache();  // 内存缓存
    // options.UseRedisCache(connectionString);  // Redis 缓存
});

// 配置授权
builder.Services.AddAuthorization();
```

### 8.3 启用中间件

```csharp
app.UseAuthentication();
app.UseAuthorization();  // ASP.NET Core 内置，会自动调用我们的 Handler
```

### 8.4 使用权限控制

```csharp
// API 权限（自动拦截）
[ApiPermission("user:list")]
[HttpGet("/api/users")]
public async Task<IActionResult> GetUsers()
{
    // 数据权限（显式调用）
    var users = await _dbContext.Users
        .WithDataScope()  // 明确控制数据权限
        .ToListAsync();
    
    return Ok(users);
}

// 不需要数据权限的场景
[ApiPermission("user:export-all")]
[HttpGet("/api/users/export-all")]
public async Task<IActionResult> ExportAllUsers()
{
    // 不调用 WithDataScope()，就是不过滤
    var allUsers = await _dbContext.Users.ToListAsync();
    return Ok(allUsers);
}
```

---

## 9. 可参考的开源项目

| 项目 | 参考价值 | 链接 |
|------|---------|------|
| **Casbin.NET** | 权限模型抽象、策略匹配 | https://github.com/casbin/Casbin.NET |
| **ASP.NET Core Identity** | 用户/角色管理 | 官方内置 |
| **ABP Framework** | 权限系统设计、模块化 | https://github.com/abpframework/abp |
| **SqlSugar** | 数据权限过滤思路 | https://github.com/DotNetNext/SqlSugar |

---

## 10. 开发路线图

### Phase 1：核心功能
- [ ] Core 层接口定义
- [ ] EF Core 实现（数据库实体、仓储）
- [ ] 基础权限服务（用户、角色、权限 CRUD）

### Phase 2：ASP.NET Core 集成
- [ ] API 权限特性和 Handler
- [ ] 动态策略提供器
- [ ] 服务注册扩展方法

### Phase 3：数据权限
- [ ] DataScope 扩展方法
- [ ] 表达式树动态构建
- [ ] 维度映射配置

### Phase 4：缓存与优化
- [ ] 内存缓存实现
- [ ] Redis 缓存实现
- [ ] 性能优化

### Phase 5：管理功能
- [ ] 权限管理 API
- [ ] API 自动扫描注册
- [ ] 权限变更审计

---

## 11. 关键设计决策总结

| 决策点 | 选择 | 理由 |
|-------|------|------|
| 目标框架 | .NET 10 Only | 减少维护成本，享受最新特性 |
| ORM | EF Core + Dapper | EF Core 为主，Dapper 补充性能敏感场景 |
| API 权限 | ASP.NET Core 内置授权 | 标准化，无缝集成 |
| 数据权限 | 显式调用扩展方法 | 代码可读，灵活自由，避免意外 |
| 缓存 | 接口抽象 + 多实现 | 使用者可选择适合的缓存方案 |
