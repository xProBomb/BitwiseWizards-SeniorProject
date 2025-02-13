using TrustTrade.Helpers;

namespace TestTrustTrade;

[TestFixture]
public class TimeAgoHelperTests
{
    private readonly DateTime _pastDateTime = new DateTime(2025, 1, 1, 0, 0, 0);

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithOneSecondDifference_ReturnsJustNow()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddSeconds(1);
        var expected = "Just now";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithFiveMinutesDifference_ReturnsFiveMinutesAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddMinutes(5);
        var expected = "5 minutes ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithFiveHoursDifference_ReturnsFiveHoursAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddHours(5);
        var expected = "5 hours ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithFiveDaysDifference_ReturnsFiveDaysAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddDays(5);
        var expected = "5 days ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithFiveMonthsDifference_ReturnsFiveMonthsAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddMonths(5);
        var expected = "5 months ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTimeAgo_WhenCalledWithFiveYearsDifference_ReturnsFiveYearsAgo()
    {
        // Arrange
        var currentDateTime = _pastDateTime.AddYears(5);
        var expected = "5 years ago";

        // Act
        var result = TimeAgoHelper.GetTimeAgo(_pastDateTime, currentDateTime);

        // Assert
        Assert.AreEqual(expected, result);
    }
}