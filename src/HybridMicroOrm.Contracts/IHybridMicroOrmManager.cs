namespace HybridMicroOrm.Contracts;

public interface IHybridMicroOrmManager
{
    Task Init();
    Task Drop();
}