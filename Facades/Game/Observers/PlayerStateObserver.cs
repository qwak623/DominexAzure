using Dominex.Contracts.Game;
using Dominex.Facades.Game.Hubs;
using Dominex.Services.Game;
using GameCore;
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
	private readonly ICardMapper cardMapper;

	public PlayerStateObserver(IHubContext<PlayerStateHub> playerStateHubContext, ICardMapper cardMapper)
	{
		this.playerStateHubContext = playerStateHubContext;
		this.cardMapper = cardMapper;
	}
	public async Task Notify(PlayerState playerState)
	{
		PlayerStateDto playerStateDto = new()
		{
			Actions = playerState.Actions,
			Buys = playerState.Buys,
			Coins = playerState.Coins,
			Hand = playerState.Hand.Select(cardMapper.ToCardDto).ToList(),
			GamePhase = "TODO fáze"
		};
		await playerStateHubContext.Clients.All.SendAsync("NotifyPlayerStateChanged", playerStateDto);
	}
}
