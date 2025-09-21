using System;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using IO.Swagger.Services;
using Microsoft.Extensions.Logging;


namespace IO.Swagger.Tests
{
    public class CalculatorServiceTests
    {
        private readonly CacheService _cacheService;

        public CalculatorServiceTests()
        {
            _cacheService = new CacheService(
                new MemoryCache(new MemoryCacheOptions()),
                NullLogger<CacheService>.Instance,
                new HttpClient()
            );
 
        }

        [Fact]
        public void Add_ShouldReturnCorrectSum()
        {
            int result = _cacheService.Add(3, 5);
            Assert.Equal(8, result);
        }

        [Fact]
        public void Subtract_ShouldReturnCorrectDifference()
        {
            int result = _cacheService.Subtract(10, 4);
            Assert.Equal(6, result);
        }

        [Fact]
        public void Multiply_ShouldReturnCorrectProduct()
        {
            int result = _cacheService.Multiply(3, 4);
            Assert.Equal(12, result);
        }

        [Fact]
        public void Divide_ShouldReturnCorrectQuotient()
        {
            int result = _cacheService.Divide(12, 3);
            Assert.Equal(4, result);
        }

        [Fact]
        public void Divide_ByZero_ShouldThrowException()
        {
            Assert.Throws<DivideByZeroException>(() => _cacheService.Divide(5, 0));
        }
    }
}
