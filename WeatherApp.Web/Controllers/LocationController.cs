using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherApp.Domain.DomainModels;
using WeatherApp.Service.Interface;

namespace WeatherApp.Web.Controllers
{
    [Authorize]

    public class LocationController : Controller
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        public IActionResult Index()
        {
            
            var locations = _locationService.GetAll();

            return View(locations);
            
        }

        public IActionResult Details(int id)
        {
            var location = _locationService.GetById(id);
            if (location == null)
                return NotFound();
            return View(location);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Location location)
        {
            if (ModelState.IsValid)
            {
                _locationService.Insert(location);
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        public IActionResult Edit(int id)
        {
            var location = _locationService.GetById(id);
            if (location == null)
                return NotFound();
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Location location)
        {
            if (id != location.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _locationService.Update(location);
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        public IActionResult Delete(int id)
        {
            var location = _locationService.GetById(id);
            if (location == null)
                return NotFound();
            return View(location);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _locationService.DeleteById(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
