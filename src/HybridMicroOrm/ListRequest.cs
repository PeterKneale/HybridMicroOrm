namespace HybridMicroOrm;

public class ListRequest(string type)
{
    public string Type { get; } = type;
    public Filter? Filter { get; init; } = null; 
    public bool IncludeDeleted { get; init; } = false;
    public SortBy SortBy { get; init; } = SortBy.Created;
    public SortOrder SortOrder { get; init; } = SortOrder.Ascending;
}

public record Filter(string Query, object Parameters);

public enum SortBy
{
    Created,
    Updated,
    Deleted
}

public static class SortByExtensions
{
    public static string ToColumnName(this SortBy sortBy)
    {
        return sortBy switch
        {
            SortBy.Created => "created_at",
            SortBy.Updated => "updated_at",
            SortBy.Deleted => "deleted_at",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
}

public enum SortOrder
{
    Ascending,
    Descending
}

public static class SortOrderExtensions
{
    public static string ToSqlOrder(this SortOrder sortOrder)
    {
        return sortOrder switch
        {
            SortOrder.Ascending => "ASC",
            SortOrder.Descending => "DESC",
            _ => throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, null)
        };
    }
}