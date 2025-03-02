public interface IProfileService
{
    void FollowUser(string currentUserId, string profileId);
    void UnfollowUser(string currentUserId, string profileId);
}
