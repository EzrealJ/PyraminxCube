# PyraminxCube 技术选型总纲

> 本文档定义 PyraminxCube（金字塔魔方）框架的整体技术决策。
> 所有子模块的技术选型应与本文档保持一致。

---

## 1. 技术栈总览

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           PyraminxCube 技术栈                           │
├─────────────────────────────────────────────────────────────────────────┤
│  后端 (.NET)                                                            │
│  ├─ 框架        │  .NET 10 + ASP.NET Core 8+                            │
│  ├─ ORM         │  EF Core 8 + Dapper                                   │
│  ├─ 认证        │  JWT Bearer                                           │
│  ├─ 缓存        │  MemoryCache + Redis (StackExchange.Redis)            │
│  ├─ 日志        │  Microsoft.Extensions.Logging + Serilog               │
│  ├─ 配置        │  Options 模式 + IConfiguration                        │
│  └─ API 文档    │  Swashbuckle (OpenAPI)                                │
├─────────────────────────────────────────────────────────────────────────┤
│  前端 (Vue)                                                             │
│  ├─ 框架        │  Vue 3.4+ + TypeScript 5.x                            │
│  ├─ UI 库       │  Ant Design Vue 4.x                                   │
│  ├─ 构建        │  Vite 5.x                                             │
│  ├─ 状态        │  Pinia 2.x                                            │
│  ├─ 路由        │  Vue Router 4.x                                       │
│  ├─ HTTP        │  Axios                                                │
│  ├─ 工具        │  lodash-es, dayjs                                     │
│  └─ 代码规范    │  ESLint + Prettier                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. 后端技术选型

### 2.1 核心框架

| 选择 | 版本 | 说明 |
|------|------|------|
| **.NET** | 10 | 目标运行时 |
| **ASP.NET Core** | 8+ | Web 框架 |

**理由**：
- .NET 10 是最新主版本，性能最优
- ASP.NET Core 8+ 是 LTS 版本，稳定性好

---

### 2.2 数据访问

| 组件 | 选择 | 说明 |
|------|------|------|
| **主 ORM** | EF Core 8 | 负责 CRUD、复杂查询、迁移 |
| **辅助** | Dapper | 性能敏感场景，高频小查询 |
| **数据库** | SQL Server（默认） | 兼容 MySQL、PostgreSQL |

**理由**：
- EF Core 生产力高，复杂查询和关联易处理
- Dapper 补充性能敏感场景，避免 ORM 性能陷阱
- 两者组合是业界常见做法

---

### 2.3 认证授权

| 组件 | 选择 | 说明 |
|------|------|------|
| **认证** | JWT Bearer | 无状态，适合分布式 |
| **授权** | Policy + Handler | ASP.NET Core 内置 |
| **密码** | PBKDF2 | 内置安全实现 |

**理由**：
- JWT 是行业标准，无状态易扩展
- ASP.NET Core 授权框架强大，配合 RBAC 灵活
- 不重复造轮子

---

### 2.4 缓存

| 场景 | 选择 | 说明 |
|------|------|------|
| **本地缓存** | MemoryCache | 单机场景 |
| **分布式缓存** | Redis | 集群场景，使用 StackExchange.Redis |

**理由**：
- MemoryCache 开箱即用，性能好
- Redis 是分布式缓存事实标准

---

### 2.5 日志

| 组件 | 选择 | 说明 |
|------|------|------|
| **日志框架** | Serilog | 结构化日志，输出到控制台/文件 |
| **日志级别** | appsettings.json 配置 | 运行时可调 |

**理由**：
- Serilog 是 .NET 最流行的日志库
- 结构化日志便于排查问题

---

### 2.6 API 文档

| 组件 | 选择 | 说明 |
|------|------|------|
| **API 文档** | Swashbuckle | 自动生成 OpenAPI 文档 |
| **UI** | Swagger UI | 在线调试接口 |

**理由**：
- ASP.NET Core 内置支持
- 前后端分离开发必备

---

## 3. 前端技术选型

### 3.1 核心框架

