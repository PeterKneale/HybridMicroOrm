namespace HybridMicroOrm.Contracts;

public class PagedResponse<T>
{
    public PagedResponse(IEnumerable<Record<T>> records, int totalCount, int pageNumber, int pageSize)
    {
        Records = records;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public IEnumerable<Record<T>> Records { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public bool IsFirstPage => PageNumber == 1;
    public bool IsLastPage => PageNumber >= TotalPages;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public int StartIndex => (PageNumber - 1) * PageSize + 1;
    public int EndIndex => Math.Min(StartIndex + PageSize - 1, TotalCount);
}