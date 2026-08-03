using Dominex.Contracts.Game;
using Dominex.Facades.Game.Hubs;
using Dominex.Services.Game;
using GameCore.Cards;
using GameCore.Observers;
using Havit.Extensions.DependencyInjection.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Dominex.Facades.Game.Observers;

[Service(Lifetime = ServiceLifetime.Singleton)]
public class KingdomObserver(IHubContext<KingdomHub> kingdomHubContext, ICardMapper cardMapper) : IKingdomObserver
{
	public async Task Notify(Kingdom kingdom)
	{
		var kingdomDto = kingdom.Select(pile =>
		{
			CardDto topCard = pile.CardInstance is not null ? cardMapper.ToCardDtoWithIndex(pile.CardInstance, 0) : new CardDto();
			topCard.Name = pile.Name;
			topCard.Type = pile.Type.ToString();
			topCard.Price = pile.CardInstance.Card.DefaultPrice;
			return new PileDto
			{
				CardCount = pile.Count,
				TopCard = topCard,
			};
		}).ToList();

		await kingdomHubContext.Clients.All.SendAsync("NotifyKingdomChanged", kingdomDto);
	}
}