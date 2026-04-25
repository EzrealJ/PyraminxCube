# RBAC 前端实现选型

> 本文档作为 RBAC 权限管理系统前端实现的技术选型索引

---

## 1. 技术栈概览

```
┌─────────────────────────────────────────────────────────────┐
│                    前端技术栈                                │
├─────────────────────────────────────────────────────────────┤
│  框架        │  Vue 3 + TypeScript                          │
├─────────────────────────────────────────────────────────────┤
│  UI 库       │  Ant Design Vue 4.x                          │
├─────────────────────────────────────────────────────────────┤
│  构建工具    │  Vite                                        │
├─────────────────────────────────────────────────────────────┤
│  状态管理    │  Pinia                                       │
├─────────────────────────────────────────────────────────────┤
│  路由        │  Vue Router 4.x                              │
├─────────────────────────────────────────────────────────────┤
│  HTTP 请求   │  Axios                                       │
├─────────────────────────────────────────────────────────────┤
│  代码规范    │  ESLint + Prettier                           │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. 核心依赖

### 2.1 基础框架

| 依赖 | 版本 | 用途 |
|------|------|------|
| vue | ^3.4+ | 核心框架 |
| typescript | ^5.0+ | 类型支持 |
| vite | ^5.0+ | 构建工具 |

### 2.2 UI 与样式

| 依赖 | 版本 | 用途 |
|------|------|------|
| ant-design-vue | ^4.x | UI 组件库 |
| @ant-design/icons-vue | ^7.x | 图标库 |

### 2.3 状态与路由

| 依赖 | 版本 | 用途 |
|------|------|------|
| pinia | ^2.x | 状态管理 |
| vue-router | ^4.x | 路由管理 |

### 2.4 工具库

| 依赖 | 版本 | 用途 |
|------|------|------|
| axios | ^1.x | HTTP 请求 |
| dayjs | ^1.x | 日期处理 |
| lodash-es | ^4.x | 工具函数 |

---

## 3. 项目结构

```
src/
├── api/                      # API 接口定义
│   ├── user.ts
│   ├── role.ts
│   ├── permission.ts
│   └── types/                # API 类型定义
│
├── assets/                   # 静态资源
│   ├── images/
│   └── styles/
│
├── components/               # 公共组件
│   ├── Permission/           # 权限相关组件
│   │   ├── PermissionTree.vue      # 权限树组件
│   │   ├── DataScopeSelect.vue     # 数据权限选择
│   │   └── RoleTransfer.vue        # 角色分配穿梭框
│   └── common/               # 通用组件
│
├── composables/              # 组合式函数
│   ├── usePermission.ts      # 权限相关 hooks
│   └── useDataScope.ts       # 数据权限 hooks
│
├── directives/               # 自定义指令
│   └── permission.ts         # v-permission 指令
│
├── layouts/                  # 布局组件
│   ├── BasicLayout.vue
│   └── BlankLayout.vue
│
├── router/                   # 路由配置
│   ├── index.ts
│   ├── routes.ts
│   └── guard.ts              # 路由守卫
│
├── stores/                   # Pinia 状态管理
│   ├── user.ts               # 用户状态
│   ├── permission.ts         # 权限状态
│   └── app.ts                # 应用状态
│
├── utils/                    # 工具函数
│   ├── request.ts            # Axios 封装
│   ├── auth.ts               # 认证相关
│   └── permission.ts         # 权限判断工具
│
├── views/                    # 页面组件
│   ├── login/
│   ├── dashboard/
│   └── system/               # 系统管理
│       ├── user/             # 用户管理
│       ├── role/             # 角色管理
│       ├── permission/       # 权限管理
│       │   ├── api/          # API 权限
│       │   ├── feature/      # 功能权限
│       │   └── data/         # 数据权限
│       └── menu/             # 菜单管理
│
├── App.vue
└── main.ts
```

---

## 4. 权限控制实现

### 4.1 权限数据获取

```typescript
// stores/permission.ts
import { defineStore } from 'pinia'
import { getUserPermissions } from '@/api/permission'

export const usePermissionStore = defineStore('permission', {
  state: () => ({
    // API 权限列表
    apiPermissions: [] as string[],
    // 功能按钮权限列表
    featurePermissions: [] as string[],
    // 数据权限范围
    dataScopes: {} as Record<string, string[]>,
  }),
  
  actions: {
    async fetchPermissions() {
      const { data } = await getUserPermissions()
      this.apiPermissions = data.apiPermissions
      this.featurePermissions = data.featurePermissions
      this.dataScopes = data.dataScopes
    },
    
    // 检查是否有某个功能权限
    hasPermission(code: string): boolean {
      return this.featurePermissions.includes(code)
    },
  },
})
```

### 4.2 按钮权限指令

```typescript
// directives/permission.ts
import type { Directive } from 'vue'
import { usePermissionStore } from '@/stores/permission'

