using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TrustTrade.Models;        
using TrustTrade.ViewModels;      

namespace TrustTrade.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly TrustTradeDbContext _context;

        public ProfileController(TrustTradeDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MyProfile()
        {
            // Get the logged-in user's IdentityId from claims (the ASP.NET Core Identity user id)
            var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityId))
            {
                return Unauthorized();
            }

            // Retrieve the user from TrustTradeDbContext including follower/following navigation properties
            var user = await _context.Users
                .Include(u => u.FollowerFollowerUsers)
                .Include(u => u.FollowerFollowingUsers)
                .FirstOrDefaultAsync(u => u.IdentityId == identityId);

            if (user == null)
            {
                return NotFound();
            }

            // Map the user data to the view model
            var model = new MyProfileViewModel
            {
                IdentityId = user.IdentityId,
                ProfileName = user.ProfileName,
                CreatedAt = user.CreatedAt,
                Bio = user.Bio,
                IsVerified = user.IsVerified ?? false,
                PlaidEnabled = user.PlaidEnabled ?? false,
                LastPlaidSync = user.LastPlaidSync,
                FollowersCount = user.FollowerFollowerUsers?.Count ?? 0,
                FollowingCount = user.FollowerFollowingUsers?.Count ?? 0
                // Posts: To be implemented later.
            };

            return View(model);
        }
    }
}
