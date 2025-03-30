namespace HybridMicroOrm.Contracts;

public interface IHybridMicroOrm
{
    Task<Record?> Get(Guid id, bool includeDeleted = false);
    Task<IEnumerable<Record>> List(ListRequest request);
    Task Insert(InsertRequest request);
    Task Update(UpdateRequest request);
    Task Delete(Guid id);
    Task SoftDelete(Guid id);
}