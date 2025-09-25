using System;
using Microsoft.AspNetCore.Mvc;
using IO.Swagger.Models;
using IO.Swagger.Services;
using static IO.Swagger.Models.MathRequest;

namespace IO.Swagger.Controllers
{
    [ApiController]
    [Route("api/math")]
    public class CalculatorController : ControllerBase
    {
        private readonly CalculatorOrCacheService _calculatorService;

        public CalculatorController(CalculatorOrCacheService calculatorService)
        {
            _calculatorService = calculatorService;
        }

        [HttpPost("calculate")]
        public IActionResult Calculate([FromBody] MathRequest request, [FromHeader(Name = "X-ArithmeticOp-ID")] string arithmeticOpId)
        {
            if (string.IsNullOrWhiteSpace(arithmeticOpId))
                return BadRequest(new { Error = "Header X-ArithmeticOp-ID is required." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                MathResponse response = request.Operation switch
                {
                    OperationEnum.AddEnum      => _calculatorService.AddAsync(request).Result,
                    OperationEnum.SubtractEnum => _calculatorService.SubtractAsync(request).Result,
                    OperationEnum.MultiplyEnum => _calculatorService.MultiplyAsync(request).Result,
                    OperationEnum.DivideEnum   => _calculatorService.DivideAsync(request).Result,
                    _ => throw new InvalidOperationException("Unknown operation")
                };

                return Ok(response);
            }
            catch (DivideByZeroException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
