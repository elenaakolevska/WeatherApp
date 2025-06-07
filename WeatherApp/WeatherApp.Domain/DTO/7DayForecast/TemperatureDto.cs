using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Domain.DTO._7DayForecast
{
    public class TemperatureDto
    {
        public double Day { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Night { get; set; }
    }
}
