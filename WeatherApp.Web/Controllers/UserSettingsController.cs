using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WeatherApp.Domain.DomainModels;
using WeatherApp.Service.Implementation;
using WeatherApp.Service.Interface;

[Authorize]
public class UserSettingsController : Controller
{
    private readonly IUserSettingsService _service;
    private readonly ILocationService _locationService;
    private readonly IWeatherAlertService _weatherAlertService;
    private readonly IWeatherApiService _weatherApiService;
    private readonly IOutfitRecommendationService _outfitRecommendationService;

    public UserSettingsController(
        IUserSettingsService service,
        ILocationService locationService,
        IWeatherAlertService weatherAlertService,
        IWeatherApiService weatherApiService,
        IOutfitRecommendationService outfitRecommendationService)
    {
        _service = service;
        _locationService = locationService;
        _weatherAlertService = weatherAlertService;
        _weatherApiService = weatherApiService;
        _outfitRecommendationService = outfitRecommendationService;
    }

    // GET: UserSettings
    public IActionResult Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Challenge();

        var settings = _service.GetByUserId(userId);
        if (settings == null || !settings.Any())
        {
            var defaultSetting = new UserSettings
            {
                UserId = userId,
                LocationId = 1,
                TemperatureUnit = "Celsius",
                ReceiveAlerts = false
            };
            _service.Insert(defaultSetting);

            settings = new List<UserSettings> { defaultSetting };
        }

        var locations = _locationService.GetAll();
        ViewBag.Locations = locations;

        return View(settings);
    }

    public IActionResult Edit(int id)
    {
        var settings = _service.GetById(id);
        if (settings == null)
            return NotFound();

        var locations = _locationService.GetAll();
        ViewBag.Locations = locations;

        return View(settings);
    }

    // POST: UserSettings/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserSettings userSettings)
    {
        if (id != userSettings.Id)
            return BadRequest();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        userSettings.UserId = userId;

        if (ModelState.IsValid)
        {
            _service.Update(userSettings);

            var currentWeather = await _weatherApiService.GetCurrentWeatherAsync(userSettings.LocationId);

            if (currentWeather != null)
            {
                _weatherAlertService.CleanInvalidAlerts();

                var existingAlerts = _weatherAlertService.GetAll()
                    .Where(a => a.UserSettingsId == userSettings.Id && a.AlertDate.Date == DateTime.UtcNow.Date)
                    .ToList();

                if (!existingAlerts.Any())
                {
                    var alert = new WeatherAlert
                    {
                        UserSettingsId = userSettings.Id,
                        AlertDate = DateTime.UtcNow,
                        AlertType = _weatherAlertService.GenerateWeatherAlert(currentWeather),
                        RecommendationText = _outfitRecommendationService.GenerateOutfitRecommendation(currentWeather)
                    };

                    _weatherAlertService.Insert(alert);
                }
                else
                {
                    foreach (var alert in existingAlerts)
                    {
                        alert.AlertType = _weatherAlertService.GenerateWeatherAlert(currentWeather);
                        alert.RecommendationText = _outfitRecommendationService.GenerateOutfitRecommendation(currentWeather);
                        _weatherAlertService.Update(alert);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Locations = _locationService.GetAll();
        return View(userSettings);
    }

    // GET: UserSettings/Details/5
    public IActionResult Details(int id)
    {
        var settings = _service.GetById(id);
        if (settings == null)
            return NotFound();

        return View(settings);
    }
}