export const permission: Directive<HTMLElement, string | string[]> = {
  mounted(el, binding) {
    const permissionStore = usePermissionStore()
    const value = binding.value
    
    const codes = Array.isArray(value) ? value : [value]
    const hasPermission = codes.some(code => 
      permissionStore.hasPermission(code)
    )
    
    if (!hasPermission) {
      el.parentNode?.removeChild(el)
    }
  },
}

// 使用方式
// <a-button v-permission="'user:add'">新增用户</a-button>
// <a-button v-permission="['user:edit', 'user:update']">编辑</a-button>
```

### 4.3 路由权限守卫

```typescript
// router/guard.ts
import { usePermissionStore } from '@/stores/permission'
import { useUserStore } from '@/stores/user'

router.beforeEach(async (to, from, next) => {
  const userStore = useUserStore()
  const permissionStore = usePermissionStore()
  
  // 白名单页面直接放行
  if (whiteList.includes(to.path)) {
    next()
    return
  }
  
  // 未登录跳转登录页
  if (!userStore.token) {
    next(`/login?redirect=${to.path}`)
    return
  }
  
  // 已登录但未获取权限信息
  if (!permissionStore.featurePermissions.length) {
    await permissionStore.fetchPermissions()
  }
  
  // 检查页面访问权限
  if (to.meta.permission) {
    const hasAccess = permissionStore.hasPermission(to.meta.permission as string)
    if (!hasAccess) {
      next('/403')
      return
    }
  }
  
  next()
})
```

### 4.4 权限组合式函数

```typescript
// composables/usePermission.ts
import { usePermissionStore } from '@/stores/permission'

export function usePermission() {
  const permissionStore = usePermissionStore()
  
  /**
   * 检查是否有权限
   */
  function hasPermission(code: string | string[]): boolean {
    const codes = Array.isArray(code) ? code : [code]
    return codes.some(c => permissionStore.hasPermission(c))
  }
  
  /**
   * 检查是否有全部权限
   */
  function hasAllPermissions(codes: string[]): boolean {
    return codes.every(c => permissionStore.hasPermission(c))
  }
  
  return {
    hasPermission,
    hasAllPermissions,
  }
}

// 使用方式
// const { hasPermission } = usePermission()
// if (hasPermission('user:delete')) { ... }
```

---

## 5. 核心页面组件

### 5.1 权限树组件

用于功能权限的树形展示和勾选。

```vue
<!-- components/Permission/PermissionTree.vue -->
<template>
  <a-tree
    v-model:checkedKeys="checkedKeys"
    :tree-data="treeData"
    checkable
    :field-names="{ key: 'id', title: 'name', children: 'children' }"
    @check="handleCheck"
  />
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { TreeProps } from 'ant-design-vue'

interface Props {
  modelValue: number[]
  treeData: TreeProps['treeData']
}

const props = defineProps<Props>()
const emit = defineEmits<{
  'update:modelValue': [value: number[]]
}>()

const checkedKeys = ref<number[]>(props.modelValue)

watch(() => props.modelValue, (val) => {
  checkedKeys.value = val
})

function handleCheck(checked: number[]) {
  emit('update:modelValue', checked)
}
</script>
```

### 5.2 角色分配穿梭框

用于给用户分配角色。

```vue
<!-- components/Permission/RoleTransfer.vue -->
<template>
  <a-transfer
    v-model:target-keys="targetKeys"
    :data-source="roleList"
    :titles="['未分配角色', '已分配角色']"
    :render="item => item.roleName"
    :row-key="item => item.id"
    @change="handleChange"
  />
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

interface Role {
  id: number
  roleName: string
}

interface Props {
  modelValue: number[]
  roleList: Role[]
}

const props = defineProps<Props>()
const emit = defineEmits<{
  'update:modelValue': [value: number[]]
}>()

const targetKeys = ref<number[]>(props.modelValue)

watch(() => props.modelValue, (val) => {
  targetKeys.value = val
})

function handleChange(keys: number[]) {
  emit('update:modelValue', keys)
}
</script>
```

### 5.3 数据权限选择

用于给角色分配数据权限范围。

```vue
<!-- components/Permission/DataScopeSelect.vue -->
<template>
  <div class="data-scope-select">
    <div v-for="dimension in dimensions" :key="dimension.code" class="dimension-item">
      <div class="dimension-header">
        <span>{{ dimension.name }}</span>
        <a-radio-group v-model:value="scopeFlags[dimension.code]" size="small">
          <a-radio-button value="CUSTOM">自定义</a-radio-button>
          <a-radio-button value="ALL">全部</a-radio-button>
          <a-radio-button value="SELF">仅自己</a-radio-button>
        </a-radio-group>
      </div>
      
      <a-tree-select
        v-if="scopeFlags[dimension.code] === 'CUSTOM'"
        v-model:value="selectedScopes[dimension.code]"
        :tree-data="dimension.scopes"
        multiple
        tree-checkable
        :field-names="{ label: 'name', value: 'id', children: 'children' }"
        placeholder="请选择数据范围"
        style="width: 100%"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

