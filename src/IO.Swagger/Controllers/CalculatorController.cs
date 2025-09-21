using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IO.Swagger.Services;
using System.Threading.Tasks;

namespace IO.Swagger.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class CalculatorController : ControllerBase
    {
        private readonly CacheService _cacheService;

        public CalculatorController(CacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet("add")]
        public IActionResult Add(int x, int y) => Ok(_cacheService.Add(x, y));

        [HttpGet("subtract")]
        public IActionResult Subtract(int x, int y) => Ok(_cacheService.Subtract(x, y));

        [HttpGet("multiply")]
        public IActionResult Multiply(int x, int y) => Ok(_cacheService.Multiply(x, y));

        [HttpGet("divide")]
        public IActionResult Divide(int x, int y) => Ok(_cacheService.Divide(x, y));

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomFromMockoon()
        {
            var value = await _cacheService.GetRandomFromMockoon();
            return Ok(value);
        }

    }
}
