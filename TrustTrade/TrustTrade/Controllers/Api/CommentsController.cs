using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.Models.DTO;

namespace TrustTrade.Controllers.Api
{
    [Route("api/comments")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(
            ICommentService commentService, 
            IUserService userService, 
            ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _userService = userService;
            _logger = logger;
        }

        // POST: api/comments
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateComment([FromBody] CommentCreateDTO commentCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                User? user = await _userService.GetCurrentUserAsync(User);
                if (user == null) return Unauthorized();

                Comment comment = await _commentService.CreateCommentAsync(user, commentCreateDTO);

                return Ok(new { commentId = comment.Id });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment.");
                return StatusCode(500, "An error occurred while creating the comment.");
            }
        }
    }
}