interface Dimension {
  code: string
  name: string
  scopes: any[]
}

interface Props {
  dimensions: Dimension[]
  modelValue: {
    flags: Record<string, string>
    scopes: Record<string, number[]>
  }
}

const props = defineProps<Props>()
const emit = defineEmits<{
  'update:modelValue': [value: Props['modelValue']]
}>()

const scopeFlags = ref<Record<string, string>>(props.modelValue.flags)
const selectedScopes = ref<Record<string, number[]>>(props.modelValue.scopes)

watch([scopeFlags, selectedScopes], () => {
  emit('update:modelValue', {
    flags: scopeFlags.value,
    scopes: selectedScopes.value,
  })
}, { deep: true })
</script>
```

---

## 6. API 封装

### 6.1 Axios 基础配置

```typescript
// utils/request.ts
import axios from 'axios'
import { message } from 'ant-design-vue'
import { useUserStore } from '@/stores/user'
import router from '@/router'

const request = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
})

// 请求拦截器
request.interceptors.request.use((config) => {
  const userStore = useUserStore()
  if (userStore.token) {
    config.headers.Authorization = `Bearer ${userStore.token}`
  }
  return config
})

// 响应拦截器
request.interceptors.response.use(
  (response) => response.data,
  (error) => {
    const { status } = error.response || {}
    
    switch (status) {
      case 401:
        message.error('登录已过期，请重新登录')
        router.push('/login')
        break
      case 403:
        message.error('没有权限访问该资源')
        break
      default:
        message.error(error.message || '请求失败')
    }
    
    return Promise.reject(error)
  }
)

export default request
```

### 6.2 权限相关 API

```typescript
// api/permission.ts
import request from '@/utils/request'

// 获取当前用户权限
export function getUserPermissions() {
  return request.get('/api/permissions/current')
}

// 获取功能权限树
export function getFeatureTree() {
  return request.get('/api/permissions/features/tree')
}

// 获取 API 权限列表
export function getApiPermissions(params?: any) {
  return request.get('/api/permissions/apis', { params })
}

// 获取数据维度列表
export function getDataDimensions() {
  return request.get('/api/permissions/dimensions')
}

// 获取维度值列表
export function getDataScopes(dimensionId: number) {
  return request.get(`/api/permissions/dimensions/${dimensionId}/scopes`)
}
```

---

## 7. 可参考的开源项目

| 项目 | 参考价值 | 链接 |
|------|---------|------|
| **Vben Admin** | 完整的 Vue 3 后台框架，权限实现 | https://github.com/vbenjs/vue-vben-admin |
| **Pure Admin** | 轻量级 Vue 3 后台模板 | https://github.com/pure-admin/vue-pure-admin |
| **Ant Design Pro Vue** | Ant Design Vue 官方后台模板 | https://github.com/vueComponent/ant-design-vue-pro |

---

## 8. 开发路线图

### Phase 1：基础框架
- [ ] 项目初始化（Vite + Vue 3 + TypeScript）
- [ ] Ant Design Vue 集成
- [ ] 路由和布局搭建
- [ ] Axios 封装和 API 规范

### Phase 2：权限基础
- [ ] 用户登录/登出
- [ ] 权限数据获取和存储
- [ ] 路由守卫实现
- [ ] v-permission 指令

### Phase 3：权限管理页面
- [ ] 用户管理（CRUD + 角色分配）
- [ ] 角色管理（CRUD + 权限分配）
- [ ] API 权限管理
- [ ] 功能权限管理（树形）

### Phase 4：数据权限
- [ ] 数据维度管理
- [ ] 数据范围值管理
- [ ] 角色数据权限配置

### Phase 5：优化完善
- [ ] 权限缓存优化
- [ ] 页面加载优化
- [ ] 用户体验优化

---

## 9. 关键设计决策总结

| 决策点 | 选择 | 理由 |
|-------|------|------|
| 框架 | Vue 3 | 国内生态好，上手快 |
| UI 库 | Ant Design Vue | Tree/Transfer 组件成熟，适合权限管理 |
| 构建工具 | Vite | 速度快，开发体验好 |
| 状态管理 | Pinia | Vue 3 官方推荐，简洁易用 |
| 权限控制 | 指令 + 路由守卫 | 声明式 + 自动化，简化开发 |
