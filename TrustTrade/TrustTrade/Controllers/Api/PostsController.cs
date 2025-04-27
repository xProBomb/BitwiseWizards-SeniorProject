using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.Models.DTO;
using TrustTrade.Models.ExtensionMethods;

namespace TrustTrade.Controllers.Api
{
    [Route("api/posts")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(
            ICommentService commentService, 
            IUserService userService, 
            ILogger<PostsController> logger)
        {
            _commentService = commentService;
            _userService = userService;
            _logger = logger;
        }

        // GET: api/posts/{postId}/comments/{commentId}
        [HttpGet("{postId}/comments/{commentId}")]
        public async Task<IActionResult> GetComment(int postId, int commentId)
        {
            // TODO: Implement fetching the comment by postId and commentId if needed in the future
            return Ok(); // Temporary response so the API doesn't break
        }

        // POST: api/posts/{postId}/comments
        [HttpPost("{postId}/comments")]
        [Authorize]
        public async Task<IActionResult> CreateComment(int postId, [FromBody] CommentCreateDTO commentCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                User? user = await _userService.GetCurrentUserAsync(User);
                if (user == null) return Unauthorized();

                Comment comment = await _commentService.CreateCommentAsync(postId, user, commentCreateDTO);

                var responseDTO = comment.ToResponseDTO();

                return CreatedAtAction(nameof(GetComment), new { postId, commentId = comment.Id }, responseDTO);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for post {PostId}", postId);
                return StatusCode(500, "An error occurred while creating the comment.");
            }
        }
    }
}