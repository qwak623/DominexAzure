using System.Linq; // added for LINQ
using System.Security;
using System.Security.Claims;
using Dominex.Contracts.Infrastructure.Security;
using Dominex.DataLayer.Repositories.Security;
using Dominex.Model.Security;
using Dominex.Primitives.Security;
using Dominex.Services.Infrastructure;
using Havit.Data.Patterns.UnitOfWorks;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Services.Caching;

namespace Dominex.Facades.Infrastructure.Security.Claims;

[Service(Profile = ServiceProfiles.WebServer)]
public class CustomClaimsBuilder : ICustomClaimsBuilder
{
	private readonly IUserRepository _userRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ICacheService _cacheService;

	public CustomClaimsBuilder(
		IUserRepository userRepository,
		IUnitOfWork unitOfWork,
		ICacheService cacheService)
	{
		_userRepository = userRepository;
		_unitOfWork = unitOfWork;
		_cacheService = cacheService;
	}

	public async Task<List<Claim>> GetCustomClaimsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
	{
			if (principal?.Identity?.IsAuthenticated != true)
		{
			throw new SecurityException("Principal is not authenticated.");
		}

		List<Claim> result = new();

		string email = principal.FindFirst(ClaimTypes.Email)?.Value;
		User user = !string.IsNullOrEmpty(email)
			? await _userRepository.GetByEmailAsync(email, cancellationToken)
			: null;

		if (user is null)
		{
			user = await OnboardUserAsync(principal, cancellationToken);
		}

		if (user.Disabled)
		{
			throw new SecurityException("User is disabled.");
		}

		if (user.Deleted is not null)
		{
			throw new SecurityException("User is deleted.");
		}

		result.Add(new Claim(ClaimConstants.UserIdClaim, user.Id.ToString(), null, ClaimConstants.ApplicationIssuer));

		var roles = user.UserRoles.Select(ur => (RoleEntry)ur.RoleId);
		foreach (var role in roles)
		{
			result.Add(new Claim(ClaimTypes.Role, role.ToString(), null, ClaimConstants.ApplicationIssuer));
		}

		return result;
	}

	private async Task<User> OnboardUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
	{
		var user = new User();

		user.IdentityProviderExternalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Google sub
		user.Email = principal.FindFirst(ClaimTypes.Email)?.Value;
		user.DisplayName = principal.FindFirst(ClaimTypes.Name)?.Value;
		user.Created = DateTime.UtcNow;

#if DEBUG
		// First user in debug gets all roles for convenience
		if (!_userRepository.GetAllIncludingDeleted().Any())
		{
			user.UserRoles.AddRange(Enum.GetValues<RoleEntry>().Select(entry => new UserRole() { RoleId = (int)entry }));
		}
#endif

		_unitOfWork.AddForInsert(user);
		await _unitOfWork.CommitAsync(cancellationToken);

		_cacheService.Clear();

		return user;
	}
}