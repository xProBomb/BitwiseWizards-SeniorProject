using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract
{
    public interface ILikeRepository : IRepository<Like>
    {
        /// <summary>
        /// Get a like by user ID and post ID
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="postId">ID of the post</param>
        /// <returns>The Like object if found, otherwise null</returns>
        Task<Like?> GetLikeByUserAndPostAsync(int userId, int postId);
    }
}
