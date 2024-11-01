using Dominex.Contracts.Game;
using Microsoft.AspNetCore.SignalR.Client;

namespace Dominex.Web.Client.Components;

public partial class GameLogger
{
	private List<GameLogDto> GameLogs { get; set; }// = new List<string>();

	private HubConnection? hubConnection;

	protected override async Task OnInitializedAsync()
	{
		// TODO get rid of absolute Uri
		hubConnection = new HubConnectionBuilder()
			.WithUrl(Navigation.ToAbsoluteUri("https://localhost:44301/loghub"))
			.Build();

		hubConnection.On<GameLogDto, int>("AppendLog", async (log, count) =>
		{
			if (GameLogs.Count + 1 != count)
			{
				await hubConnection.SendAsync("RequestLogHistory");
			}
			else
			{
				GameLogs.Add(log);
				StateHasChanged();
			}
		});

		hubConnection.On<List<GameLogDto>>("ReceiveLogHistory", logHistory =>
		{
			GameLogs = logHistory;
			StateHasChanged();
		});

		await hubConnection.StartAsync();
		await hubConnection.SendAsync("RequestLogHistory");
	}

	public async ValueTask DisposeAsync()
	{
		if (hubConnection != null)
		{
			await hubConnection.DisposeAsync();
		}
	}
}
