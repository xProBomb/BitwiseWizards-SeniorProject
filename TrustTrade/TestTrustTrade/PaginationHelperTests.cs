using TrustTrade.Helpers;

namespace TestTrustTrade;

[TestFixture]
public class PaginationHelperTests
{
    [TestCase(1, 10, 5, new[] { 1, 2, 3, 4, 5 })]
    [TestCase(2, 10, 5, new[] { 1, 2, 3, 4, 5 })]
    [TestCase(3, 10, 5, new[] { 1, 2, 3, 4, 5 })]
    [TestCase(4, 10, 5, new[] { 2, 3, 4, 5, 6 })]
    [TestCase(5, 10, 5, new[] { 3, 4, 5, 6, 7 })]
    [TestCase(6, 10, 5, new[] { 4, 5, 6, 7, 8 })]
    [TestCase(7, 10, 5, new[] { 5, 6, 7, 8, 9 })]
    [TestCase(8, 10, 5, new[] { 6, 7, 8, 9, 10 })]
    [TestCase(9, 10, 5, new[] { 6, 7, 8, 9, 10 })]
    [TestCase(10, 10, 5, new[] { 6, 7, 8, 9, 10 })]
    public void GetPagination_WhenMaxPagesFive_ReturnsExpectedPages(int currentPage, int totalPages, int maxPages, int[] expectedArray)
    {
        // Convert array to list
        var expected = new List<int>(expectedArray);

        // Act
        var result = PaginationHelper.GetPagination(currentPage, totalPages, maxPages);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetPagination_WhenTotalPagesEqualtoMaxPages_ReturnsAllPages()
    {
        // Arrange
        int currentPage = 2;
        int totalPages = 5;
        int maxPages = 5;
        List<int> expected = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = PaginationHelper.GetPagination(currentPage, totalPages, maxPages);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetPagination_WhenTotalPagesLessThanMaxPages_ReturnsAllPages()
    {
        // Arrange
        int currentPage = 2;
        int totalPages = 3;
        int maxPages = 5;
        List<int> expected = new List<int> { 1, 2, 3 };

        // Act
        var result = PaginationHelper.GetPagination(currentPage, totalPages, maxPages);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetPagination_WhenOnlyOnePageAvailable_ReturnsOnePage()
    {
        // Arrange
        int currentPage = 1;
        int totalPages = 1;
        int maxPages = 5;
        List<int> expected = new List<int> { 1 };

        // Act
        var result = PaginationHelper.GetPagination(currentPage, totalPages, maxPages);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetPagination_WhenCurrentPageInMiddleAndMaxPagesEven_ReturnsExpectedPages()
    {
        // Arrange
        int currentPage = 6;
        int totalPages = 10;
        int maxPages = 4;
        List<int> expected = new List<int> { 4, 5, 6, 7 };

        // Act
        var result = PaginationHelper.GetPagination(currentPage, totalPages, maxPages);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetPagination_WhenLargeInputsProvided_ReturnsExpectedPages()
    {
        // Arrange
        int currentPage = 999;
        int totalPages = 1010;
        int maxPages = 5;
        List<int> expected = new List<int> { 997, 998, 999, 1000, 1001 };

        // Act
        var result = PaginationHelper.GetPagination(currentPage, totalPages, maxPages);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }
}