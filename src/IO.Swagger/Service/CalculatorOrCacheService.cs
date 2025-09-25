using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using IO.Swagger.Models;
using Newtonsoft.Json;
using System.Net.Http;
using Confluent.Kafka;
using System.Threading.Tasks;

namespace IO.Swagger.Services
{
    public class CalculatorOrCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CalculatorOrCacheService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IProducer<Null, string> _kafkaProducer;

        public CalculatorOrCacheService(IMemoryCache cache, ILogger<CalculatorOrCacheService> logger, HttpClient httpClient)
        {
            _cache = cache;
            _logger = logger;
            _httpClient = httpClient;

            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092"
            };
            _kafkaProducer = new ProducerBuilder<Null, string>(config).Build();
        }

        private async Task<(decimal, bool)> GetOrAdd(string operation, decimal x, decimal y, Func<decimal> factory)
        {
            bool fromCache;
            string key = $"{operation}:{x}:{y}";

            #region Cache HIT
            if (_cache.TryGetValue(key, out decimal result))
            {
                _logger.LogInformation($"Cache HIT - {operation}({x},{y}) = {result}");
                fromCache = true;
                return (result, fromCache);
            }
            #endregion

            #region Moockoon

            var stubResponse = await _httpClient.GetStringAsync($"http://localhost:3000/api/meta/{operation}");
            _logger.LogInformation($"Cache MISS - Description from stub: {stubResponse}");

            #endregion

            #region Cache MISS
            result = factory();
            _cache.Set(key, result, TimeSpan.FromSeconds(30));

            _logger.LogInformation($"Cache MISS - {operation}({x},{y}), value calculated and cached");
            fromCache = false;
            #endregion

            #region Kafka
            
            var message = new
            {
                requestId = Guid.NewGuid(),
                operation = operation,
                x = x,
                y = y,
                result = result,
                timestamp = DateTime.UtcNow
            };
            _kafkaProducer.Produce("math-operations", new Message<Null, string> { Value = JsonConvert.SerializeObject(message) });
            #endregion

            return (result, fromCache);
        }

        public async Task<MathResponse> AddAsync(MathRequest request)
        {
            decimal result; 
            bool fromCache;

            (result, fromCache) = await GetOrAdd("add", request.X, request.Y, () => request.X + request.Y);
            return CreateResponse(request, result, "add", fromCache);
        }

        public async Task<MathResponse> SubtractAsync(MathRequest request)
        {
            decimal result;
            bool fromCache;

            (result, fromCache) = await GetOrAdd("subtract", request.X, request.Y, () => request.X - request.Y);
            return CreateResponse(request, result, "subtract", fromCache);
        }

        public async Task<MathResponse> MultiplyAsync(MathRequest request)
        {
            decimal result;
            bool fromCache;

            (result, fromCache) = await GetOrAdd("multiply", request.X, request.Y, () => request.X * request.Y);
            return CreateResponse(request, result, "multiply", fromCache);
        }

        public async Task<MathResponse> DivideAsync(MathRequest request)
        {
            if (request.Y == 0)
                throw new DivideByZeroException("Cannot divide by zero");

            decimal result;
            bool fromCache;

            (result, fromCache) = await GetOrAdd("divide", request.X, request.Y, () => request.X / request.Y);
            return CreateResponse(request, result, "divide", fromCache);
        }

        private MathResponse CreateResponse(MathRequest request, decimal result, string operation, bool fromCache)
        {
            return new MathResponse
            {
                RequestId = Guid.NewGuid().ToString(),
                Operation = operation,
                X = request.X,
                Y = request.Y,
                Result = result,
                FromCache = fromCache,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
