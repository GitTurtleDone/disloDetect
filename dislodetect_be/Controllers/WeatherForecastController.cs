using Microsoft.AspNetCore.Mvc;

namespace dislodetect_be.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
        /*do
        var weatherForecasts = new[]
        {
            new { Date = "2023-09-24", TemperatureC = 25, Summary = "Sunny" },
            new { Date = "2023-09-25", TemperatureC = 22, Summary = "Partly Cloudy" },
            // Add more forecast items here
        };

        return Ok(weatherForecasts);
        */
    }
}
