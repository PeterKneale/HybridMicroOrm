namespace HybridMicroOrm.Tests.TestData;

public class Car
{
    public Guid Id { get; set; }
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public static string Type => nameof(Car);
}