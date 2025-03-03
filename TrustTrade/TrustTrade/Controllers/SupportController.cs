using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrustTrade.Services; 
using TrustTrade.ViewModels; 
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.WebSockets;


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
        var body = $@"
        <html>
        <head>
        <style>
            body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            }}
            .header {{
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 15px;
            }}
            .detail {{
            margin: 5px 0;
            }}
            .message {{
            margin-top: 20px;
            padding: 10px;
            background-color: #f9f9f9;
            border: 1px solid #ddd;
            }}
        </style>
        </head>
        <body>
        <div class='header'>Support Request</div>
        <div class='detail'><strong>Date:</strong> {DateTime.UtcNow}</div>
        <div class='detail'><strong>User Email:</strong> {userEmail}</div>
        <div class='detail'><strong>Name:</strong> {model.Name}</div>
        <div class='detail'><strong>Tag:</strong> {model.Tag}</div>
        <hr>
        <div class='message'>
            <strong>Message:</strong>
            <p>{model.Message}</p>
        </div>
        </body>
        </html>
        ";


        // Send the email to correct email address
        await _emailSender.SendEmailAsync("trusttrade.auth@gmail.com", subject, body);

        TempData["SuccessMessage"] = "Your support request has been sent successfully.";
        return RedirectToAction("ContactSupport");
    }
}
