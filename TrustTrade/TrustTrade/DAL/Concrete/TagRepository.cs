using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete;

public class TagRepository : Repository<Tag>, ITagRepository
{
    private DbSet<Tag> _tags;

    public TagRepository(TrustTradeDbContext context) : base(context)
    {
        _tags = context.Tags;
    }

    public List<string> GetAllTagNames()
    {
        return _tags
            .Select(t => t.TagName)
            .ToList();
    }

    public Tag? GetTagByName(string tagName)
    {
        return _tags
            .FirstOrDefault(t => t.TagName.ToLower() == tagName.ToLower());
    }
}
