# CLAUDE.md

本文档为 Claude Code (claude.ai/code) 在本项目中工作时提供指导。

## 项目概述

**PyraminxCube** (金字塔魔法) - 一个以 RBAC（基于角色的访问控制）权限管理为核心的模块化 .NET 框架。

## 构建命令

```bash
# 构建单个项目
cd src/backend/RBAC/PyraminxCube.Rbac.Core && dotnet build

# 运行 WebApp
cd src/backend/Applications/PyraminxCube.Applications.WebApp && dotnet run

# 运行测试（如果存在）
dotnet test
```

## 架构

### 后端结构 (`src/backend/`)

```
src/backend/
├── Applications/
│   └── PyraminxCube.Applications.WebApp/    # 主 Web API 应用（目前较简单，需集成 RBAC）
├── RBAC/                                      # RBAC 权限框架
│   ├── PyraminxCube.Rbac.Core/               # 核心接口和模型
│   ├── PyraminxCube.Rbac.EntityFrameworkCore/ # EF Core 实现，包含 DbContext、实体、服务
│   └── PyraminxCube.Rbac.AspNetCore/         # ASP.NET Core 集成（授权处理器）
├── Cache/                                     # 缓存抽象层
│   ├── PyraminxCube.Cache.Abstractions/
│   └── PyraminxCube.Cache.Redis/
├── Repositories/                              # 数据访问层
├── Models/                                    # 领域模型
├── Extensions/                                # 扩展方法
└── Common/Utilities/                          # 工具类
```

### 前端结构 (`src/frontend/`)

预留目录，用于未来 Vue.js 前端应用。

### 核心设计模式

- **RBAC 权限模型**：三层权限体系
  - API 权限：通过 `[ApiPermission]` 特性进行端点级别的访问控制
  - 功能权限：按钮/UI 元素控制（树形结构，包含 MODULE/PAGE/BUTTON 类型）
  - 数据权限：通过显式调用 `.WithDataScope()` 扩展方法进行行级数据过滤
- **实体分类**：
  - `RbacEntity`：租户级实体（用户、角色、数据范围）
  - `RbacGlobalEntity`：全局实体（跨租户共享，如 ApiPermission、FeaturePermission、DataDimension）
- **多租户支持**：通过 TenantId 进行租户隔离
- **软删除**：EF Core 全局查询过滤器自动应用 `!IsDeleted` 条件

### 文档参考

详细 RBAC 设计文档位于 `docs/RBAC/`：
- `RBAC_Design.md` - 设计概览和原则
- `Functional_Design.md` - 功能清单
- `Database_Design.md` - 数据库表结构
- `CSharp_Implementation.md` - 后端实现细节
- `Frontend_Implementation.md` - 前端技术栈

## 重要说明

- WebApp (`Program.cs`) 目前较简单 - 需要注册 RBAC 服务
- RBAC API 权限校验使用 `[ApiPermission("api:code")]` 或 `[ApiPermission]`（自动推断）
- 数据权限过滤需要显式调用：`_dbContext.Orders.WithDataScope(service, user).ToListAsync()`
- 使用 Entity Framework Core 的全局查询过滤器实现软删除
- 权限计算使用并集：用户有效权限 = 所有角色权限的合并