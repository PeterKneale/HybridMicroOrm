using HybridMicroOrm.Contexts;

namespace HybridMicroOrm.Internals;

internal class DefaultCurrentDateTime : ICurrentDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}