using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.Services.Web.Interfaces;

namespace TrustTrade.Controllers.Api
{
    [Route("api/saves")]
    [ApiController]
    public class SavesApiController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly ILogger<SavesApiController> _logger;

        public SavesApiController(
            IPostService postService,
            IUserService userService,
            ILogger<SavesApiController> logger)
        {
            _postService = postService;
            _userService = userService;
            _logger = logger;
        }

        // POST: api/saves/posts/{postId}
        [HttpPost("posts/{postId}")]
        [Authorize]
        public async Task<IActionResult> AddPostSave(int postId)
        {
            try
            {
                User? currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null) return Unauthorized();

                Console.WriteLine("userId: " + currentUser.Id);
                Console.WriteLine("postId: " + postId);
                Console.WriteLine("We are in the AddPostSave method.!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1");

                await _postService.AddPostToSavedPostsAsync(postId, currentUser.Id);
                return Ok(new { message = "Post saved successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving post.");
                return StatusCode(500, "An error occurred while saving the post.");
            }
        }

        // DELETE: api/saves/posts/{postId}
        [HttpDelete("posts/{postId}")]
        [Authorize]
        public async Task<IActionResult> RemovePostSave(int postId)
        {
            try
            {
                User? currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null) return Unauthorized();

                await _postService.RemovePostFromSavedPostsAsync(postId, currentUser.Id);
                return Ok(new { message = "Post removed from saved posts." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing post from saved posts.");
                return StatusCode(500, "An error occurred while removing the post from saved posts.");
            }
        }
    }
}