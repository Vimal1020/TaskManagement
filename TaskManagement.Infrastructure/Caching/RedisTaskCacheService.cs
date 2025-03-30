using StackExchange.Redis;
using System.Text.Json;
using TaskManagement.Core.Dto;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Caching
{
    public class RedisTaskCacheService : ITaskCacheService
    {
        private readonly IDatabase _redis;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        public RedisTaskCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        private string GenerateCacheKey(int userId, int page, int pageSize, TaskStatusEnum? status) =>
            $"user_tasks:{userId}:{page}:{pageSize}:{status}";

        public async Task<PaginatedResult<TaskEntity>> GetUserTasksAsync(int userId, int page, int pageSize, TaskStatusEnum? status)
        {
            var cacheKey = GenerateCacheKey(userId, page, pageSize, status);
            var cached = await _redis.StringGetAsync(cacheKey);

            return cached.IsNullOrEmpty
                ? null
                : JsonSerializer.Deserialize<PaginatedResult<TaskEntity>>(cached);
        }

        public async Task SetUserTasksAsync(int userId, int page, int pageSize, TaskStatusEnum? status, PaginatedResult<TaskEntity> tasks)
        {
            var cacheKey = GenerateCacheKey(userId, page, pageSize, status);
            var json = JsonSerializer.Serialize(tasks);
            await _redis.StringSetAsync(cacheKey, json, _cacheDuration);
        }

        public async Task InvalidateUserTasksCacheAsync(int userId)
        {
            var keysPattern = $"user_tasks:{userId}:*";
            var endpoints = _redis.Multiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.Multiplexer.GetServer(endpoint);
                foreach (var key in server.Keys(pattern: keysPattern))
                {
                    await _redis.KeyDeleteAsync(key);
                }
            }
        }
    }
}