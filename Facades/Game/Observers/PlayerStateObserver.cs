using Dominex.Facades.Game.Hubs;
using GameCore;
using GameCore.GameCore;
using GameCore.Observers;
using Havit.Extensions.DependencyInjection.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Dominex.Facades.Game.Observers;

// todo vyresit jestli toto ma byt singleton (mel by byt jeden pro kazdou hru)
[Service(Lifetime = ServiceLifetime.Singleton)]
public class PlayerStateObserver : IPlayerStateObserver
{
	private readonly IHubContext<PlayerStateHub> playerStateHubContext;

	public PlayerStateObserver(IHubContext<PlayerStateHub> playerStateHubContext)
	{
		this.playerStateHubContext = playerStateHubContext;
	}
	public async Task Notify(PlayerState playerState)
	{
		await playerStateHubContext.Clients.All.SendAsync("NotifyPlayerStateChanged", playerState);
	}
}
