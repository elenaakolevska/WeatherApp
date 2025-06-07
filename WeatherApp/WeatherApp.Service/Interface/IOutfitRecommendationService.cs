using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Domain.DomainModels;

namespace WeatherApp.Service.Interface
{
    public interface IOutfitRecommendationService
    {
        string GenerateOutfitRecommendation(WeatherData weatherData);
    }
}
