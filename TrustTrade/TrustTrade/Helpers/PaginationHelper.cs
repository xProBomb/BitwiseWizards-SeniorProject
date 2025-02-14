namespace TrustTrade.Helpers;

public static class PaginationHelper
{
    public static List<int> GetPagination(int currentPage, int totalPages, int maxPages = 5)
    {
        List<int> pages = new List<int>();
        
        if (totalPages <= maxPages)
        {
            for (int i = 1; i <= totalPages; i++)
            {
                pages.Add(i);
            }
        }
        else
        {
            int halfRange = maxPages / 2;
            int startPage, endPage;

            if (currentPage - halfRange <= 1)
            {
                startPage = 1;
                endPage = maxPages;
            }
            else if (currentPage + halfRange >= totalPages)
            {
                startPage = totalPages - maxPages + 1;
                endPage = totalPages;
            }
            else
            {
                startPage = currentPage - halfRange;
                endPage = currentPage + halfRange;

                // Adjust if maxPages is even to maintain symmetry
                if (maxPages % 2 == 0)
                {
                    endPage--;
                }
            }

            for (int i = startPage; i <= endPage; i++)
            {
                pages.Add(i);
            }
        }
        return pages;
    }
}
