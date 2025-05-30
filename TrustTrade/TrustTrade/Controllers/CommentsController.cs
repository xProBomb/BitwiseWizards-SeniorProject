using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.ViewModels;
using Microsoft.AspNetCore.Authorization;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.Models.ExtensionMethods;

namespace TrustTrade.Controllers
{
    [Route("comments")]
    public class CommentsController : Controller
    {
        private readonly ILogger<CommentsController> _logger;
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;

        public CommentsController(
            ILogger<CommentsController> logger, 
            ICommentService commentService,
            IUserService userService)
        {
            _logger = logger;
            _commentService = commentService;
            _userService = userService;
        }

        [HttpGet("rendercommentpartial/{commentId}")]
        public async Task<IActionResult> RenderCommentPartial(int commentId)
        {
            Comment? comment = await _commentService.GetCommentByIdAsync(commentId);
            if (comment == null) return NotFound();

            User? currentUser = await _userService.GetCurrentUserAsync(User);

            var vm = comment.ToViewModel(currentUser);
            return PartialView("_CommentPartial", vm);
        }
    }
}