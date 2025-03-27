namespace HybridMicroOrm.Tests.TestData;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public static string Type => nameof(User);
}