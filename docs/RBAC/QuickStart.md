# RBAC 快速入门指南

> 5 分钟快速上手 PyraminxCube RBAC 框架。
> 建议配合 [Glossary.md](./Glossary.md) 术语表阅读。

---

## 一句话概述

**PyraminxCube** 是一个以 RBAC（基于角色的访问控制）权限管理为核心的 .NET 模块化框架。

```
功能权限 → 框架完整实现 ✅
数据权限 → 框架提供抽象机制 ⚡
```

---

## 前置要求

| 要求 | 说明 |
|------|------|
| .NET 9+ | 后端运行时 |
| SQL Server / MySQL / PostgreSQL | 数据库 |
| Vue 3 + Ant Design Vue | 前端（可选） |

---

## 快速开始（5 分钟）

### 步骤 1：安装 NuGet 包

```bash
# 核心包
dotnet add package PyraminxCube.Rbac.Core

# EF Core 实现
dotnet add package PyraminxCube.Rbac.EntityFrameworkCore

# ASP.NET Core 集成
dotnet add package PyraminxCube.Rbac.AspNetCore
```

---

### 步骤 2：配置服务

在 `Program.cs` 中注册 RBAC 服务：

```csharp
// 添加 RBAC 服务
builder.Services.AddPyraminxRbac(options =>
{
    options.UseEntityFrameworkCore<AppDbContext>();
});

// 配置授权
builder.Services.AddAuthorization();
```

---

### 步骤 3：启用中间件

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

---

### 步骤 4：使用 API 权限控制

```csharp
[ApiPermission("user:list")]
[HttpGet("/api/users")]
public async Task<IActionResult> GetUsers()
{
    var users = await _dbContext.Users.ToListAsync();
    return Ok(users);
}
```

---

### 步骤 5：使用数据权限控制

```csharp
[ApiPermission("order:list")]
[HttpGet("/api/orders")]
public async Task<IActionResult> GetOrders()
{
    // 显式调用数据权限过滤
    var orders = await _dbContext.Orders
        .WithDataScope()
        .Where(o => o.Status == "pending")
        .ToListAsync();
    
    return Ok(orders);
}
```

---

## 核心概念速览

### 1. 三层权限体系

```
┌─────────────────────────────────────────────────────┐
│                  权限控制层次                         │
├─────────────────┬───────────────────────────────────┤
│  API 权限       │  [ApiPermission("user:list")]    │
│  (接口级别)     │  拦截非法请求                      │
├─────────────────┼───────────────────────────────────┤
│  功能权限       │  v-permission 指令                │
│  (按钮级别)     │  控制按钮显示/隐藏                 │
├─────────────────┼───────────────────────────────────┤
│  数据权限       │  .WithDataScope() 扩展方法        │
│  (行级过滤)     │  查询时自动过滤数据               │
└─────────────────┴───────────────────────────────────┘
```

### 2. 用户-角色-权限模型

```
用户 ←── N:M ──→ 角色 ←── N:M ──→ 权限
                              │
                              ├─→ API 权限
                              ├─→ 功能权限
                              └─→ 数据权限
```

### 3. 权限计算规则

| 规则 | 说明 |
|------|------|
| 多角色合并 | **并集**（角色越多，权限越大） |
| 数据过滤 | **交集**（只返回有权限的数据） |
| 多维度组合 | **AND 关系**（必须同时满足） |

---

## 常见使用场景

### 场景 1：控制 API 访问

```csharp
// 只有拥有 "order:create" 权限的用户才能访问
[ApiPermission("order:create")]
[HttpPost("/api/orders")]
public async Task<IActionResult> CreateOrder(OrderDto dto)
{
    // 业务逻辑
}
```

---

### 场景 2：控制按钮显示

```vue
<!-- 只有拥有 "user:add" 权限的用户才能看到此按钮 -->
<a-button v-permission="'user:add'">新增用户</a-button>
```

---

### 场景 3：控制数据访问

```csharp
// 只返回当前用户有权限查看的数据
var orders = await _dbContext.Orders
    .WithDataScope()
    .ToListAsync();
```

---

### 场景 4：超级管理员

```csharp
// IsSuperAdmin = true 的用户跳过所有权限校验
public class User
{
    public bool IsSuperAdmin { get; set; }
}
```

---

## 项目结构

```
src/backend/
├── RBAC/
│   ├── PyraminxCube.Rbac.Core/              # 核心接口
│   ├── PyraminxCube.Rbac.EntityFrameworkCore/ # EF Core 实现
│   └── PyraminxCube.Rbac.AspNetCore/        # ASP.NET Core 集成
└── Applications/
    └── PyraminxCube.Applications.WebApp/    # 示例应用
```

---

## 下一步做什么

| 目标 | 推荐阅读 |
|------|---------|
| 理解完整设计 | [RBAC_Design.md](./RBAC_Design.md) |
| 了解术语 | [Glossary.md](./Glossary.md) |
| 查看功能清单 | [Functional_Design.md](./Functional_Design.md) |
| 了解数据库设计 | [Database_Design.md](./Database_Design.md) |
| 深入技术实现 | [CSharp_Implementation.md](./CSharp_Implementation.md) |

---

## 常见问题

**Q: 数据权限一定要用吗？**
A: 不强制。`.WithDataScope()` 是显式调用，不调用就不过滤。

**Q: 如何定义数据维度？**
A: 在业务系统中向 `DataDimensions` 表插入维度定义。

**Q: 如何实现超级管理员？**
A: 设置用户 `IsSuperAdmin = true`，自动跳过所有权限校验。

---

## 更新日志

| 日期 | 内容 |
|------|------|
| 2026-04-25 | 初始版本 |