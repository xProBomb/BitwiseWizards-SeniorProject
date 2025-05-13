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
        private readonly ISaveService _saveService;
        private readonly IUserService _userService;
        private readonly ILogger<SavesApiController> _logger;

        public SavesApiController(
            ISaveService saveService,
            IUserService userService,
            ILogger<SavesApiController> logger)
        {
            _saveService = saveService;
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

                await _saveService.AddSavedPostAsync(postId, currentUser.Id);
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

                await _saveService.RemoveSavedPostAsync(postId, currentUser.Id);
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