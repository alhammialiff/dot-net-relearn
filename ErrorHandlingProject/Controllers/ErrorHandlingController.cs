using Microsoft.AspNetCore.Mvc;
using System;


// 1. Declare Route for the controller 
[Route("api/[controller]")]
[ApiController]
public class ErrorHandlingController : ControllerBase
{
    // 2. Declare CRUD Methods
    [HttpGet("division")]
    public IActionResult GetDivisionResult(int numerator, int denominator)
    {
        try
        {
            var result = numerator / denominator;

            // 200 Response
            return Ok("Here's the result: " + result);
        }
        catch (DivideByZeroException)
        {
            Console.WriteLine("Error: Division by zero is not allowed.");

            // 400 Reponse
            return BadRequest("Cannot divide by zero.");
        }

    }
    
}