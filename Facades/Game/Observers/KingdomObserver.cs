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
public class KingdomObserver : IKingdomObserver
{
	private readonly IHubContext<KingdomHub> kingdomHubContext;
	private readonly ICardMapper cardMapper;

	public KingdomObserver(IHubContext<KingdomHub> kingdomHubContext, ICardMapper cardMapper)
	{
		this.kingdomHubContext = kingdomHubContext;
		this.cardMapper = cardMapper;
	}

	public async Task Notify(Kingdom kingdom)
	{
		var kingdomDto = kingdom.Select(pile =>
		{
			CardDto topCard = pile.Card is not null ? cardMapper.ToCardDto(pile.Card) : new CardDto();
			topCard.Name = pile.Name;
			topCard.Type = pile.Type.ToString();
			topCard.Price = pile.Price;
			return new PileDto
			{
				CardCount = pile.Count,
				TopCard = topCard,
			};
		}).ToList();

		await kingdomHubContext.Clients.All.SendAsync("NotifyKingdomChanged", kingdomDto);
	}
}