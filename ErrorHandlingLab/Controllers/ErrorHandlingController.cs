// Import ASP.NET Core MVC features
using Microsoft.AspNetCore.Mvc;

// Set the route for this controller to 'api/errorhandling'
[Route("api/[controller]")]
// Mark this class as an API controller (enables automatic model binding, validation, etc.)
[ApiController]
public class ErrorHandlingController : ControllerBase
{
    // Inject a logger for this controller
    private readonly ILogger<ErrorHandlingController> _logger;

    // Constructor receives the logger via dependency injection
    public ErrorHandlingController(ILogger<ErrorHandlingController> logger)
    {
        _logger = logger;
    }

    // GET endpoint: /api/errorhandling/division?numerator=10&denominator=2
    [HttpGet("division")]
    public IActionResult GetDivisionResult(int numerator, int denominator)
    {
        try
        {
            // Attempt to divide numerator by denominator
            var result = numerator / denominator;
            // Log the result at Information level
            _logger.LogInformation($"Result - {result}");
            // Return 200 OK with the result
            return Ok(result);
        }
        catch (DivideByZeroException)
        {
            // Log error to console and logger
            Console.WriteLine("Error: Division by zero is not allowed");
            _logger.LogError("Error: Division by zero is not allowed");
            // Return 400 Bad Request with error message
            return BadRequest("Error: Division by zero is not allowed");
        }
    }
}

