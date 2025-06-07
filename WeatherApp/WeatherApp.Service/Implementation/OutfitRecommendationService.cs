using System;
using System.Text;
using WeatherApp.Domain.DomainModels;
using WeatherApp.Service.Interface;

namespace WeatherApp.Service.Implementation
{
    public class OutfitRecommendationService : IOutfitRecommendationService
    {
      

        public string GenerateOutfitRecommendation(WeatherData weatherData)
        {
            var recommendation = new StringBuilder();
            double temp = weatherData.TemperatureCelsius;

            if (temp < -5)
                recommendation.Append("❄️ Extreme cold warning! Wear a heavy winter coat, thermal layers, hat, and gloves. ");
            else if (temp < 0)
                recommendation.Append("🧥 Freezing temperatures! Wear a warm coat, scarf, and boots. ");
            else if (temp < 10)
                recommendation.Append("🧣 Chilly weather! Wear a sweater, long pants, and a windbreaker. ");
            else if (temp < 20)
                recommendation.Append("👕 Mild weather! A light jacket or hoodie should suffice. ");
            else if (temp < 30)
                recommendation.Append("👚 Warm weather! T-shirt, shorts, and hat are recommended. ");
            else
                recommendation.Append("🔥 Heat warning! Wear light, breathable clothing and stay hydrated. ");

            if (!string.IsNullOrEmpty(weatherData.Conditions))
            {
                var condition = weatherData.Conditions.ToLower();
                if (condition.Contains("rain"))
                    recommendation.Append("🌧️ Bring an umbrella or raincoat. ");
                if (condition.Contains("wind"))
                    recommendation.Append("💨 Windy conditions - wear a wind-resistant jacket. ");
                if (condition.Contains("snow"))
                    recommendation.Append("⛄ Snow expected - wear waterproof boots and warm layers. ");
                if (condition.Contains("sun") || condition.Contains("clear"))
                    recommendation.Append("☀️ Don't forget sunscreen and sunglasses! ");
            }

            return recommendation.ToString();
        }

        
    }
}