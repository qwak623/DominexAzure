using Dominex.Contracts.Game;
using Dominex.Facades.Game.Hubs;
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

	public PlayerStateObserver(IHubContext<PlayerStateHub> playerStateHubContext)
	{
		this.playerStateHubContext = playerStateHubContext;
	}
	public async Task Notify(PlayerState playerState)
	{
		PlayerStateDto playerStateDto = new()
		{
			Actions = playerState.Actions,
			Buys = playerState.Buys,
			Coins = playerState.Coins,
			Hand = playerState.Hand.Select(c => new CardDto
			{
				CardName = c.Name,
				CardType = c.Type.ToString(),
				Description = c.Description,
				Price = c.Price
			}).ToList(),
			GamePhase = "TODO fáze"
		};
		await playerStateHubContext.Clients.All.SendAsync("NotifyPlayerStateChanged", playerStateDto);
	}
}
