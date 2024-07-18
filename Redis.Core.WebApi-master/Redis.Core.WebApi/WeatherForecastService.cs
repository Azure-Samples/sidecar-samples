using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Redis.Core.WebApi.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redis.Core.WebApi
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<WeatherForecastService> _logger;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private const string WeatherData_Key = "Weather_Data";

        public WeatherForecastService(IDistributedCache cache,
            ILogger<WeatherForecastService> logger)
        {
            _cache = cache;
            _logger = logger;
        }
        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecasts()
        {
            var dataFromCache = await _cache.GetRecordAsync<IEnumerable<WeatherForecast>>(WeatherData_Key);
            if (dataFromCache != null)
            {
                _logger.LogInformation("Data loaded from cache @"+DateTime.Now);
                return dataFromCache;
            }
            else
            {
                _logger.LogInformation("Data loaded from db @"+DateTime.Now);
                var dataFromDb = GetDataFromDB();
                await _cache.SetRecordAsync<IEnumerable<WeatherForecast>>(dataFromDb, WeatherData_Key);
                return dataFromDb;
            }
        }

        public IEnumerable<WeatherForecast> GetDataFromDB()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
