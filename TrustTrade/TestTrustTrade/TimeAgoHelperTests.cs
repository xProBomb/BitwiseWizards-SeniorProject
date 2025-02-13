using TrustTrade.Helpers;

namespace TestTrustTrade;

[TestFixture]
public class TimeAgoHelperTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithOneSecondDifference_ReturnsJustNow()
    {
        // Arrange
        var pastDateTime = new DateTime(2025, 1, 1, 0, 0, 0);
        var currentDateTime = pastDateTime.AddSeconds(1);
        var expected = "Just now";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithFiveMinutesDifference_ReturnsFiveMinutesAgo()
    {
        // Arrange
        var pastDateTime = new DateTime(2025, 1, 1, 0, 0, 0);
        var currentDateTime = pastDateTime.AddMinutes(5);
        var expected = "5 minutes ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithFiveHoursDifference_ReturnsFiveHoursAgo()
    {
        // Arrange
        var pastDateTime = new DateTime(2025, 1, 1, 0, 0, 0);
        var currentDateTime = pastDateTime.AddHours(5);
        var expected = "5 hours ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithFiveDaysDifference_ReturnsFiveDaysAgo()
    {
        // Arrange
        var pastDateTime = new DateTime(2025, 1, 1, 0, 0, 0);
        var currentDateTime = pastDateTime.AddDays(5);
        var expected = "5 days ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithFiveMonthsDifference_ReturnsFiveMonthsAgo()
    {
        // Arrange
        var pastDateTime = new DateTime(2025, 1, 1, 0, 0, 0);
        var currentDateTime = pastDateTime.AddMonths(5);
        var expected = "5 months ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithFiveYearsDifference_ReturnsFiveYearsAgo()
    {
        // Arrange
        var pastDateTime = new DateTime(2025, 1, 1, 0, 0, 0);
        var currentDateTime = pastDateTime.AddYears(5);
        var expected = "5 years ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }
}