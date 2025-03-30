namespace HybridMicroOrm.Contracts;

public interface ICurrentDateTime
{
    DateTime UtcNow { get; }
}