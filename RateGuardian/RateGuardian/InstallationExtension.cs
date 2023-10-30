using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RateGuardian.Middleware;
using RateGuardian.Storage;

namespace RateGuardian;

public static class InstallationExtension
{
    public static IServiceCollection AddRateGuardian(this IServiceCollection services)
    {
        services
            .AddSingleton<IInMemStorage, InMemStorage>()
            .AddTransient<IpWhitelistMiddleware>();
        
        return services;
    }
    
    public static IApplicationBuilder UseRateGuardian(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IpWhitelistMiddleware>();
    }
}