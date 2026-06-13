//using System.Security.Claims;
//using Dominex.Primitives.Security;
//using Havit.Extensions.DependencyInjection.Abstractions;

//namespace Dominex.Services.Infrastructure.Security;

//[Service(Profile = ServiceProfiles.WebServer)]
//public class ApplicationAuthorizationService : IApplicationAuthorizationService
//{
//	private readonly IApplicationAuthenticationService _applicationAuthenticationService;

//	public ApplicationAuthorizationService(IApplicationAuthenticationService applicationAuthenticationService)
//	{
//		_applicationAuthenticationService = applicationAuthenticationService;
//	}

//	public IEnumerable<RoleEntry> GetCurrentUserRoles()
//	{
//		return _applicationAuthenticationService.GetCurrentClaimsPrincipal().FindAll(ClaimTypes.Role).Select(c => Enum.Parse<RoleEntry>(c.Value));
//	}

//	public bool IsCurrentUserInRole(RoleEntry role)
//	{
//		return _applicationAuthenticationService.GetCurrentClaimsPrincipal().IsInRole(role.ToString());
//	}
//}
