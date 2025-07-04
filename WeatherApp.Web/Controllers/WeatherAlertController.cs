﻿using System.Diagnostics;
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

        public WeatherAlertController(
            IWeatherAlertService weatherAlertService,
            ILocationService locationService,
            IOutfitRecommendationService outfitRecommendationService,
            IRepository<UserSettings> userSettingsRepository,
            IWeatherApiService weatherApiService)
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
                        AlertDate = DateTime.UtcNow,
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

            if (userSettings == null)
            {
                return View(new List<WeatherAlert>());
            }

            ViewBag.UserLocation = userSettings.Location?.City ?? "Unknown";
            ViewBag.ReceiveAlerts = userSettings.ReceiveAlerts;

            if (!userSettings.ReceiveAlerts)
            {
                return View(new List<WeatherAlert>());
            }

            var weatherData = await _weatherApiService.GetCurrentWeatherAsync(userSettings.LocationId);

            if (weatherData != null)
            {
                var existingAlerts = _weatherAlertService.GetAll()
                    .Where(a => a.UserSettingsId == userSettings.Id &&
                                a.AlertDate.Date == DateTime.UtcNow.Date)
                    .ToList();

                if (!existingAlerts.Any())
                {
                    var alert = new WeatherAlert
                    {
                        UserSettingsId = userSettings.Id,
                        AlertDate = DateTime.UtcNow,
                        AlertType = _weatherAlertService.GenerateWeatherAlert(weatherData),
                        RecommendationText = _outfitRecommendationService.GenerateOutfitRecommendation(weatherData)
                    };

                    Debug.WriteLine($"[DEBUG] ALERT DATE: {alert.AlertDate}");
                    Debug.WriteLine($"[DEBUG] WEATHER DATA: {weatherData?.Date} - {weatherData?.TemperatureCelsius}°C - {weatherData?.Conditions}");

                    _weatherAlertService.Insert(alert);
                }
            }

            var userAlerts = _weatherAlertService.GetAll()
                .Where(a => a.UserSettingsId == userSettings.Id)
                .OrderByDescending(a => a.AlertDate)
                .ToList();

            return View(userAlerts);
        }

        [HttpPost]
        public IActionResult CreateWeatherAlert([FromBody] WeatherAlert alert)
        {
            if (alert == null)
                return BadRequest("Alert data is null.");

            // Ensure AlertDate is UTC
            if (alert.AlertDate.Kind != DateTimeKind.Utc)
                alert.AlertDate = alert.AlertDate.ToUniversalTime();

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
                // Ensure AlertDate is UTC
                if (alert.AlertDate.Kind != DateTimeKind.Utc)
                    alert.AlertDate = alert.AlertDate.ToUniversalTime();

                _weatherAlertService.Insert(alert);
                ViewBag.Locations = new SelectList(_locationService.GetAll(), "Id", "City");

                return RedirectToAction(nameof(Index));
            }
            return View(alert);
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

                if (existingAlert.UserSettings == null)
                {
                    ModelState.AddModelError("", "User settings not found for this alert");
                    return View(alert);
                }

                if (alert.AlertDate.Kind != DateTimeKind.Utc)
                    alert.AlertDate = DateTime.SpecifyKind(alert.AlertDate, DateTimeKind.Utc);

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

        public IActionResult CleanAllAlerts()
        {
            var all = _weatherAlertService.GetAll().ToList();
            foreach (var a in all)
                _weatherAlertService.DeleteById(a.Id);

            TempData["Message"] = "All alerts deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}