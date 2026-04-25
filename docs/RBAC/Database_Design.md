# RBAC 数据库设计

## 1. 通用字段规范

所有表均包含以下基础字段：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT/UUID | 主键，唯一标识 |
| `IsValid` | BOOLEAN | 软删除标记，true=有效 |
| `CreateUserId` | BIGINT | 创建人ID |
| `CreateTime` | DATETIME | 创建时间 |
| `ModifyUserId` | BIGINT | 最后修改人ID |
| `ModifyTime` | DATETIME | 最后修改时间 |

---

## 2. 核心表结构

### 2.1 用户管理模块

#### 用户表 (Users)

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `Username` | VARCHAR(50) | 用户名，唯一 |
| `Email` | VARCHAR(100) | 邮箱，唯一 |
| `Password` | VARCHAR(255) | 加密后的密码 |
| `Status` | TINYINT | 状态：0-禁用，1-启用 |
| 通用字段 | - | - |

#### 用户扩展表 (UserProfiles)

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `UserId` | BIGINT | 关联用户ID |
| `Avatar` | VARCHAR(255) | 头像URL |
| `Nickname` | VARCHAR(50) | 昵称 |
| `PhoneNumber` | VARCHAR(20) | 电话号码 |
| `Gender` | TINYINT | 性别 |
| `Birthday` | DATE | 生日 |
| `Address` | VARCHAR(255) | 地址 |
| `Bio` | TEXT | 个人简介 |
| 通用字段 | - | - |

---

### 2.2 角色管理模块

#### 角色表 (Roles)

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `RoleCode` | VARCHAR(50) | 角色编码，唯一，如 "ADMIN" |
| `RoleName` | VARCHAR(100) | 角色名称，如 "系统管理员" |
| `Description` | VARCHAR(500) | 角色描述 |
| `SortOrder` | INT | 排序号 |
| 通用字段 | - | - |

#### 用户角色关联表 (UserRoles)

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `UserId` | BIGINT | 用户ID |
| `RoleId` | BIGINT | 角色ID |
| 通用字段 | - | - |

**索引：** `UNIQUE(UserId, RoleId)`

---

## 3. 功能权限模块（框架完整实现）

### 3.1 API权限

#### API分组表 (ApiGroups)

用于将API按模块/功能分组，便于批量授权。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `GroupCode` | VARCHAR(50) | 分组编码，唯一 |
| `GroupName` | VARCHAR(100) | 分组名称 |
| `Description` | VARCHAR(500) | 分组描述 |
| `SortOrder` | INT | 排序号 |
| 通用字段 | - | - |

#### API权限表 (ApiPermissions)

定义每个API接口的权限。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `ApiCode` | VARCHAR(100) | API编码，唯一 |
| `ApiName` | VARCHAR(100) | API名称 |
| `ApiEndpoint` | VARCHAR(200) | 接口路径，如 `/api/users/list` |
| `HttpMethod` | VARCHAR(10) | HTTP方法：GET/POST/PUT/DELETE |
| `Description` | VARCHAR(500) | 接口描述 |
| `SortOrder` | INT | 排序号 |
| 通用字段 | - | - |

**索引：** `UNIQUE(ApiEndpoint, HttpMethod)`

#### API与分组关联表 (ApiGroupMappings)

一个API可属于多个分组。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `ApiId` | BIGINT | API权限ID |
| `GroupId` | BIGINT | API分组ID |
| 通用字段 | - | - |

**索引：** `UNIQUE(ApiId, GroupId)`

#### 角色API权限关联表 (RoleApiPermissions)

为角色分配API权限（支持单个API或API组）。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `RoleId` | BIGINT | 角色ID |
| `PermissionId` | BIGINT | 权限ID（ApiId 或 GroupId） |
| `PermissionType` | VARCHAR(20) | 类型：`API` 或 `API_GROUP` |
| 通用字段 | - | - |

**索引：** `UNIQUE(RoleId, PermissionId, PermissionType)`

---

### 3.2 功能按钮权限

#### 功能权限表 (FeaturePermissions)

树形结构，管理模块/页面/按钮权限。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `FeatureCode` | VARCHAR(100) | 功能编码，唯一 |
| `FeatureName` | VARCHAR(100) | 功能名称 |
| `ParentId` | BIGINT | 父级ID，NULL表示顶级 |
| `FeatureType` | VARCHAR(20) | 类型：`MODULE`/`PAGE`/`BUTTON` |
| `Path` | VARCHAR(200) | 前端路由路径（PAGE类型使用） |
| `Icon` | VARCHAR(100) | 图标（MODULE/PAGE类型使用） |
| `SortOrder` | INT | 排序号 |
| `Description` | VARCHAR(500) | 功能描述 |
| 通用字段 | - | - |

