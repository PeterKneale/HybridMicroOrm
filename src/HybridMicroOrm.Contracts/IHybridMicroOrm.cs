namespace HybridMicroOrm.Contracts;

public interface IHybridMicroOrm
{
    Task<Record?> Get(Guid id);
    Task<Record?> Get(string id);
    Task<Record?> Get(GetRequest request);
    Task<T?> Get<T>(Guid id);
    Task<T?> Get<T>(string id);
    Task<T?> Get<T>(GetRequest request);
    Task<IEnumerable<Record>> List(ListRequest request);
    Task<IEnumerable<T>> List<T>(ListRequest request);
    Task Insert(InsertRequest request);
    Task Update(UpdateRequest request);
    Task Delete(Guid id);
    Task SoftDelete(Guid id);
}
