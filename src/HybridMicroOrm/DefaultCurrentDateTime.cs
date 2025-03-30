namespace HybridMicroOrm;

internal class DefaultCurrentDateTime : ICurrentDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}