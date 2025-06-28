using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Domain.DomainModels
{
    public class WeatherData : BaseEntity
    {
        public DateTime Date { get; set; }
        public double TemperatureCelsius { get; set; }
        public double Humidity { get; set; }
        public string? Conditions { get; set; }
        public int LocationId { get; set; }
        public Location? Location { get; set; }
    }
}
