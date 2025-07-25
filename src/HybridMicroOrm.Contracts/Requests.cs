namespace HybridMicroOrm.Contracts;

public class GetRequest
{
    public GetRequest(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
    public string? Type { get; init; }
    public bool IncludeDeleted { get; init; }
}

public class UpdateRequest
{
    private UpdateRequest(Guid id, string type, object data)
    {
        Id = id;
        Type = type;
        Data = data;
    }

    public Guid Id { get; }
    public string Type { get; }
    public object Data { get; }

    public static UpdateRequest Create(Guid id, string type, object data)
    {
        return new UpdateRequest(id, type, data);
    }
}

public record InsertRequest
{
    private InsertRequest(Guid id, string type, object data, bool isTenantData)
    {
        Id = id;
        Type = type;
        Data = data;
        IsTenantData = isTenantData;
    }

    public Guid Id { get; }
    public string Type { get; }
    public object Data { get; }
    public bool IsTenantData { get; }

    public static InsertRequest Create(Guid id, string type, object data, bool isTenantData = true)
    {
        return new InsertRequest(id, type, data, isTenantData);
    }
}

public class ListRequest(string type)
{
    public string Type { get; } = type;
    public Filter? Filter { get; init; } = null;
    public bool IncludeDeleted { get; init; } = false;
    public SortBy SortBy { get; init; } = SortBy.Created;
    public SortOrder SortOrder { get; init; } = SortOrder.Ascending;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
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