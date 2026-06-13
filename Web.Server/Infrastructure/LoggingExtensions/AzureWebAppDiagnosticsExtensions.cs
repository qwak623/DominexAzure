using Microsoft.Extensions.Options;

namespace Dominex.Web.Server.Infrastructure.LoggingExtensions;

public static class AzureWebAppDiagnosticsExtensions
{
	public static ILoggingBuilder AddCustomizedAzureWebAppDiagnostics(this ILoggingBuilder builder)
	{
		builder.AddAzureWebAppDiagnostics();

		// Running in Azure App Service?
		// inspiration: https://github.com/dotnet/aspnetcore/blob/c00e0e775208cb7cb377f9bd0c8a66a0b3d0ed4d/src/Logging.AzureAppServices/src/WebAppContext.cs#L30
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")))
		{
			// see ConfigurationBasedLevelSwitcherRemoval comment
			builder.Services.AddSingleton<IConfigureOptions<LoggerFilterOptions>, ConfigurationBasedLevelSwitcherRemoval>();
		}
		return builder;
	}

}