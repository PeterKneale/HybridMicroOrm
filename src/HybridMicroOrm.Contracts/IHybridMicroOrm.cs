namespace HybridMicroOrm.Contracts;

public interface IHybridMicroOrm
{
    Task<Record<T>?> Get<T>(Guid id, CancellationToken cancellationToken = default);
    Task<Record<T>?> Get<T>(string id, CancellationToken cancellationToken = default);
    Task<Record<T>?> Get<T>(GetRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<Record<T>>> List<T>(ListRequest request, CancellationToken cancellationToken = default);
    Task Insert(InsertRequest request, CancellationToken cancellationToken = default);
    Task Update(UpdateRequest request, CancellationToken cancellationToken = default);
    Task Delete(Guid id, CancellationToken cancellationToken = default);
    Task SoftDelete(Guid id, CancellationToken cancellationToken = default);
}
