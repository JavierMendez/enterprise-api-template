using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace EnterpriseApi.Features.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddFeatureChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("LiveCheck", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("La aplicación se está ejecutando."), tags: new[] { "live" })
            .AddCheck<ReadinessDependencyCheck>("ReadyCheck", tags: new [] { "ready" });

        return services;
    }

    public static IEndpointRouteBuilder MapFeatureHealthCheckEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/live", new HealthCheckOptions
        {
            Predicate = (check) => check.Tags.Contains("live")
        });

        endpoints.MapHealthChecks("/ready", new HealthCheckOptions
        {
            Predicate = (check) => check.Tags.Contains("ready")
        });

        return endpoints;
    }
}