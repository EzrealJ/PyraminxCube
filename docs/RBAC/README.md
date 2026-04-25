# RBAC 设计大纲

> 快速回顾核心设计思路，防止时间久了忘记 😄

---

## 一句话总结

**功能权限在框架内完整实现，数据权限只提供抽象机制。**

---

## 核心设计理念

```mermaid
graph LR
    subgraph 功能权限[功能权限 - 技术性]
        API[API接口权限<br/>精细到每个接口]
        BTN[功能按钮权限<br/>树形结构]
    end
    
    subgraph 数据权限[数据权限 - 业务性]
        DIM[维度定义（抽象）<br/>框架提供表结构]
        AUTH[勾选式授权<br/>权限并集计算]
    end
    
    功能权限 -->|框架完整实现| ✅[完整实现]
    数据权限 -->|框架提供机制| ⚡[抽象机制]
```

---

## 两类权限对比

| 对比项 | 功能权限 | 数据权限 |
|-------|---------|---------|
| 性质 | 技术性 | 业务性 |
| 控制目标 | 能做什么（接口/按钮） | 能看什么（数据范围） |
| 框架实现 | 完整实现 | 仅提供机制 |
| 维度 | 固定（API、按钮） | 抽象（由业务定义） |
| 通用性 | 所有系统通用 | 每个系统可能不同 |

---

## 核心表清单

### 功能权限（框架实现）
- `ApiGroups` - API分组
- `ApiPermissions` - API权限
- `ApiGroupMappings` - API与分组关联
- `RoleApiPermissions` - 角色API权限
- `FeaturePermissions` - 功能按钮权限（树形）
- `RoleFeaturePermissions` - 角色功能权限
- `FeatureApiMappings` - **按钮与API关联**（避免按钮能看到但点了没用）

### 数据权限（抽象机制）
- `DataDimensions` - **维度定义**（业务系统填充）
- `DataScopes` - **维度值**（业务系统填充）
- `RoleDataScopes` - 角色数据范围
- `RoleDataScopeFlags` - 特殊标记（ALL/SELF）
- `DataDimensionMappings` - 维度与业务表映射（业务配置）

---

## 权限计算规则

```
多角色权限合并 = 【并集】（角色越多，权限越大）
数据查询过滤 = 【交集】（用户权限 ∩ 实际数据）
多维度组合 = 【AND关系】（必须同时满足所有维度）
```

**特殊标记优先级：**
- 任一角色有 `ALL` → 该维度全部权限
- 所有角色都是 `SELF` → 仅自己创建的数据

**授权模式：**
- 精确授权：勾选哪个节点就授权哪个，不自动继承子级
- 数据权限只控制读，写操作通过功能权限控制

---

## 数据权限抽象化示例

**框架层面（不预设具体维度）：**
```
DataDimensions 表：业务系统自己定义有哪些维度
DataScopes 表：业务系统自己定义每个维度有哪些值
```

**业务系统A（ERP）可能这样定义：**
```
维度：部门、区域
值：研发部、市场部、华东区、华南区...
```

**业务系统B（项目管理）可能这样定义：**
```
维度：项目、团队
值：项目A、项目B、前端团队、后端团队...
```

**框架不关心具体是什么维度，只提供机制！**

---

## 关键设计决策

1. **为什么功能权限在框架内完整实现？**
   - API和按钮是技术概念，所有系统都有
   - 可以标准化，框架可以完整处理

2. **为什么数据权限只提供抽象机制？**
   - 数据权限维度是业务概念，每个系统不同
   - 框架预设维度会限制复用性
   - 只提供"可能性"，具体由业务定义

3. **为什么用并集合并角色权限？**
   - 并集更符合"授权"的语义：给用户多个角色是为了扩大权限
   - 交集会导致角色越多权限越小，反直觉

4. **为什么查询时用交集？**
   - 用户权限范围 ∩ 实际数据 = 只返回有权限的数据
   - 这是数据权限过滤的本质

5. **为什么不用表达式匹配？**
   - 表达式复杂，学习成本高
   - 简单勾选更直观：勾一项授一项
   - 满足大多数场景需求

6. **为什么数据权限只控制读？**
   - 读控制（数据权限）+ 功能控制（API权限）= 完整的权限体系
   - 写操作通过API权限间接控制，更灵活

7. **为什么要建立按钮与API的关联？**
   - 避免"按钮能看到但点了没用"的尴尬情况
   - 授权时可提示/自动授权关联的API

---

## 文件导航

| 文档 | 说明 |
|------|------|
| [QuickStart.md](./QuickStart.md) | **快速入门指南**（5 分钟上手，推荐首次阅读） |
| [RBAC_Design.md](./RBAC_Design.md) | 模块完整设计文档 |
| [Glossary.md](./Glossary.md) | 术语表（快速查阅专业术语） |
| [Functional_Design.md](./Functional_Design.md) | 功能设计清单（具体要做什么） |
| [Database_Design.md](./Database_Design.md) | 数据库表结构设计 |
| [CSharp_Implementation.md](./CSharp_Implementation.md) | C# 后端技术选型 |
| [Frontend_Implementation.md](./Frontend_Implementation.md) | Vue 前端技术选型 |

---

## 推荐阅读顺序

1. **首次使用**：先读 [QuickStart.md](./QuickStart.md) 快速上手（5 分钟）
2. **整体了解**：再读 [RBAC_Design.md](./RBAC_Design.md) 了解完整设计
3. **术语查阅**：遇到陌生术语时查看 [Glossary.md](./Glossary.md)
4. **功能开发**：参考 [Functional_Design.md](./Functional_Design.md) 获取功能清单
5. **数据库设计**：查看 [Database_Design.md](./Database_Design.md) 表结构
6. **技术实现**：选型参考 [CSharp_Implementation.md](./CSharp_Implementation.md) 和 [Frontend_Implementation.md](./Frontend_Implementation.md)

---

## TODO（未来可扩展）

- [ ] 角色继承机制
- [ ] 权限有效期
- [ ] 审批流程集成
- [ ] 权限变更审计日志
- [ ] 权限缓存策略详细设计
