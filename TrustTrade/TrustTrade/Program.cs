using Going.Plaid;
using Microsoft.EntityFrameworkCore;
using TrustTrade.Models;
using TrustTrade.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using TrustTrade.Services;
using TrustTrade.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Database Contexts
var trustTradeConnectionString = builder.Configuration.GetConnectionString("TrustTradeConnection");
builder.Services.AddDbContext<TrustTradeDbContext>(options => options
    .UseLazyLoadingProxies()
    .UseSqlServer(trustTradeConnectionString));

var identityConnectionString = builder.Configuration.GetConnectionString("IdentityConnection") 
    ?? throw new InvalidOperationException("Connection string 'IdentityConnection' not found.");
builder.Services.AddDbContext<AuthenticationDbContext>(options =>
    options.UseSqlServer(identityConnectionString));

// Development Tools
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity and Authentication Configuration
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = true; // Set to true in production
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AuthenticationDbContext>();

// Cookie Configuration
builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.Cookie.Name = "TrustTrade.Auth";
});

// Add Plaid services
builder.Services.AddPlaid(builder.Configuration.GetSection("Plaid"));

// MVC Configuration
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Email Sender Configuration
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);


var app = builder.Build();

// Development vs Production Configuration
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/Home/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication Middleware - ORDER IS CRITICAL
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();