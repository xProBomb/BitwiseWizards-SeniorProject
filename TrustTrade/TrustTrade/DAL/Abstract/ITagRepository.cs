using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

public interface ITagRepository : IRepository<Tag>
{
    List<string?> GetAllTagNames();
}