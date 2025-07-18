namespace HybridMicroOrm.Contracts;

public interface IHybridMicroOrm
{
    Task<Record<T>?> Get<T>(Guid id);
    Task<Record<T>?> Get<T>(string id);
    Task<Record<T>?> Get<T>(GetRequest request);
    Task<IEnumerable<Record<T>>> List<T>(ListRequest request);
    Task Insert(InsertRequest request);
    Task Update(UpdateRequest request);
    Task Delete(Guid id);
    Task SoftDelete(Guid id);
}
