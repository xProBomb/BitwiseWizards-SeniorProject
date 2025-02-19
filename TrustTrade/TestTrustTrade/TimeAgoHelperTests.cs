using TrustTrade.Helpers;

namespace TestTrustTrade;

[TestFixture]
public class TimeAgoHelperTests
{
    private readonly DateTime _pastDateTime = new(2025, 1, 1, 0, 0, 0);

    [Test]
    public void GetTimeAgo_WhenOneSecondDifference_ReturnsJustNow()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddSeconds(1);
        var expected = "Just now";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetTimeAgo_WhenOneMinuteDifference_ReturnsOneMinuteAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddMinutes(1);
        var expected = "1 minute ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetTimeAgo_WhenFiveMinutesDifference_ReturnsFiveMinutesAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddMinutes(5);
        var expected = "5 minutes ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetTimeAgo_WhenOneHourDifference_ReturnsOneHourAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddHours(1);
        var expected = "1 hour ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetTimeAgo_WhenFiveHoursDifference_ReturnsFiveHoursAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddHours(5);
        var expected = "5 hours ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetTimeAgo_WhenOneDayDifference_ReturnsOneDayAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddDays(1);
        var expected = "1 day ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetTimeAgo_WhenFiveDaysDifference_ReturnsFiveDaysAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddDays(5);
        var expected = "5 days ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetTimeAgo_WhenOneMonthDifference_ReturnsOneMonthAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddMonths(1);
        var expected = "1 month ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetTimeAgo_WhenFiveMonthsDifference_ReturnsFiveMonthsAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddMonths(5);
        var expected = "5 months ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetTimeAgo_WhenOneYearDifference_ReturnsOneYearAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddYears(1);
        var expected = "1 year ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetTimeAgo_WhenFiveYearsDifference_ReturnsFiveYearsAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddYears(5);
        var expected = "5 years ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }
}