using System;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using IO.Swagger.Services;
using IO.Swagger.Models;
using Microsoft.Extensions.Logging;
using static IO.Swagger.Models.MathRequest;
using System.Threading.Tasks;


namespace IO.Swagger.Tests
{
    public class CalculatorServiceTests
    {
        private readonly CalculatorOrCacheService _calculatorOrCacheService;

        public CalculatorServiceTests()
        {
            _calculatorOrCacheService = new CalculatorOrCacheService(
                new MemoryCache(new MemoryCacheOptions()),
                NullLogger<CalculatorOrCacheService>.Instance,
                new HttpClient()
            );

        }

        [Fact]
        public async Task Add_ShouldReturnCorrectSum()
        {
            MathRequest request = new MathRequest() { Operation = OperationEnum.AddEnum, X = 3, Y = 5 };
            MathResponse mathResponse = await _calculatorOrCacheService.AddAsync(request);
            Assert.Equal(8, mathResponse.Result);
        }

        [Fact]
        public async Task Subtract_ShouldReturnCorrectDifference()
        {
            MathRequest request = new MathRequest() { Operation = OperationEnum.SubtractEnum, X = 10, Y = 4 };
            MathResponse mathResponse = await _calculatorOrCacheService.SubtractAsync(request);
            Assert.Equal(6, mathResponse.Result);
        }

        [Fact]
        public async Task Multiply_ShouldReturnCorrectProduct()
        {
            MathRequest request = new MathRequest() { Operation = OperationEnum.MultiplyEnum, X = 3, Y = 4 };
            MathResponse mathResponse = await _calculatorOrCacheService.MultiplyAsync(request);
            Assert.Equal(12, mathResponse.Result);
        }

        [Fact]
        public async Task Divide_ShouldReturnCorrectQuotient()
        {
            MathRequest request = new MathRequest() { Operation = OperationEnum.DivideEnum, X = 12, Y = 3 };
            MathResponse mathResponse = await _calculatorOrCacheService.DivideAsync(request);
            Assert.Equal(4, mathResponse.Result);
        }

        [Fact]
        public async Task Divide_ByZero_ShouldThrowExceptionAsync()
        {
            MathRequest request = new MathRequest() { Operation = OperationEnum.DivideEnum, X = 5, Y = 0 };
            await Assert.ThrowsAsync<DivideByZeroException>(async () => await _calculatorOrCacheService.DivideAsync(request) );
        }
    }
}
