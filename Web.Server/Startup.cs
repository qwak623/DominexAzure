using Hangfire;
using Hangfire.Dashboard;
using Havit.Blazor.Grpc.Server;
using Dominex.Contracts;
using Dominex.Contracts.Infrastructure;
using Dominex.DependencyInjection;
using Dominex.Facades.Infrastructure.Security;
using Dominex.Model.Security;
using Dominex.Services.HealthChecks;
using Dominex.Services.Infrastructure.MigrationTool;
using Dominex.Web.Server.Infrastructure.ApplicationInsights;
using Dominex.Web.Server.Infrastructure.ConfigurationExtensions;
using Dominex.Web.Server.Infrastructure.HealthChecks;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoBuf.Grpc.Server;
using Dominex.Web.Client.Components;
using Dominex.Facades.Game.Hubs;

namespace Dominex.Web.Server;

// todo pročistit startupy od věcí, které nepoužívám a nerozumím jim
public class Startup
{
	private readonly IConfiguration configuration;

	public Startup(IConfiguration configuration)
	{
		this.configuration = configuration;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		services.ConfigureForWebServer(configuration);

		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

		services.AddDatabaseDeveloperPageExceptionFilter();

		services.AddOptions();

		services.AddCustomizedMailing(configuration);

		// SmtpExceptionMonitoring to errors@havit.cz
		services.AddExceptionMonitoring(configuration);

		// Application Insights
		services.AddApplicationInsightsTelemetry(configuration);
		services.AddSingleton<ITelemetryInitializer, GrpcRequestStatusTelemetryInitializer>();
		services.AddSingleton<ITelemetryInitializer, EnrichmentTelemetryInitializer>();
		services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) => { module.EnableSqlCommandTextInstrumentation = true; });

		services.AddAuthorization(options =>
		{
			options.AddPolicy(PolicyNames.HangfireDashboardAcccessPolicy, policy => policy
				.RequireAuthenticatedUser()
				.RequireRole(nameof(Role.Entry.SystemAdministrator)));
		});
		services.AddCustomizedAuth(configuration);

		// server-side UI
		services.AddControllersWithViews();
		services.AddRazorPages();

		// signalR
		services.AddSignalR();

		// gRPC
		services.AddGrpcServerInfrastructure(assemblyToScanForDataContracts: typeof(Dto).Assembly);
		services.AddCodeFirstGrpcReflection();

		// Health checks
		services.AddHealthChecks()
			.AddCheck<DominexDbContextHealthCheck>("Database");

		// Hangfire
		services.AddCustomizedHangfire(configuration);
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			app.UseMigrationsEndPoint();
			app.UseWebAssemblyDebugging();
		}
		else
		{
			app.UseExceptionHandler("/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			// TODO app.UseHsts();
		}

		app.UseHttpsRedirection();
		app.UseBlazorFrameworkFiles();
		app.UseStaticFiles();

		app.UseExceptionMonitoring();

		app.UseRouting();

		app.UseAuthentication();
		app.UseIdentityServer();
		app.UseAuthorization();

		app.UseGrpcWeb(new GrpcWebOptions() { DefaultEnabled = true });

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapRazorPages();
			endpoints.MapControllers();
			endpoints.MapFallbackToPage("/_Host");

			//vyčistit
			endpoints.MapHub<LogHub>("/loghub");
			endpoints.MapHub<KingdomHub>("/kingdomhub");
			endpoints.MapHub<PlayerStateHub>("/playerstatehub");

			endpoints.MapGrpcServicesByApiContractAttributes(
				typeof(IDataSeedFacade).Assembly,
				configureEndpointWithAuthorization: endpoint =>
				{
					endpoint.RequireAuthorization(); // TODO? AuthorizationPolicyNames.ApiScopePolicy when needed
				});
			endpoints.MapCodeFirstGrpcReflectionService();

			endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
			{
				AllowCachingResponses = false,
				ResponseWriter = HealthCheckWriter.WriteResponse
			});

			endpoints.MapHangfireDashboard("/hangfire", new DashboardOptions
			{
				Authorization = new List<IDashboardAuthorizationFilter>() { }, // see https://sahansera.dev/securing-hangfire-dashboard-with-endpoint-routing-auth-policy-aspnetcore/
				DisplayStorageConnectionString = false,
				DashboardTitle = "Dominex - Jobs",
				StatsPollingInterval = 60_000, // once a minute
				DisplayNameFunc = (_, job) => Havit.Hangfire.Extensions.Helpers.JobNameHelper.TryGetSimpleName(job, out string simpleName)
													? simpleName
													: job.ToString()
			})
			.RequireAuthorization(PolicyNames.HangfireDashboardAcccessPolicy);
		});

		if (configuration.GetValue<bool>("AppSettings:Migrations:RunMigrations"))
		{
			app.ApplicationServices.GetRequiredService<IMigrationService>().UpgradeDatabaseSchemaAndData();
		}
	}
}
