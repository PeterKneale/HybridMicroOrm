namespace HybridMicroOrm.Tests.Contexts;

public class TestUserContext : IUserContext
{
    public Guid? UserId { get; set; }
}