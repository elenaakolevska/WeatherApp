using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherApp.Domain.DomainModels;
using WeatherApp.Service.Interface;

[Authorize]

public class UserSettingsController : Controller
{
    private readonly IUserSettingsService _service;
    private readonly ILocationService _locationService;

    public UserSettingsController(IUserSettingsService service, ILocationService locationService)
    {
        _service = service;
        _locationService = locationService;
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
    public IActionResult Edit(int id, UserSettings userSettings)
    {
        if (id != userSettings.Id)
            return BadRequest();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        userSettings.UserId = userId; 

        if (ModelState.IsValid)
        {
            _service.Update(userSettings);
            return RedirectToAction(nameof(Index));
        }

        var locations = _locationService.GetAll();
        ViewBag.Locations = locations;
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
