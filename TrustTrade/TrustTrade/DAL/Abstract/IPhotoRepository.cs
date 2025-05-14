using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract
{
    public interface IPhotoRepository : IRepository<Photo>
    {
        Task<Photo?> FindByIdAsync(int id);
        Task<IEnumerable<Photo>> GetPhotosByPostIdAsync(int postId);
    }
}