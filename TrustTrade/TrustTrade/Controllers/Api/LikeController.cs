using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.Services.Web.Interfaces;
using System.Threading.Tasks;
using TrustTrade.Services;

namespace TrustTrade.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LikeController : ControllerBase
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;
        private readonly ILogger<LikeController> _logger;
        private readonly INotificationService _notificationService;
        
        // rate limit like spammers
        private static readonly Dictionary<string, DateTime> _lastActionTimes = new();
        private static readonly TimeSpan _actionCooldown = TimeSpan.FromSeconds(5);

        public LikeController(
            ILikeRepository likeRepository,
            IPostRepository postRepository,
            IUserService userService,
            ILogger<LikeController> logger,
            INotificationService notificationService)
        {
            _likeRepository = likeRepository;
            _postRepository = postRepository;
            _userService = userService;
            _logger = logger;
            _notificationService = notificationService;
        }

        // POST: api/Like/toggle/5
        [HttpPost("toggle/{postId}")]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            // Get current user
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            
            // Apply rate limiting
            string actionKey = $"{user.Id}:{postId}";
            if (_lastActionTimes.TryGetValue(actionKey, out DateTime lastActionTime))
            {
                if (DateTime.UtcNow - lastActionTime < _actionCooldown)
                {
                    return StatusCode(429, new { error = "Too many requests. Please wait before trying again." });
                }
            }
            
            // Update the last action time
            _lastActionTimes[actionKey] = DateTime.UtcNow;

            // Check if post exists
            var post = await _postRepository.FindByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            // Check if user already liked the post
            var existingLike = await _likeRepository.GetLikeByUserAndPostAsync(user.Id, postId);

            bool isLiked;
            int likeCount;

            if (existingLike != null)
            {
                // Unlike: Remove existing like
                await _likeRepository.DeleteAsync(existingLike);
                isLiked = false;
            }
            else
            {
                // Like: Add new like
                var newLike = new Like
                {
                    UserId = user.Id,
                    PostId = postId,
                    CreatedAt = DateTime.UtcNow
                };
                await _likeRepository.AddOrUpdateAsync(newLike);
                isLiked = true;
                
                // Generate notification when a post is liked
                if (post.UserId != user.Id)  // Don't notify for self-likes
                {
                    await _notificationService.CreateLikeNotificationAsync(user.Id, postId, post.UserId);
                }
            }

            // Get updated like count
            post = await _postRepository.FindByIdAsync(postId);
            likeCount = post.Likes.Count;

            return Ok(new { isLiked, likeCount });
        }

        // GET: api/Like/status/5
        [HttpGet("status/{postId}")]
        public async Task<IActionResult> GetLikeStatus(int postId)
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var post = await _postRepository.FindByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            var existingLike = await _likeRepository.GetLikeByUserAndPostAsync(user.Id, postId);
            bool isLiked = existingLike != null;
            int likeCount = post.Likes.Count;

            return Ok(new { isLiked, likeCount });
        }
    }
}