| 组件 | 选择 | 版本 | 说明 |
|------|------|------|------|
| **框架** | Vue | 3.4+ | Composition API |
| **语言** | TypeScript | 5.x | 类型安全 |
| **构建** | Vite | 5.x | 开发体验快 |

**理由**：
- Vue 3 是当前主流，Composition API 组织代码灵活
- TypeScript 保障代码质量
- Vite 开发体验极佳

---

### 3.2 UI 库

| 组件 | 选择 | 版本 | 说明 |
|------|------|------|------|
| **UI 框架** | Ant Design Vue | 4.x | 企业级组件库 |
| **图标** | @ant-design/icons-vue | 7.x | 配套图标库 |

**理由**：
- Ant Design Vue 是国内企业后台首选
- Tree、Transfer、Table 组件成熟，适合 RBAC 场景
- 组件设计规范，风格统一

---

### 3.3 状态管理

| 组件 | 选择 | 版本 | 说明 |
|------|------|------|------|
| **状态管理** | Pinia | 2.x | Vue 3 官方推荐 |
| **持久化** | pinia-plugin-persistedstate | 可选 | 状态持久化 |

**理由**：
- Pinia 是 Vue 3 官方推荐，比 Vuex 轻量
- API 简洁，TypeScript 支持好

---

### 3.4 HTTP 请求

| 组件 | 选择 | 说明 |
|------|------|------|
| **HTTP 客户端** | Axios | 拦截器、请求/响应统一处理 |
| **认证头** | Bearer Token | 与后端 JWT 对应 |

**理由**：
- Axios 是 Vue 生态事实标准
- 拦截器机制方便统一处理 Token、错误

---

### 3.5 代码规范

| 工具 | 说明 |
|------|------|
| **ESLint** | 代码检查 |
| **Prettier** | 代码格式化 |
| **stylelint** | CSS 格式化（可选） |

**理由**：
- 团队协作必备，保持代码风格统一
- 可配合 Git Hooks 强制执行

---

## 4. 架构模式

### 4.1 应用架构

```
单体应用（Monolithic）
┌──────────────────────────────────────────────────────────────┐
│                      Web Application                          │
├──────────────────────────────────────────────────────────────┤
│  Controllers (API层)                                          │
│       ↓                                                        │
│  Services (业务层)                                            │
│       ↓                                                        │
│  Repositories (数据访问层)                                    │
│       ↓                                                        │
│  DbContext (EF Core) + Dapper                                 │
└──────────────────────────────────────────────────────────────┘
```

**原则**：
- 分层清晰：Controller → Service → Repository
- 依赖方向：Controller → Service → Repository
- 单一职责：每个类只做一件事

---

### 4.2 项目结构

```
src/
├── backend/
│   ├── Applications/
│   │   └── PyraminxCube.Applications.WebApp/     # 入口项目
│   ├── Common/                                    # 通用工具
│   ├── Repositories/                              # 数据访问
│   ├── Extensions/                                # 扩展方法
│   ├── Modules/                                   # 功能模块
│   │   └── RBAC/
│   │       ├── PyraminxCube.Rbac.Core/
│   │       ├── PyraminxCube.Rbac.EntityFrameworkCore/
│   │       └── PyraminxCube.Rbac.AspNetCore/
│   └── Cache/                                     # 缓存模块
│       ├── PyraminxCube.Cache.Abstractions/
│       └── PyraminxCube.Cache.Redis/
│
└── frontend/
    ├── src/
    │   ├── api/                      # API 接口
    │   ├── assets/                   # 静态资源
    │   ├── components/               # 公共组件
    │   ├── composables/              # 组合式函数
    │   ├── directives/               # 自定义指令
    │   ├── layouts/                  # 布局组件
    │   ├── router/                   # 路由配置
    │   ├── stores/                   # Pinia 状态
    │   ├── utils/                    # 工具函数
    │   └── views/                    # 页面组件
    ├── index.html
    ├── package.json
    └── vite.config.ts
```

**说明**：
- 后端按功能模块分包，模块内部采用分层结构
- 前端按功能类型分包（api/components/views/stores 等）
- 模块之间通过接口解耦

---

### 4.3 依赖注入

