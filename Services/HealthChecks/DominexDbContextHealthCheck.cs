using Havit.Data.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dominex.Services.HealthChecks;

public class DominexDbContextHealthCheck : BaseHealthCheck
{
	private readonly IDbContext _dbContext;

	public DominexDbContextHealthCheck(IDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	protected async override Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken)
	{
		return await _dbContext.Database.CanConnectAsync(cancellationToken)
			? HealthCheckResult.Healthy()
			: HealthCheckResult.Unhealthy();
	}
}
