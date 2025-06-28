using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Domain.DomainModels
{
    public class WeatherAlert : BaseEntity
    {
        public int UserSettingsId { get; set; }
        public UserSettings? UserSettings { get; set; }
        public string? AlertType { get; set; }
        public DateTime AlertDate { get; set; }
        public string? RecommendationText { get; set; }

    }
}
