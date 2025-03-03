using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrustTrade.Services; 
using TrustTrade.ViewModels; 
using Microsoft.AspNetCore.Identity.UI.Services;


[Authorize]
public class SupportController : Controller
{
    private readonly IEmailSender _emailSender;
    private readonly UserManager<IdentityUser> _userManager;

    public SupportController(IEmailSender emailSender, UserManager<IdentityUser> userManager)
    {
        _emailSender = emailSender;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult ContactSupport()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ContactSupport(ContactSupportViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Get the logged in user's email from Identity
        var user = await _userManager.GetUserAsync(User);
        var userEmail = user?.Email ?? "unknown@domain.com";

        // Compose the email subject and body
        var subject = $"Support Request - {model.Tag}";
        var body = $"Date: {DateTime.UtcNow}\n" +
                   $"User Email: {userEmail}\n" +
                   $"Name: {model.Name}\n" +
                   $"Tag: {model.Tag}\n\n" +
                   $"Message:\n{model.Message}";

        // Send the email to correct email address
        await _emailSender.SendEmailAsync("trusttrade.auth@gmail.com", subject, body);

        TempData["SuccessMessage"] = "Your support request has been sent successfully.";
        return RedirectToAction("ContactSupport");
    }
}
