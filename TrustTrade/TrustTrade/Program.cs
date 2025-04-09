using Going.Plaid;
using Microsoft.EntityFrameworkCore;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.DAL.Concrete;
using TrustTrade.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using TrustTrade.Services;


var builder = WebApplication.CreateBuilder(args);

// Database Contexts
builder.Services.AddDbContext<TrustTradeDbContext>(options => options
    .UseLazyLoadingProxies()
    .UseSqlServer(builder.Configuration.GetConnectionString("TrustTradeConnection"), sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }));

// Repositories
builder.Services.AddScoped<DbContext, TrustTradeDbContext>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IHoldingsRepository, HoldingsRepository>();
builder.Services.AddScoped<ISearchUserRepository, SearchUserRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var identityConnectionString = builder.Configuration.GetConnectionString("IdentityConnection") 
    ?? throw new InvalidOperationException("Connection string 'IdentityConnection' not found.");
builder.Services.AddDbContext<AuthenticationDbContext>(options =>
    options.UseSqlServer(identityConnectionString));

// Development Tools
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity and Authentication Configuration
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Set to true in production
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
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

// Add services to the container.
builder.Services.AddScoped<IProfileService, ProfileService>();

// Add services for performance scoring
builder.Services.AddScoped<IPerformanceScoreRepository, PerformanceScoreRepository>();

// Add services for verification history
builder.Services.AddScoped<IVerificationHistoryRepository, VerificationHistoryRepository>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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