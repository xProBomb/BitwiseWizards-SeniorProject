using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

/// <summary>
/// Repository interface for SiteSettings entities.
/// </summary>
public interface ISiteSettingsRepository : IRepository<SiteSettings>
{
    /// <summary>
    /// Gets the site settings.
    /// </summary>
    /// <returns>The site settings.</returns>
    Task<SiteSettings> GetSiteSettingsAsync();
}