| 层级 | 注册方式 |
|------|---------|
| **接口** | `services.AddScoped<IPermissionService, PermissionService>()` |
| **瞬时** | `services.AddTransient<IEmailService, EmailService>()` |
| **单例** | `services.AddSingleton<IConfigService, ConfigService>()` |

**原则**：
- 接口 + 实现分离，便于单元测试
- 尽量使用 Scoped，保持请求内实例一致

---

## 5. 安全策略

### 5.1 认证

| 项目 | 策略 |
|------|------|
| **认证方式** | JWT Bearer Token |
| **Token 存储** | HttpOnly Cookie（推荐）或 LocalStorage |
| **Token 有效期** | Access Token: 2小时，Refresh Token: 7天 |
| **密码加密** | PBKDF2 + Salt |

### 5.2 授权

| 项目 | 策略 |
|------|------|
| **RBAC** | 基于角色，权限精确到 API/按钮/数据 |
| **超级管理员** | 跳过所有权限校验 |
| **数据权限** | 显式调用，不自动附加 |

### 5.3 防护

| 威胁 | 防护措施 |
|------|---------|
| **XSS** | Vue 自动转义，输出前注意 |
| **CSRF** | AntiForgery Token |
| **SQL 注入** | 参数化查询（EF Core 默认） |
| **暴力破解** | 登录失败次数限制 |

---

## 6. 部署方案

### 6.1 开发环境

```
后端：dotnet run（VS Code / Rider / Visual Studio）
前端：npm run dev（Vite 热更新）
数据库：SQL Server Developer / Docker
```

### 6.2 生产环境

| 方式 | 说明 |
|------|------|
| **传统部署** | 后端编译发布 + 前端构建 Nginx 托管 |
| **Docker** | 前后端分离容器，docker-compose 编排 |

**推荐**：Docker 一把梭，方便快捷。

---

## 7. 开发工作流

### 7.1 代码管理

| 工具 | 说明 |
|------|------|
| **Git** | 版本控制 |
| **分支策略** | Git Flow 或 Trunk-Based |

### 7.2 代码质量

| 工具 | 说明 |
|------|------|
| **后端** | Rider/Resharper 内置检查 |
| **前端** | ESLint + Prettier |
| **提交** | Commitlint 规范提交信息 |

---

## 8. 文档体系

```
docs/
├── README.md                      # 项目介绍
├── ARCHITECTURE.md                # 架构设计
├── TECH_STACK.md                  # 技术选型（本文件）
├── MODULES/
│   ├── RBAC/
│   │   ├── QuickStart.md          # 快速入门
│   │   ├── Glossary.md            # 术语表
│   │   ├── RBAC_Design.md         # 设计文档
│   │   ├── Database_Design.md     # 数据库设计
│   │   └── ...
│   └── CACHE/
│       └── ...
└── DEPLOY.md                      # 部署文档
```

---

## 9. 后续模块技术参考

| 模块 | 推荐技术 |
|------|---------|
| **缓存** | MemoryCache / Redis |
| **分布式锁** | RedLock + Redis |
| **消息队列** | RabbitMQ / MassTransit |
| **搜索** | Elasticsearch |
| **对象存储** | MinIO / 阿里云 OSS |
| **定时任务** | Quartz.NET / Hangfire |

---

## 10. 关键决策记录

| 日期 | 决策 | 理由 |
|------|------|------|
| 2026-04-25 | .NET 10 | 最新主版本，性能最优 |
| 2026-04-25 | Ant Design Vue 4.x | 国内企业后台主流，组件成熟 |
| 2026-04-25 | 单体架构 | 简化部署，按需取用包 |
| 2026-04-25 | JWT 认证 | 无状态，易扩展 |
| 2026-04-25 | EF Core + Dapper | 生产力 + 性能兼顾 |

---

## 相关文档

| 文档 | 说明 |
|------|------|
| [README.md](./README.md) | 项目介绍 |
| [ARCHITECTURE.md](./ARCHITECTURE.md) | 详细架构设计 |
| [DEPLOY.md](./DEPLOY.md) | 部署指南 |
| [docs/RBAC/](./RBAC/) | RBAC 模块文档 |