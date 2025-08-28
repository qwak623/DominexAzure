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
			Name = card.Name,
			Type = card.Type.ToString(),
			Description = card.Description,
			Message = card.Message,
			Price = card.Price,
			AddActions = card.AddActions,
			AddBuys = card.AddBuys,
			AddCoins = card.AddCoins,
			Coins = card.Coins,
			DrawCards = card.DrawCards,
			VictoryPoints = card.VictoryPoints
		};
	}

	public CardDto ToCardDtoWithIndex(Card card, int index)
	{
		return ToCardDto(card) with { Index = index };
	}

	public IEnumerable<CardDto> ToCardDto(IEnumerable<Card> cards)
	{
		return cards.Select(ToCardDto);
	}
}
