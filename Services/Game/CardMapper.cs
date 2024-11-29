using Dominex.Contracts.Game;
using GameCore.Cards;
using Havit.Extensions.DependencyInjection.Abstractions;

namespace Dominex.Services.Game;

[Service]
public class CardMapper : ICardMapper
{
	public CardDto ToCardDto(Card card)
	{
		return new CardDto
		{
			Id = card.Id,
			Name = card.Name,
			Type = card.Type.ToString(),
			Description = card.Description,
			Price = card.Price,
			AddActions = card.AddActions,
			AddBuys = card.AddBuys,
			AddCoins = card.AddCoins,
			Coins = card.Coins,
			DrawCards = card.DrawCards,
			VictoryPoints = card.VictoryPoints
		};
	}
}
