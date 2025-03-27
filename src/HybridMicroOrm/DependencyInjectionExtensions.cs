using HybridMicroOrm.Contexts;
using HybridMicroOrm.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace HybridMicroOrm;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddHybridMicroOrm(this IServiceCollection services, Action<HybridMicroOrmOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddScoped<ICurrentDateTime, DefaultCurrentDateTime>();
        services.AddScoped<IHybridMicroOrm, Internals.HybridMicroOrm>();
        services.AddScoped<IHybridMicroOrmManager, HybridMicroOrmManager>();
        return services;
    }
}