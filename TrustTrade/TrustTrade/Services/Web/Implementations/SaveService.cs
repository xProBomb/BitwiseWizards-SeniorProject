using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.ViewModels;
using TrustTrade.Helpers;

namespace TrustTrade.Services.Web.Implementations;

/// <summary>
/// Service for saving related operations.
/// </summary>
public class SaveService : ISaveService
{
    private readonly ILogger<SaveService> _logger;
    private readonly IPostRepository _postRepository;
    private readonly ISavedPostRepository _savedPostRepository;

    public SaveService(ILogger<SaveService> logger, IPostRepository postRepository, ISavedPostRepository savedPostRepository)
    {
        _logger = logger;
        _postRepository = postRepository;
        _savedPostRepository = savedPostRepository;
    }

    public async Task AddSavedPostAsync(int postId, int userId)
    {
        Post? post = await _postRepository.FindByIdAsync(postId);
        if (post == null)
        {
            _logger.LogWarning("Post with ID {postId} not found.", postId);
            throw new KeyNotFoundException($"Post with ID {postId} not found.");
        }

        bool isPostSaved = await _savedPostRepository.IsPostSavedByUserAsync(postId, userId);
        if (isPostSaved)
        {
            _logger.LogInformation("Post With ID {postId} is already saved by user with ID {userId}.", postId, userId);
            throw new InvalidOperationException($"Post with ID {postId} is already saved by user with ID {userId}.");
        }

        var savedPost = new SavedPost
        {
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _savedPostRepository.AddOrUpdateAsync(savedPost);
        _logger.LogInformation("Post with ID {postId} saved by user with ID {userId}.", postId, userId);
    }

    public async Task RemoveSavedPostAsync(int postId, int userId)
    {
        SavedPost? savedPost = await _savedPostRepository.FindByPostIdAndUserIdAsync(postId, userId);
        if (savedPost == null)
        {
            _logger.LogWarning("Saved post with PostId {postId} and UserId {userId} not found.", postId, userId);
            throw new KeyNotFoundException($"Saved post with PostId {postId} and UserId {userId} not found.");
        }

        await _savedPostRepository.DeleteAsync(savedPost);
        _logger.LogInformation("Post with ID {postId} removed from saved posts for user with ID {userId}.", postId, userId);
    }
}