**示例数据：**
```
ID  | FeatureCode     | FeatureName | ParentId | FeatureType
----|-----------------|-------------|----------|------------
1   | system          | 系统管理     | NULL     | MODULE
2   | system:user     | 用户管理     | 1        | PAGE
3   | system:user:add | 新增用户     | 2        | BUTTON
4   | system:user:edit| 编辑用户     | 2        | BUTTON
5   | system:user:del | 删除用户     | 2        | BUTTON
```

#### 角色功能权限关联表 (RoleFeaturePermissions)

为角色分配功能权限。**精确授权：勾选哪个节点就授权哪个，不自动继承子级。**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `RoleId` | BIGINT | 角色ID |
| `FeatureId` | BIGINT | 功能权限ID |
| 通用字段 | - | - |

**索引：** `UNIQUE(RoleId, FeatureId)`

#### 功能与API关联表 (FeatureApiMappings)

建立功能按钮与API的关联关系，避免"按钮能看到但点了没用"的情况。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `FeatureId` | BIGINT | 功能权限ID（通常是BUTTON类型） |
| `ApiId` | BIGINT | API权限ID |
| 通用字段 | - | - |

**索引：** `UNIQUE(FeatureId, ApiId)`

**作用：**
- 授权按钮时，提示关联的API
- 可实现"授权按钮时自动授权对应API"
- 保证按钮权限与API权限的一致性

**示例数据：**
```
FeatureId | ApiId | 说明
----------|-------|---------------------------
3         | 101   | 新增用户按钮 → POST /api/users
4         | 102   | 编辑用户按钮 → PUT /api/users/{id}
5         | 103   | 删除用户按钮 → DELETE /api/users/{id}
```

---

## 4. 数据权限模块（框架提供抽象机制）

### 4.1 设计说明

数据权限采用**抽象维度**设计：
- 框架只提供"维度"和"维度值"的表结构
- 具体有哪些维度（如部门、区域），由业务系统定义
- 采用简单的**勾选式授权**，不使用表达式

**权限计算规则：**
- **多角色权限合并**：并集（角色越多，权限范围越大）
- **数据查询过滤**：用户权限范围 ∩ 实际数据（只返回有权限的数据）
- **多维度组合**：AND关系（必须同时满足所有维度）
- **数据权限只控制读**：写操作通过功能权限（API权限）控制

**授权模式：**
- 精确授权：勾选哪个节点就授权哪个，不自动继承子级
- 维度值支持层级结构（通过ParentId），但授权时需显式勾选

### 4.2 表结构

#### 数据维度定义表 (DataDimensions)

定义数据权限控制的维度类型。**此表由业务系统填充**。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `DimensionCode` | VARCHAR(50) | 维度编码，唯一，如 `DEPARTMENT`、`REGION` |
| `DimensionName` | VARCHAR(100) | 维度名称，如 "部门"、"区域" |
| `Description` | VARCHAR(500) | 维度描述 |
| `SortOrder` | INT | 排序号 |
| 通用字段 | - | - |

**示例数据（业务系统定义）：**
```
ID | DimensionCode | DimensionName | Description
---|---------------|---------------|------------------
1  | DEPARTMENT    | 部门          | 按组织部门控制数据
2  | REGION        | 区域          | 按地理区域控制数据
3  | PROJECT       | 项目          | 按项目控制数据
```

#### 数据范围值表 (DataScopes)

定义每个维度下的具体值。**此表由业务系统填充**。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `DimensionId` | BIGINT | 所属维度ID |
| `ScopeCode` | VARCHAR(50) | 范围编码，如 `DEPT_RD`、`REGION_EAST` |
| `ScopeName` | VARCHAR(100) | 范围名称，如 "研发部"、"华东区" |
| `ParentId` | BIGINT | 父级ID，支持层级结构 |
| `SortOrder` | INT | 排序号 |
| `Description` | VARCHAR(500) | 描述 |
| 通用字段 | - | - |

**索引：** `UNIQUE(DimensionId, ScopeCode)`

