using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using PyraminxCube.Rbac.Core.Abstractions;
using PyraminxCube.Rbac.Core.Models;

namespace PyraminxCube.Rbac.EntityFrameworkCore.Services
{
    /// <summary>
    /// 权限缓存实现（支持分布式缓存和内存缓存）
    /// </summary>
    public class PermissionCache : IPermissionCache
    {
        private readonly IDistributedCache? _distributedCache;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

        // 内存缓存后备方案
        private static readonly ConcurrentDictionary<string, CacheEntry> _memoryCache = new();

        private const string CacheKeyPrefix = "rbac:permissions:";

        public PermissionCache(IDistributedCache? distributedCache = null)
        {
            _distributedCache = distributedCache;
        }

        public async Task<UserPermissions?> GetAsync(long userId, long tenantId, CancellationToken cancellationToken = default)
        {
            var key = GetCacheKey(userId, tenantId);

            // 优先使用分布式缓存
            if (_distributedCache != null)
            {
                var bytes = await _distributedCache.GetAsync(key, cancellationToken);
                if (bytes != null)
                {
                    return JsonSerializer.Deserialize<UserPermissions>(bytes);
                }
                return null;
            }

            // 使用内存缓存
            if (_memoryCache.TryGetValue(key, out var entry))
            {
                if (entry.ExpiresAt > DateTimeOffset.UtcNow)
                {
                    return entry.Value;
                }
                _memoryCache.TryRemove(key, out _);
            }
            return null;
        }

        public async Task SetAsync(UserPermissions permissions, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var key = GetCacheKey(permissions.UserId, permissions.TenantId);
            var exp = expiration ?? _defaultExpiration;

            // 优先使用分布式缓存
            if (_distributedCache != null)
            {
                var bytes = JsonSerializer.SerializeToUtf8Bytes(permissions);
                await _distributedCache.SetAsync(key, bytes, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = exp
                }, cancellationToken);
                return;
            }

            // 使用内存缓存
            _memoryCache[key] = new CacheEntry
            {
                Value = permissions,
                ExpiresAt = DateTimeOffset.UtcNow.Add(exp)
            };
        }

        public async Task RemoveAsync(long userId, long tenantId, CancellationToken cancellationToken = default)
        {
            var key = GetCacheKey(userId, tenantId);

            if (_distributedCache != null)
            {
                await _distributedCache.RemoveAsync(key, cancellationToken);
                return;
            }

            _memoryCache.TryRemove(key, out _);
        }

        public async Task RemoveByTenantAsync(long tenantId, CancellationToken cancellationToken = default)
        {
            var prefix = $"{CacheKeyPrefix}{tenantId}:";

            if (_distributedCache != null)
            {
                // 分布式缓存一般需要通过 Lua 脚本或 SCAN 命令来删除
                // 这里简单处理，实际使用时可以覆盖此实现
                await Task.CompletedTask;
                return;
            }

            // 内存缓存：删除所有匹配前缀的 key
            var keysToRemove = _memoryCache.Keys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var key in keysToRemove)
            {
                _memoryCache.TryRemove(key, out _);
            }
        }

        public async Task RemoveAllAsync(CancellationToken cancellationToken = default)
        {
            if (_distributedCache != null)
            {
                // 分布式缓存一般需要通过 Lua 脚本或 SCAN 命令来删除
                await Task.CompletedTask;
                return;
            }

            _memoryCache.Clear();
        }

        private static string GetCacheKey(long userId, long tenantId) => $"{CacheKeyPrefix}{tenantId}:{userId}";

        private class CacheEntry
        {
            public UserPermissions Value { get; set; } = null!;
            public DateTimeOffset ExpiresAt { get; set; }
        }
    }
}
