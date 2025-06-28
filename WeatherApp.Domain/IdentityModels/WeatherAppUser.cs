using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WeatherApp.Domain.DomainModels;

namespace WeatherApp.Domain.IdentityModels
{
    public class WeatherAppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public ICollection<UserSettings> UserSettings { get; set; } = new List<UserSettings>();
    }
}
