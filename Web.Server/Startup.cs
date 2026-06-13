using BlazorApplicationInsights;
using Dominex.Contracts;
using Dominex.Contracts.Infrastructure;
using Dominex.DependencyInjection;
using Dominex.Facades.Game.Hubs;
using Dominex.Services.HealthChecks;
using Dominex.Web.Client;
using Dominex.Web.Client.Infrastructure.Configuration;
using Dominex.Web.Server.Infrastructure.Antiforgery;
using Dominex.Web.Server.Infrastructure.ApplicationInsights;
using Dominex.Web.Server.Infrastructure.ConfigurationExtensions;
using Dominex.Web.Server.Infrastructure.ExceptionHandling;
using Dominex.Web.Server.Infrastructure.Security;
using Microsoft.AspNetCore.Components.Authorization;
using Dominex.Web.Server.Infrastructure.HealthChecks;
using Dominex.Web.Server.Infrastructure.MigrationTool;
using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;
using Havit.Blazor.Grpc.Server;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Components.Forms;
using ProtoBuf.Grpc.Server;

namespace Dominex.Web.Server;

// todo pročistit startupy od věcí, které nepoužívám a nerozumím jim
public class Startup
{
	private readonly IConfiguration _configuration;

	public Startup(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddOptions();
		services.Configure<WebClientOptions>(_configuration.GetSection("WebClient"));

		services.ConfigureForWebServer(_configuration);

		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

		services.AddDatabaseDeveloperPageExceptionFilter();

		// todo consider mailing
		//services.AddCustomizedMailing(_configuration);

		// SmtpExceptionMonitoring to errors@havit.cz
		services.AddExceptionMonitoring(_configuration);

		// Application Insights
		services.AddApplicationInsightsTelemetry(_configuration);
		services.AddSingleton<ITelemetryInitializer, GrpcRequestStatusTelemetryInitializer>();
		services.AddSingleton<ITelemetryInitializer, CloudRoleNameTelemetryInitializer>();
		services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) => { module.EnableSqlCommandTextInstrumentation = true; });

		// The configuration is transferred to the client through prerendering
		services.AddBlazorApplicationInsights(c => c.ConnectionString = _configuration.GetSection("ApplicationInsights").GetValue<string>("ConnectionString"));

		// --- Register Havit Hx services on the server as well (prerender / interactive components need these) ---
		services.AddHxServices();
		services.AddHxMessenger();
		services.AddHxMessageBoxHost();
		// -----------------------------------------------------------------------------------------------

		services.AddCustomAuthentication(_configuration);
		services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

		services.AddRazorComponents()
			.AddInteractiveWebAssemblyComponents()
			.AddAuthenticationStateSerialization(options => options.SerializationCallback = Dominex.Web.Server.Infrastructure.Security.CustomClaimsSerializer.SerializeAuthenticationStateAsync);

		services.AddScoped<AntiforgeryStateProvider, WorkaroundEndpointAntiforgeryStateProvider>();

		// server-side UI
		services.AddControllersWithViews();
		services.AddRazorPages();

		// signalR
		services.AddSignalR();

		// gRPC
		services.AddGrpcServerInfrastructure(assemblyToScanForDataContracts: typeof(Dto).Assembly);
		services.AddCodeFirstGrpcReflection();

		// Health checks
		TimeSpan defaultHealthCheckTimeout = TimeSpan.FromSeconds(10);
		services.AddHealthChecks()
			.AddCheck<DominexDbContextHealthCheck>("Database", timeout: defaultHealthCheckTimeout);
		// todo consider mailing server
		//	.AddCheck<MailServiceHealthCheck>("SMTP", timeout: defaultHealthCheckTimeout); 

		// Hangfire
		//services.AddCustomizedHangfire(_configuration);

		// Migrations
		services.Configure<MigrationsOptions>(_configuration.GetSection(MigrationsOptions.Path));
		services.AddHostedService<MigrationHostedService>();
	}

	public void ConfigureMiddleware(WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			app.UseMigrationsEndPoint();
			app.UseWebAssemblyDebugging();
		}
		else
		{
			app.UseExceptionHandler("/error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			// TODO app.UseHsts();
		}

		app.UseMiddleware<CustomResponseForKnownExceptionsMiddleware>();

		app.UseHttpsRedirection();

		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.UseAntiforgery();

		app.UseGrpcWeb(new GrpcWebOptions() { DefaultEnabled = true });
	}

	public void ConfigureEndpoints(WebApplication app)
	{
		app.MapStaticAssets();

		app.MapRazorPages();
		app.MapControllers();
		app.MapRazorComponents<App>()
			.AddInteractiveWebAssemblyRenderMode();
		//.AddAdditionalAssemblies(typeof(Dominex.Web.Client.Program).Assembly);

		// TODO vyčistit
		app.MapHub<LogHub>("/loghub");
		app.MapHub<KingdomHub>("/kingdomhub");
		app.MapHub<PlayerStateHub>("/playerstatehub");


		app.MapGrpcServicesByApiContractAttributes(
				typeof(IDataSeedFacade).Assembly,
				configureEndpointWithAuthorization: endpoint =>
				{
					// endpoints requiring authorization removed; secure later when you restore auth
				});
		app.MapCodeFirstGrpcReflectionService();

		app.MapLoginAndLogout();

		app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
		{
			AllowCachingResponses = false,
			ResponseWriter = HealthCheckWriter.WriteResponseAsync
		});

		// todo hangfire might not be necessary 
		//app.MapHangfireDashboard("/hangfire", new DashboardOptions
		//{
		//	DefaultRecordsPerPage = 50,
		//	Authorization = new List<IDashboardAuthorizationFilter>(), // see https://sahansera.dev/securing-hangfire-dashboard-with-endpoint-routing-auth-policy-aspnetcore/
		//	DisplayStorageConnectionString = false,
		//	DashboardTitle = "Dominex - Jobs",
		//	StatsPollingInterval = 60_000, // once a minute
		//	DisplayNameFunc = (_, job) => Havit.Hangfire.Extensions.Helpers.JobNameHelper.TryGetSimpleName(job, out string simpleName)
		//										? simpleName
		//										: job.ToString()
		//});

		app.MapFallbackToFile("index.html");
	}
}
