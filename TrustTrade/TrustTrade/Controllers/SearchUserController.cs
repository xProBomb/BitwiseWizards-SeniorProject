using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TrustTrade.Data;
using TrustTrade.Models;
using System.Collections.Generic;

namespace TrustTrade.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchUserRepository _searchuserRepository;

        public SearchController(ISearchUserRepository searchuserRepository)
        {
            _searchuserRepository = searchuserRepository;
        }

        // Renders the main Search page
        [HttpGet]
        public IActionResult SearchUser()
        {
            return View();
        }

        // This action is called asynchronously to search for users in real time
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string searchTerm)
        {
            // some validation, prob want to add more?
            if (searchTerm?.Length > 50)
            {
                return BadRequest("Search term is too long.");
            }

            var users = await _searchuserRepository.SearchUsersAsync(searchTerm);

            
            return PartialView("_UserSearchResultsPartial", users);
        }

    }
}
