using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WeatherApp.Domain.IdentityModels;

public class LogoutModel : PageModel
{
    private readonly SignInManager<WeatherAppUser> _signInManager;

    public LogoutModel(SignInManager<WeatherAppUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect(returnUrl ?? Url.Content("~/"));
    }

    public IActionResult OnGet()
    {
        return RedirectToPage("./Login");
    }
}
