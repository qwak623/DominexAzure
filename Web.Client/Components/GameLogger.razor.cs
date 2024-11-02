using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Dominex.Web.Client.Components;

public partial class GameLogger
{
	[Inject] protected NavigationManager Navigation { get; set; }
	private List<GameLogDto> gameLogs { get; set; }// = new List<string>();

	private HubConnection hubConnection;

	protected override async Task OnInitializedAsync()
	{
		// TODO get rid of absolute Uri
		hubConnection = new HubConnectionBuilder()
			.WithUrl(Navigation.ToAbsoluteUri("https://localhost:44301/loghub"))
			.Build();

		hubConnection.On<GameLogDto, int>("AppendLog", async (log, count) =>
		{
			if (gameLogs.Count + 1 != count)
			{
				await hubConnection.SendAsync("RequestLogHistory");
			}
			else
			{
				gameLogs.Add(log);
				StateHasChanged();
			}
		});

		hubConnection.On<List<GameLogDto>>("ReceiveLogHistory", logHistory =>
		{
			gameLogs = logHistory;
			StateHasChanged();
		});

		await hubConnection.StartAsync();
		await hubConnection.SendAsync("RequestLogHistory");
	}

	private string GetPlayerLogsColor(string playerId)
	{
		return (playerId ?? string.Empty) switch
		{
			"Todo Name" => "player1Logs",
			"TODO NAME 2" => "player2Logs",
			_ => "player3Logs"
		};
	}

	public async ValueTask DisposeAsync()
	{
		if (hubConnection != null)
		{
			await hubConnection.DisposeAsync();
		}
	}
}
