using GameCore.Observers;
using Microsoft.AspNetCore.SignalR;

namespace Dominex.Facades.Game.Hubs;

public class LogHub(IGameLogger gameLogger) : Hub
{
	public async Task RequestLogHistory()
	{
		await Clients.Caller.SendAsync("ReceiveLogHistory", gameLogger.LogHistory);
	}
}