**示例数据（业务系统定义）：**
```
ID | DimensionId | ScopeCode    | ScopeName | ParentId
---|-------------|--------------|-----------|----------
1  | 1           | DEPT_RD      | 研发部     | NULL
2  | 1           | DEPT_RD_FE   | 前端组     | 1
3  | 1           | DEPT_RD_BE   | 后端组     | 1
4  | 1           | DEPT_MARKET  | 市场部     | NULL
5  | 2           | REGION_EAST  | 华东区     | NULL
6  | 2           | REGION_SOUTH | 华南区     | NULL
```

#### 角色数据权限关联表 (RoleDataScopes)

为角色分配数据权限范围。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `RoleId` | BIGINT | 角色ID |
| `ScopeId` | BIGINT | 数据范围ID |
| 通用字段 | - | - |

**索引：** `UNIQUE(RoleId, ScopeId)`

#### 数据权限特殊标记表 (RoleDataScopeFlags)

支持特殊的数据权限标记（如"全部"、"仅自己"）。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `RoleId` | BIGINT | 角色ID |
| `DimensionId` | BIGINT | 维度ID |
| `ScopeFlag` | VARCHAR(20) | 特殊标记：`ALL`=全部，`SELF`=仅自己 |
| 通用字段 | - | - |

**索引：** `UNIQUE(RoleId, DimensionId)`

**说明：**
- 当角色在某维度设置了 `ALL` 标记，表示该角色在该维度下可访问所有数据
- 当角色在某维度设置了 `SELF` 标记，表示只能访问自己创建的数据
- 如果既没有特殊标记，也没有分配具体范围，则在该维度下无数据权限

---

## 5. 数据库关系图

```
┌─────────────┐     ┌─────────────┐
│   Users     │────│ UserProfiles │
│             │ 1:1│             │
└─────────────┘     └─────────────┘
       │
       │ 1:N
       ▼
┌─────────────┐     ┌─────────────┐
│  UserRoles  │────│    Roles    │
│             │ N:1│             │
└─────────────┘     └─────────────┘
                           │
         ┌─────────────────┼─────────────────┐
         │                 │                 │
         ▼                 ▼                 ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│RoleApiPermissions│ │RoleFeature      │ │RoleDataScopes   │
│                 │ │Permissions      │ │                 │
└─────────────────┘ └─────────────────┘ └─────────────────┘
         │                 │                 │
         ▼                 ▼                 ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ ApiPermissions  │ │FeaturePermissions│ │  DataScopes     │
│ + ApiGroups     │ │    (树形)        │ │                 │
└─────────────────┘ └─────────────────┘ └─────────────────┘
                                               │
                                               ▼
                                        ┌─────────────────┐
                                        │ DataDimensions  │
                                        │  (维度定义)      │
                                        └─────────────────┘
```

---

## 6. 权限计算规则

### 6.1 功能权限（API + 按钮）

用户的功能权限 = **所有角色功能权限的并集**

```
用户拥有角色A和角色B

角色A的API权限：[接口1, 接口2]
角色B的API权限：[接口2, 接口3]

用户最终API权限 = [接口1, 接口2, 接口3]（并集）
```

### 6.2 数据权限

用户的数据权限 = **所有角色数据范围的并集**

```
用户拥有角色A和角色B

角色A的数据范围：
  - 部门维度：[研发部]
  - 区域维度：[华东区]

角色B的数据范围：
  - 部门维度：[研发部, 市场部]
  - 区域维度：[华南区]

用户最终数据权限：
  - 部门维度：[研发部, 市场部]（并集）
  - 区域维度：[华东区, 华南区]（并集）
```

**特殊标记优先级：**
- 如果任一角色在某维度有 `ALL` 标记 → 该维度权限为全部
- 如果所有角色在某维度都是 `SELF` 标记 → 该维度权限为仅自己

---

## 7. 维度与业务表映射（业务系统配置）

### 数据权限维度映射表 (DataDimensionMappings)

配置数据维度与业务表字段的映射关系。**此表由业务系统配置**。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `DimensionId` | BIGINT | 维度ID |
| `TableName` | VARCHAR(100) | 业务表名 |
| `ColumnName` | VARCHAR(100) | 关联字段名 |
| `Description` | VARCHAR(500) | 描述 |
| 通用字段 | - | - |

**示例：**
```
ID | DimensionId | TableName | ColumnName    | Description
---|-------------|-----------|---------------|------------------
1  | 1           | orders    | department_id | 订单表的部门字段
2  | 1           | customers | dept_id       | 客户表的部门字段
3  | 2           | orders    | region_id     | 订单表的区域字段
```

