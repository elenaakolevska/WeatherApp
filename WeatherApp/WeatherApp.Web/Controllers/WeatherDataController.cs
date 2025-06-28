using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherApp.Domain.DomainModels;
using WeatherApp.Service.Interface;

namespace WeatherApp.Web.Controllers
{
    [AllowAnonymous]

    public class WeatherDataController : Controller
    {
        private readonly IWeatherDataService _weatherDataService;
        private readonly ILocationService _locationService;
        private readonly IWeatherApiService _weatherApiService;

        public WeatherDataController(
            IWeatherDataService weatherDataService,
            ILocationService locationService,
            IWeatherApiService weatherApiService)
        {
            _weatherDataService = weatherDataService;
            _locationService = locationService;
            _weatherApiService = weatherApiService;
        }

        public IActionResult Index(int? locationId)
        {
            var allData = _weatherDataService.GetAll();

            var model = new WeatherViewModel
            {
                Locations = _locationService.GetAll(),
                SelectedLocationId = locationId
            };

            if (locationId.HasValue)
            {
                model.Current = allData
                    .Where(x => x.LocationId == locationId.Value && x.Date == DateTime.UtcNow.Date)
                    .ToList();

                model.Forecast = allData
                    .Where(x => x.LocationId == locationId.Value && x.Date > DateTime.UtcNow.Date)
                    .ToList();
            }
            else
            {
                model.Current = allData.Where(x => x.Date == DateTime.UtcNow.Date).ToList();
                model.Forecast = allData.Where(x => x.Date > DateTime.UtcNow.Date).ToList();
            }

            return View(model);
        }




        public IActionResult Details(int id)
        {
            var data = _weatherDataService.GetById(id);
            if (data == null) return NotFound();
            return View(data);
        }

        public async Task<IActionResult> RefreshCurrent(int locationId)
        {
            await _weatherApiService.GetCurrentWeatherAsync(locationId);

            var currentData = _weatherDataService.GetAll()
                .Where(x => x.LocationId == locationId && x.Date == DateTime.UtcNow.Date)
                .ToList();

            var allForecastData = _weatherDataService.GetAll()
                .Where(x => x.Date > DateTime.UtcNow.Date)
                .ToList();

            var model = new WeatherViewModel
            {
                Current = currentData,          
                Forecast = allForecastData,     
                Locations = _locationService.GetAll(),
                SelectedLocationId = locationId
            };

            return View("Index", model);
        }

        public async Task<IActionResult> Refresh7Day(int locationId)
        {
            await _weatherApiService.Get7DayForecastAsync(locationId);

            var allCurrentData = _weatherDataService.GetAll()
                .Where(x => x.Date == DateTime.UtcNow.Date)
                .ToList();

            var forecastData = _weatherDataService.GetAll()
                .Where(x => x.LocationId == locationId && x.Date > DateTime.UtcNow.Date)
                .ToList();

            var model = new WeatherViewModel
            {
                Current = allCurrentData,      
                Forecast = forecastData,       
                Locations = _locationService.GetAll(),
                SelectedLocationId = locationId
            };

            return View("Index", model);
        }



        public IActionResult Delete(int id)
        {
            var data = _weatherDataService.GetById(id);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _weatherDataService.DeleteById(id);
            TempData["Message"] = "Forecast entry deleted.";
            return RedirectToAction("Index");
        }
    }
}
