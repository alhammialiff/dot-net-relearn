using Microsoft.AspNetCore.Mvc;

public class WeatherForecast
{

    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }

    public WeatherForecast(DateTime date, int temperatureC, string summary)
    {
        Date = date;
        TemperatureC = temperatureC;
        Summary = summary;
    }

}


[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy",
        "Hot", "Sweltering", "Scorching"
    };

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {

        // Declare random generator
        var rng = new Random();

        // Return an array(5) that contain instances of WeatherForecast
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast(
            DateTime.Now.AddDays(index),
            rng.Next(-20, 55),
            Summaries[rng.Next(Summaries.Length)]
        )).ToArray();

    }


    [HttpPost]
    public IActionResult Post([FromBody] WeatherForecast forecast)
    {
        return Ok(forecast);
    }


    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] WeatherForecast forecast)
    {

        // Update data for the given ID
        // Example: Find and update an item with a matching ID
        // var existingForecast = /* Fetch the data */;
        // existingForecast.Date = forecast.Date;
        return NoContent();

    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id) {

        // Delete data for the given ID
        return NoContent();

    }

}



