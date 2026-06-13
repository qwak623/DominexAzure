namespace Dominex.Web.Server.Infrastructure.ConfigurationExtensions;

public static class AuthConfig
{
	// No-op placeholder. Authentication/authorization removed temporarily.
	// Restore real registration when you implement security again.
	public static void AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
	{
		// intentionally left blank
	}

	// Keep old name (if any code calls AddCustomizedAuth)
	public static void AddCustomizedAuth(this IServiceCollection services, IConfiguration configuration)
		=> AddCustomAuthentication(services, configuration);
}
