using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Domain.DomainModels
{
    public class WeatherViewModel
    {
        public IEnumerable<WeatherData> Current { get; set; } = new List<WeatherData>();
        public IEnumerable<WeatherData> Forecast { get; set; } = new List<WeatherData>();
        public int? SelectedLocationId { get; set; }
        public IEnumerable<Location> Locations { get; set; } = new List<Location>();
    }

}
