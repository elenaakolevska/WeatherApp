using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;


namespace WeatherApp.Web
{
    
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Тука ставаш код за испраќање email, на пример преку SMTP, SendGrid итн.
            // За сега, може да остане празно или да логираш.

            return Task.CompletedTask;
        }
    }

}
