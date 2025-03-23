using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

public interface ITagRepository : IRepository<Tag>
{
    Task<List<string>> GetAllTagNamesAsync();

    Task<Tag?> FindByTagNameAsync(string tagName);
}