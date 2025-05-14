namespace TrustTrade.Services.Web.Interfaces;

/// <summary>
/// Interface for saving related services.
/// </summary>
public interface ISaveService
{
    Task AddSavedPostAsync(int postId, int userId);

    Task RemoveSavedPostAsync(int postId, int userId);
}