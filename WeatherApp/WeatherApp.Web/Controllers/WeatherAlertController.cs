using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Domain.DomainModels;
using WeatherApp.Repository.Interface;
using WeatherApp.Service.Implementation;
using WeatherApp.Service.Interface;

namespace WeatherApp.Web.Controllers
{
    [Authorize]

    public class WeatherAlertController : Controller
    {
        private readonly IWeatherAlertService _weatherAlertService;
        private readonly ILocationService _locationService;
        private readonly IWeatherApiService _weatherApiService;
        private readonly IOutfitRecommendationService _outfitRecommendationService;
        private readonly IRepository<UserSettings> _userSettingsRepository;


        public WeatherAlertController(IWeatherAlertService weatherAlertService, ILocationService locationService, IOutfitRecommendationService outfitRecommendationService, IRepository<UserSettings> userSettingsRepository, IWeatherApiService weatherApiService)
        {
            _weatherAlertService = weatherAlertService;
            _locationService = locationService;
            _outfitRecommendationService = outfitRecommendationService;
            _userSettingsRepository = userSettingsRepository;
            _weatherApiService = weatherApiService;
        }

        public async Task CheckAndCreateAlertsForAllUsers()
        {
            var allUserSettings = _userSettingsRepository.GetAll(
                   selector: x => x,  
                   include: x => x.Include(u => u.Location)  
               );
            foreach (var userSettings in allUserSettings)
            {
                var weatherData = await _weatherApiService.GetCurrentWeatherAsync(userSettings.LocationId);

                if (weatherData != null)
                {
                    var alert = new WeatherAlert
                    {
                        UserSettings = userSettings,
                        AlertDate = DateTime.Now,
                        AlertType = _weatherAlertService.GenerateWeatherAlert(weatherData),
                        RecommendationText = _outfitRecommendationService.GenerateOutfitRecommendation(weatherData)
                    };

                    _weatherAlertService.Insert(alert);
                }
            }
        }

        // GET: WeatherAlert
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userSettings = _userSettingsRepository.GetAll(
                selector: x => x,
                include: x => x.Include(u => u.Location))
                .FirstOrDefault(x => x.UserId == userId);

            if (userSettings?.Location != null)
            {
                ViewBag.UserLocation = userSettings.Location.City;

                var weatherData = await _weatherApiService.GetCurrentWeatherAsync(userSettings.LocationId);

                if (weatherData != null)
                {
                    var existingAlerts = _weatherAlertService.GetAll()
                        .Where(a => a.UserSettingsId == userSettings.Id &&
                                   a.AlertDate.Date == DateTime.Today)
                        .ToList();

                    if (!existingAlerts.Any())
                    {
                        var alert = new WeatherAlert
                        {
                            UserSettingsId = userSettings.Id,
                            AlertDate = DateTime.Now,
                            AlertType = _weatherAlertService.GenerateWeatherAlert(weatherData),
                            RecommendationText = _outfitRecommendationService.GenerateOutfitRecommendation(weatherData)
                        };
                        _weatherAlertService.Insert(alert);
                    }
                }

                var userAlerts = _weatherAlertService.GetAll()
                    .Where(a => a.UserSettingsId == userSettings.Id)
                    .OrderByDescending(a => a.AlertDate)
                    .ToList();

                return View(userAlerts);
            }

            return View(new List<WeatherAlert>());
        }

     

        [HttpPost]
        public IActionResult CreateWeatherAlert([FromBody] WeatherAlert alert)
        {
            if (alert == null)
                return BadRequest("Alert data is null.");

            try
            {
                var createdAlert = _weatherAlertService.Insert(alert);
                return CreatedAtAction(nameof(GetWeatherAlertById), new { id = createdAlert.Id }, createdAlert);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetWeatherAlertById/{id}")]
        public IActionResult GetWeatherAlertById(int id)
        {
            var alert = _weatherAlertService.GetById(id);
            if (alert == null)
                return NotFound();

            return Ok(alert);
        }

        // GET: WeatherAlert/Details/5
        public IActionResult Details(int id)
        {
            var alert = _weatherAlertService.GetById(id);
            if (alert == null) return NotFound();

            return View(alert);
        }

        // GET: WeatherAlert/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WeatherAlert/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(WeatherAlert alert)
        {
            if (ModelState.IsValid)
            {
                _weatherAlertService.Insert(alert);
                ViewBag.Locations = new SelectList(_locationService.GetAll(), "Id", "City");

                return RedirectToAction(nameof(Index));
            }
            return View(alert);
        }
        public IActionResult CleanInvalidAlerts()
        {
            var invalidAlerts = _weatherAlertService.GetAll()
                .Where(a => string.IsNullOrEmpty(a.AlertType) ||
                           string.IsNullOrEmpty(a.RecommendationText))
                .ToList();

            foreach (var alert in invalidAlerts)
            {
                _weatherAlertService.DeleteById(alert.Id);
            }

            TempData["Message"] = $"Removed {invalidAlerts.Count} invalid alerts";
            return RedirectToAction(nameof(Index));
        }

        // GET: WeatherAlert/Edit/5
        public IActionResult Edit(int id)
        {
            var alert = _weatherAlertService.GetById(id);
            if (alert == null) return NotFound();

            return View(alert);
        }

        // POST: WeatherAlert/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WeatherAlert alert)
        {
            if (id != alert.Id) return NotFound();

            try
            {
                var existingAlert = _weatherAlertService.GetByIdWithUserSettings(id);
                if (existingAlert == null) return NotFound();

                // Check if UserSettings exists
                if (existingAlert.UserSettings == null)
                {
                    ModelState.AddModelError("", "User settings not found for this alert");
                    return View(alert);
                }

                if (existingAlert.AlertDate.Date != alert.AlertDate.Date)
                {
                    var weatherData = await _weatherApiService.GetWeatherForDateAsync(
                        existingAlert.UserSettings.LocationId,
                        alert.AlertDate);

                    if (weatherData != null)
                    {
                        existingAlert.AlertType = _weatherAlertService.GenerateWeatherAlert(weatherData);
                        existingAlert.RecommendationText = _outfitRecommendationService.GenerateOutfitRecommendation(weatherData);
                    }
                    else
                    {
                        existingAlert.AlertType = "✅ Normal Conditions - No severe weather expected";
                        existingAlert.RecommendationText = "Check weather source manually";
                    }
                }

                existingAlert.AlertDate = alert.AlertDate;
                _weatherAlertService.Update(existingAlert);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error saving changes: {ex.Message}");
                return View(alert);
            }
        }


        // GET: WeatherAlert/Delete/5
        public IActionResult Delete(int id)
        {
            var alert = _weatherAlertService.GetByIdWithUserSettings(id);
            if (alert == null) return NotFound();

            return View(alert);
        }

        // POST: WeatherAlert/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _weatherAlertService.DeleteById(id);
                TempData["SuccessMessage"] = "Weather alert deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting alert: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