通过此配置，框架可在查询时自动附加数据权限过滤条件。

---

## 8. 日志审计模块（可选）

### 8.1 操作日志表 (OperationLogs)

记录用户的关键操作。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `UserId` | BIGINT | 操作用户ID |
| `Username` | VARCHAR(50) | 操作用户名（冒余存储，便于查询） |
| `Module` | VARCHAR(50) | 操作模块（如 USER/ROLE/PERMISSION） |
| `Action` | VARCHAR(50) | 操作类型（如 CREATE/UPDATE/DELETE） |
| `TargetId` | VARCHAR(100) | 操作目标ID |
| `TargetName` | VARCHAR(200) | 操作目标名称 |
| `BeforeData` | TEXT | 操作前数据（JSON） |
| `AfterData` | TEXT | 操作后数据（JSON） |
| `IpAddress` | VARCHAR(50) | 操作者IP地址 |
| `UserAgent` | VARCHAR(500) | 浏览器UA |
| `CreateTime` | DATETIME | 操作时间 |

**索引：** `INDEX(UserId)`, `INDEX(Module)`, `INDEX(CreateTime)`

### 8.2 登录日志表 (LoginLogs)

记录用户的登录记录。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `UserId` | BIGINT | 用户ID（登录失败时可能为NULL） |
| `Username` | VARCHAR(50) | 登录用户名 |
| `LoginStatus` | TINYINT | 登录状态：0-失败，1-成功 |
| `FailReason` | VARCHAR(200) | 失败原因（如 密码错误/账号禁用） |
| `IpAddress` | VARCHAR(50) | 登录IP地址 |
| `Location` | VARCHAR(100) | 登录地点（根据IP解析） |
| `Browser` | VARCHAR(100) | 浏览器类型 |
| `Os` | VARCHAR(100) | 操作系统 |
| `UserAgent` | VARCHAR(500) | 完整UA |
| `CreateTime` | DATETIME | 登录时间 |

**索引：** `INDEX(UserId)`, `INDEX(Username)`, `INDEX(CreateTime)`

### 8.3 权限变更日志表 (PermissionChangeLogs)

记录权限配置的变更，便于审计追溯。

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | BIGINT | 主键 |
| `OperatorId` | BIGINT | 操作人用户ID |
| `OperatorName` | VARCHAR(50) | 操作人用户名 |
| `ChangeType` | VARCHAR(50) | 变更类型（如 ROLE_PERMISSION/USER_ROLE/DATA_SCOPE） |
| `Action` | VARCHAR(20) | 操作：ADD/REMOVE/UPDATE |
| `TargetType` | VARCHAR(50) | 目标类型（USER/ROLE） |
| `TargetId` | BIGINT | 目标ID |
| `TargetName` | VARCHAR(100) | 目标名称 |
| `BeforeData` | TEXT | 变更前（JSON） |
| `AfterData` | TEXT | 变更后（JSON） |
| `Remark` | VARCHAR(500) | 备注 |
| `CreateTime` | DATETIME | 变更时间 |

**索引：** `INDEX(OperatorId)`, `INDEX(TargetType, TargetId)`, `INDEX(CreateTime)`

---

## 9. 索引建议汇总

| 表名 | 索引字段 | 索引类型 |
|------|---------|---------|
| Users | Username | UNIQUE |
| Users | Email | UNIQUE |
| Roles | RoleCode | UNIQUE |
| UserRoles | (UserId, RoleId) | UNIQUE |
| ApiPermissions | ApiCode | UNIQUE |
| ApiPermissions | (ApiEndpoint, HttpMethod) | UNIQUE |
| ApiGroups | GroupCode | UNIQUE |
| ApiGroupMappings | (ApiId, GroupId) | UNIQUE |
| RoleApiPermissions | (RoleId, PermissionId, PermissionType) | UNIQUE |
| FeaturePermissions | FeatureCode | UNIQUE |
| RoleFeaturePermissions | (RoleId, FeatureId) | UNIQUE |
| DataDimensions | DimensionCode | UNIQUE |
| DataScopes | (DimensionId, ScopeCode) | UNIQUE |
| RoleDataScopes | (RoleId, ScopeId) | UNIQUE |
| RoleDataScopeFlags | (RoleId, DimensionId) | UNIQUE |
| FeatureApiMappings | (FeatureId, ApiId) | UNIQUE |
