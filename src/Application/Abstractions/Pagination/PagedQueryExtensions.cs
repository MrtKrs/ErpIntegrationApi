namespace Application.Abstractions.Pagination;

public static class PagedQueryExtensions
{
    public static int GetOffSet(this IPagedQuery query) =>
        (query.Page -1) * query.PageSize;

    public static string? ToSqlLikePattern(this IPagedQuery query)
    {
        if(string.IsNullOrWhiteSpace(query.Search))
        {
            return null;
        }

        string escaped = query.Search.Trim()
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");

        return $"%{escaped}%";
    }
}
