using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dominex.Services.HealthChecks;

public abstract class BaseHealthCheck : IHealthCheck
{
	async Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
	{
		try
		{
			return await CheckHealthAsync(cancellationToken);
		}
		catch (Exception exception)
		{
			return HealthCheckResult.Unhealthy(exception: exception);
		}
	}

	protected abstract Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken);
}