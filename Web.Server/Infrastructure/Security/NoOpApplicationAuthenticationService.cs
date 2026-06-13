//using System.Security.Claims;
//using Dominex.Model.Security;
//using Dominex.Services.Infrastructure.Security;
//using System.Text.Encodings.Web;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.Extensions.Options;

//namespace Dominex.Web.Server.Infrastructure.Security;

//public class NoOpApplicationAuthenticationService : IApplicationAuthenticationService
//{
//	// anonymous principal so calls to authentication APIs don't NRE
//	private static readonly ClaimsPrincipal AnonymousPrincipal = new(new ClaimsIdentity());

//	public NoOpApplicationAuthenticationService()
//	{
//	}

//	public ClaimsPrincipal GetCurrentClaimsPrincipal() => AnonymousPrincipal;

//	// When auth is disabled we cannot resolve a real User; return null to indicate absence.
//	public User GetCurrentUser() => null;

//	// Return 0 as a safe default when no authenticated user exists.
//	public int GetCurrentUserId() => 0;

//}

//public class NoOpAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
//{
//	public const string SchemeName = "NoOp";

//	public NoOpAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
//		: base(options, logger, encoder, clock)
//	{
//	}

//	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
//	{
//		var claims = new[]
//		{
//			new Claim(ClaimTypes.NameIdentifier, "anonymous"),
//			new Claim(ClaimTypes.Name, "Anonymous")
//		};
//		var identity = new ClaimsIdentity(claims, SchemeName);
//		var principal = new ClaimsPrincipal(identity);
//		var ticket = new AuthenticationTicket(principal, SchemeName);

//		return Task.FromResult(AuthenticateResult.Success(ticket));
//	}
//}