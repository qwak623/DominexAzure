using Dominex.Contracts.Game;
using Dominex.Facades.Game.Hubs;
using GameCore.Cards;
using GameCore.Observers;
using Havit.Extensions.DependencyInjection.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Dominex.Facades.Game.Observers;

[Service(Lifetime = ServiceLifetime.Singleton)]
public class KingdomObserver : IKingdomObserver
{
	private readonly IHubContext<KingdomHub> kingdomHubContext;

	public KingdomObserver(IHubContext<KingdomHub> kingdomHubContext)
	{
		this.kingdomHubContext = kingdomHubContext;
	}

	public async Task Notify(Kingdom kingdom)
	{
		var kingdomDto = kingdom
			.Select(pile => new PileDto
			{
				CardCount = pile.Count,
				TopCard = new CardDto
				{
					CardName = pile.Name,
					CardType = pile.Type.ToString(),
					Description = pile.Card.Description,
					Price = pile.Price
				}
			})
			.ToList();

		await kingdomHubContext.Clients.All.SendAsync("NotifyKingdomChanged", kingdomDto);
	}
}