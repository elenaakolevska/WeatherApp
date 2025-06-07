using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Domain.DTO.CurrentWeatherDto;

namespace WeatherApp.Domain.DTO._7DayForecast
{
    public class DailyForecastDto
    {
        public long Dt { get; set; }
        public TemperatureDto? Temp { get; set; }
        public List<WeatherDescriptionDto>? Weather { get; set; }
        public int Humidity { get; set; }
        public double Wind_speed { get; set; }
    }
}
