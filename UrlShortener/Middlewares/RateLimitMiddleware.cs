using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace UrlShortener.Api.Middlewares
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private readonly int _requestsPerMinute;

        public RateLimitMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<RateLimitMiddleware> logger, IConfiguration config)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
            _requestsPerMinute = config.GetValue<int>("RateLimiting:RequestsPerMinute", 60);
        }

        public async Task Invoke(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var cacheKey = $"rl:{ip}";

            var entry = _cache.GetOrCreate(cacheKey, e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return new RateLimitEntry { Count = 0 };
            });

            if (entry == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Rate limiting internal error\"}");
                _logger.LogError("RateLimitEntry is null for {Ip}", ip);
                return;
            }

            if (entry.Count >= _requestsPerMinute)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Too many requests per minute\"}");
                _logger.LogWarning("Rate limit exceeded for {Ip}", ip);
                return;
            }

            entry.Count++;
            _cache.Set(cacheKey, entry, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            });

            await _next(context);
        }

        private class RateLimitEntry
        {
            public int Count { get; set; }
        }
    }
}
