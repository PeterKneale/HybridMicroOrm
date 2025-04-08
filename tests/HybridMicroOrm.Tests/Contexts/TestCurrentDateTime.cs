namespace HybridMicroOrm.Tests.Contexts;

public class TestCurrentDateTime : ICurrentDateTime
{
    private DateTime? _utcNow;

    public DateTime UtcNow
    {
        get => _utcNow ?? DateTime.UtcNow;
        set => _utcNow = value;
    }

    public void Reset() => _utcNow = null;
}