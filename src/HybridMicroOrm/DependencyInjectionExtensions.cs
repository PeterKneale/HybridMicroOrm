using Microsoft.Extensions.DependencyInjection;

namespace HybridMicroOrm;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddHybridMicroOrm(this IServiceCollection services, Action<HybridMicroOrmOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddScoped<ICurrentDateTime, DefaultCurrentDateTime>();
        services.AddScoped<IHybridMicroOrm, HybridMicroOrm>();
        services.AddScoped<IHybridMicroOrmManager, HybridMicroOrmManager>();
        return services;
    }
}