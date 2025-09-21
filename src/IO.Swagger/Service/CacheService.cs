using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace IO.Swagger.Services
{
    public class CacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly HttpClient _httpClient;

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger, HttpClient httpClient)
        {
            _cache = cache;
            _logger = logger;
            _httpClient = httpClient;
        }

        private int GetOrAdd(string operation, int x, int y, Func<int> factory)
        {
            string key = $"{operation}:{x}:{y}";

            if (_cache.TryGetValue(key, out int result))
            {
                _logger.LogInformation($"Cache HIT - {operation}({x},{y}) = {result}");
                return result;
            }

            result = factory();
            _cache.Set(key, result, TimeSpan.FromSeconds(30));

            _logger.LogInformation($"Cache MISS - {operation}({x},{y}), value calculated and cached");
            return result;
        }

        public int Add(int x, int y) => GetOrAdd("Add", x, y, () => x + y);
        public int Subtract(int x, int y) => GetOrAdd("Subtract", x, y, () => x - y);
        public int Multiply(int x, int y) => GetOrAdd("Multiply", x, y, () => x * y);

        public int Divide(int x, int y)
        {
            if (y == 0) throw new DivideByZeroException("Cannot divide by zero");
            return GetOrAdd("Divide", x, y, () => x / y);
        }

        public async Task<int> GetRandomFromMockoon()
        {
            var response = await _httpClient.GetAsync("http://localhost:3001/api/meta/cache-miss");
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<RandomResponse>(json);

            _logger.LogInformation("Fetched random value from Mockoon: {value}", obj?.Value);

            return obj?.Value ?? 0;
        }

        private class RandomResponse
        {
            public int Value { get; set; }
        }
    }
}
