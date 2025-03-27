namespace HybridMicroOrm.Contexts;

public interface ICurrentDateTime
{
    DateTime UtcNow { get; }
}