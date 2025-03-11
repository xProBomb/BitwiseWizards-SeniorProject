using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace TrustTrade.Controllers
{
    public class PostsController : Controller
    {
        private readonly ILogger<PostsController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPostRepository _postRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;

        public PostsController(
            ILogger<PostsController> logger, 
            UserManager<IdentityUser> userManager, 
            IPostRepository postRepository, 
            ITagRepository tagRepository, 
            IUserRepository userRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _postRepository = postRepository;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            // Retrieve all existing tags for the view model
            var vm = new CreatePostVM
            {
                ExistingTags = _tagRepository.GetAllTagNames()
            };

            return View(vm);
        }

        [Authorize]        
        [HttpPost]
        public async Task<IActionResult> Create(CreatePostVM createPostVM)
        {
            if (ModelState.IsValid)
            {
                string? identityUserId = _userManager.GetUserId(User);
                if (identityUserId == null)
                {
                    return Unauthorized();
                }

                // Retrieve the user from the database
                User? user = await _userRepository.FindByIdentityIdAsync(identityUserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Map the CreatePostVM to the Post entity
                var post = new Post
                {
                    UserId = user.Id,
                    Title = createPostVM.Title,
                    Content = createPostVM.Content
                };

                // Add the selected tags to the post
                foreach (string tagName in createPostVM.SelectedTags)
                {
                    Tag? tag = _tagRepository.FindByTagName(tagName);
                    if (tag != null)
                    {
                        // Add the tag to the post and the post to the tag
                        post.Tags.Add(tag);
                        tag.Posts.Add(post);
                    }
                }

                // Save the post to the database
                _postRepository.AddOrUpdate(post);
                return RedirectToAction("Index", "Home");
            }

            // Repopulate the existing tags in case of validation errors
            createPostVM.ExistingTags = _tagRepository.GetAllTagNames();
            return View(createPostVM);
        }
    }
}
