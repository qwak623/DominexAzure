using GameCore.Observers;
using Microsoft.AspNetCore.SignalR;

namespace Dominex.Facades.Game.Hubs;

public class LogHub : Hub
{
	private readonly IGameLogger gameLogger;

	public LogHub(IGameLogger gameLogger)
	{
		this.gameLogger = gameLogger;
	}

	public async Task RequestLogHistory()
	{
		await Clients.Caller.SendAsync("ReceiveLogHistory", gameLogger.LogHistory);
	}
}
