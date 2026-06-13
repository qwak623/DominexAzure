using Dominex.Contracts.Infrastructure;
using Dominex.Primitives.Security;
using Havit.Extensions.DependencyInjection.Abstractions;
using Dominex.Model.Security;
using Havit.Services.Caching;
using Microsoft.AspNetCore.Authorization;

namespace Dominex.Facades.Infrastructure;

[Service]
//[Authorize(Roles = nameof(RoleEntry.SystemAdministrator))]
public class MaintenanceFacade : IMaintenanceFacade
{
	private readonly ICacheService _cacheService;

	public MaintenanceFacade(ICacheService cacheService)
	{
		_cacheService = cacheService;
	}

	public Task ClearCacheAsync(CancellationToken cancellationToken = default)
	{
		_cacheService.Clear();

		return Task.CompletedTask;
	}
}