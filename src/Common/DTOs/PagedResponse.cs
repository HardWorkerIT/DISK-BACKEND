namespace Common.DTOs;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PerInPage { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PerInPage);

    public PagedResponse(List<T> items, int totalCount, int page, int perInPage)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PerInPage = perInPage;
    }

    public PagedResponse(IEnumerable<T> items, int totalCount, int page, int perInPage)
    {
        Items = items.ToList();
        TotalCount = totalCount;
        Page = page;
        PerInPage = perInPage;
    }
}

