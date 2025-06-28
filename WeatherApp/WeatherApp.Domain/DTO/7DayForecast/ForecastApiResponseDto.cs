using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Domain.DTO._7DayForecast
{
    public class ForecastApiResponseDto
    {
        public List<DailyForecastDto>? Daily { get; set; }
    }
}
