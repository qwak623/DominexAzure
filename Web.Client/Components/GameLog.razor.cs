using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Dominex.Web.Client.Components;

public partial class GameLog
{
	[Parameter] public List<string> Log { get; set; } = new List<string>();

	private HubConnection? hubConnection;

	protected override async Task OnInitializedAsync()
	{
		// TODO get rid of absolute Uri
		hubConnection = new HubConnectionBuilder()
			.WithUrl(Navigation.ToAbsoluteUri("https://localhost:44301/loghub"))
			.Build();

		hubConnection.On<string>("AppendLog", log =>
		{
			Log.Add(log);

			StateHasChanged();
		});

		await hubConnection.StartAsync();
		//await hubConnection.SendAsync("ReceiveLog", Log);
	}

	public async ValueTask DisposeAsync()
	{
		if (hubConnection != null)
		{
			await hubConnection.DisposeAsync();
		}
	}
}
