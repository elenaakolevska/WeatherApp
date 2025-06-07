using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Domain.IdentityModels;

namespace WeatherApp.Domain.DomainModels
{
    public class UserSettings : BaseEntity
    {
        public int LocationId { get; set; }
        public Location? Location { get; set; }

        public string TemperatureUnit { get; set; } = "Celsius";
        public bool ReceiveAlerts { get; set; }

        public string? UserId { get; set; }  
        public WeatherAppUser? User { get; set; }      
    }

}
