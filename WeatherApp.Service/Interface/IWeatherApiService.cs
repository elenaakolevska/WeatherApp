using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Domain.DomainModels;

namespace WeatherApp.Service.Interface
{
    public interface IWeatherApiService
    {
        Task<WeatherData?> GetCurrentWeatherAsync(int locationId);
        Task<IEnumerable<WeatherData>> Get7DayForecastAsync(int locationId);
        Task<WeatherData> GetWeatherForDateAsync(int locationId, DateTime date);
       
    }
}
