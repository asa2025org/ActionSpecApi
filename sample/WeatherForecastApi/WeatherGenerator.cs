using ASA.Core;
using ASA.Core.Models;

namespace WeatherForecastApi
{
    public class WeatherGenerator : IModule
    {
        public string Name => "WeatherForecastApi/weather-generator";
        public string Version => "1.0.0";

        public Task<StepOutput> ExecuteAsync(Dictionary<string, object> parameters, AsaExecutionContext context)
        {
            // Extract parameters with defaults
            int days = GetParameterValue(parameters, "days", 5);
            int minTemp = GetParameterValue(parameters, "min-temp", -20);
            int maxTemp = GetParameterValue(parameters, "max-temp", 55);
            string[] summaries = GetParameterValue(parameters, "summaries", 
                new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" });

            // Generate forecast data
            var forecast = Enumerable.Range(1, days).Select(index =>
                new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(minTemp, maxTemp),
                    summaries[Random.Shared.Next(summaries.Length)]
                )).ToArray();

            return Task.FromResult(new StepOutput
            {
                Success = true,
                Data = forecast
            });
        }

        private T GetParameterValue<T>(Dictionary<string, object> parameters, string key, T defaultValue)
        {
            if (parameters.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
                
                try
                {
                    // Try to convert the value
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    // If conversion fails, return default
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }
    }

    // WeatherForecast record - same as in the original sample
    public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}