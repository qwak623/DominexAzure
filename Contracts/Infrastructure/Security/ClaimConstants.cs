namespace Dominex.Contracts.Infrastructure.Security;

/// <summary>
/// Central place for well-known claim names and issuer used across the application.
/// Restoring this prevents magic strings and makes tests/configuration consistent.
/// </summary>
public static class ClaimConstants
{
	/// <summary>
	/// Issuer used when creating application-scoped claims (used by cookie/refresh helpers).
	/// </summary>
	public const string ApplicationIssuer = "Dominex";

	/// <summary>
	/// Claim that carries the application user id.
	/// </summary>
	public const string UserIdClaim = "dominex:user_id";

	// Add more application-specific claim names here as needed, e.g.:
	// public const string TenantIdClaim = "dominex:tenant_id";
	// public const string PreferredLocaleClaim = "dominex:locale";
}
