using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Dominex.Web.Client.Components;

public partial class Kingdom
{
	[Inject] protected NavigationManager Navigation { get; set; }
	[Inject] protected IGameFacade GameFacade { get; set; }

	private List<PileDto> kingdom = new();

	private HubConnection hubConnection;

	protected async override Task OnParametersSetAsync()
	{
		await GameFacade.RequestKingdomNotification();
	}

	protected override async Task OnInitializedAsync()
	{
		// TODO get rid of absolute Uri
		hubConnection = new HubConnectionBuilder()
			.WithUrl(Navigation.ToAbsoluteUri("https://localhost:44301/kingdomhub"))
			.Build();

		hubConnection.On<List<PileDto>>("NotifyKingdomChanged", kingdom =>
		{
			this.kingdom = kingdom;
			StateHasChanged();
			return Task.CompletedTask;
		});

		await hubConnection.StartAsync();
	}
}
