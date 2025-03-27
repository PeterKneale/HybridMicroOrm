namespace HybridMicroOrm;

public interface IHybridMicroOrmManager
{
    Task Init();
    Task Destroy();
}