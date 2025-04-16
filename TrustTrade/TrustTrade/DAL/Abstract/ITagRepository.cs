using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

/// <summary>
/// Repository interface for Tag entities.
/// </summary>
public interface ITagRepository : IRepository<Tag>
{
    /// <summary>
    /// Get all tag names.
    /// </summary>
    /// <returns>A list of all tag names.</returns>
    Task<List<string>> GetAllTagNamesAsync();

    /// <summary>
    /// Find a tag by its name.
    /// </summary>
    /// <param name="tagName">The name of the tag to find.</param>
    /// <returns>The tag with the specified name, or null if not found.</returns>
    Task<Tag?> FindByTagNameAsync(string tagName);
}