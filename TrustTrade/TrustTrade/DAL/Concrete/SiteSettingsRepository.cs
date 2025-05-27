using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete;

/// <summary>
/// Repository for SiteSettings entities.
/// </summary>
public class SiteSettingsRepository : Repository<SiteSettings>, ISiteSettingsRepository
{
    private DbSet<SiteSettings> _siteSettings;

    public SiteSettingsRepository(TrustTradeDbContext context) : base(context)
    {
        _siteSettings = context.SiteSettings;
    }

    public async Task<SiteSettings> GetSiteSettingsAsync()
    {
        // Site settings are currently stored in one row. This can be changed later if needed.
        var siteSettings = await _siteSettings
            .FirstOrDefaultAsync();

        if (siteSettings == null)
        {
            // If no settings exist, create a default one
            siteSettings = new SiteSettings
            {
                IsPresentationModeEnabled = false
            };
            await AddOrUpdateAsync(siteSettings);
        }

        return siteSettings;
    }

}
