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

    public async Task<List<string>> GetAllTagNamesAsync()
    {
        return await _tags
            .Select(t => t.TagName)
            .ToListAsync();
    }

    public async Task<Tag?> FindByTagNameAsync(string tagName)
    {
        return await _tags
            .FirstOrDefaultAsync(t => t.TagName.ToLower() == tagName.ToLower());
    }
}
