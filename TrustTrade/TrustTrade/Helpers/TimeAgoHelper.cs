namespace TrustTrade.Helpers;

public static class TimeAgoHelper
{
    public static string GetTimeAgo(DateTime? pastDate, DateTime? currentDate = null)
    {
        DateTime now = currentDate ?? DateTime.UtcNow; // If currentDate is not provided, use DateTime.Now
        if (pastDate == null)
        {
            return "Date not available";
        }

        TimeSpan timeSince = now - pastDate.Value;

        if (timeSince.TotalMinutes < 1)
        {
            return "Just now";
        }
        if (timeSince.TotalMinutes < 60)
        {
            return $"{timeSince.Minutes} minutes ago";
        }
        if (timeSince.TotalHours < 24)
        {
            return $"{timeSince.Hours} hours ago";
        }
        if (timeSince.TotalDays < 30)
        {
            return $"{timeSince.Days} days ago";
        }
        if (timeSince.TotalDays < 365)
        {
            return $"{timeSince.Days / 30} months ago";
        }
        return $"{timeSince.Days / 365} years ago";
    }
}