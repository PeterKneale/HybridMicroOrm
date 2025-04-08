namespace HybridMicroOrm;

internal class CurrentDateTime : ICurrentDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}