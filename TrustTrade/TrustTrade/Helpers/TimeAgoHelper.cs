namespace TrustTrade.Helpers;

public static class TimeAgoHelper
{
    public static string GetTimeAgo(DateTime pastDate, DateTime? currentDate = null)
    {
        // If currentDate is not provided, use DateTime.Now
        DateTime now = currentDate ?? DateTime.UtcNow;
        TimeSpan timeSince = now - pastDate;

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