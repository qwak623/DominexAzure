using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Dominex.Web.Client.Components;

public partial class PlayerState
{
	[Inject] protected NavigationManager Navigation { get; set; }
	private PlayerStateDto PlayerStateDto { get; set; }

	private HubConnection hubConnection;

	protected override async Task OnInitializedAsync()
	{
		// TODO get rid of absolute Uri
		hubConnection = new HubConnectionBuilder()
			.WithUrl(Navigation.ToAbsoluteUri("https://localhost:44301/playerstatehub"))
			.Build();

		hubConnection.On<PlayerStateDto>("NotifyPlayerStateChanged", playerStateDto =>
		{
			PlayerStateDto = playerStateDto;
			StateHasChanged();
			return Task.CompletedTask;
		});

		await hubConnection.StartAsync();
	}
}
