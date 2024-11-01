using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Dominex.Web.Client.Components;

public partial class GameLog
{
	private List<string> Log { get; set; }// = new List<string>();

	private HubConnection? hubConnection;

	protected override async Task OnInitializedAsync()
	{
		// TODO get rid of absolute Uri
		hubConnection = new HubConnectionBuilder()
			.WithUrl(Navigation.ToAbsoluteUri("https://localhost:44301/loghub"))
			.Build();

		hubConnection.On<string, int>("AppendLog", async (log, count) =>
		{
			if (Log.Count + 1 != count)
			{
				await hubConnection.SendAsync("RequestLogHistory");
			}
			else
			{
				Log.Add(log);
				StateHasChanged();
			}
		});

		hubConnection.On<List<string>>("ReceiveLogHistory", async logHistory =>
		{
			Log = logHistory;
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
