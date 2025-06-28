using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Domain.DTO.CurrentWeatherDto
{
    public class WeatherApiResponseDto
    {
        public List<WeatherDescriptionDto>? Weather { get; set; }
        public MainDto? Main { get; set; }
        public WindDto? Wind { get; set; }
        public long Dt { get; set; }
        public string? Name { get; set; }
    }
}
