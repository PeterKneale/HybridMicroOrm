namespace HybridMicroOrm.Contracts;

public interface IHybridMicroOrmManager
{
    Task Init(CancellationToken cancellationToken = default);
    Task Drop(CancellationToken cancellationToken = default);
}