using Dominex.Contracts.Game;
using GameCore;
using GameCore.Cards;
using Havit.Extensions.DependencyInjection.Abstractions;

namespace Dominex.Services.Game;

[Service]
public class CardMapper : ICardMapper
{
	public CardDto ToCardDto(Card card, PlayerState ps = null)
	{
		return new CardDto
		{
			Name = card.Name.ToDisplayName(),
			Type = card.Name.ToString(),
			Description = card.Description,
			Message = card.Message,
			Price = card.GetPrice(ps),
			AddActions = card.AddActions,
			AddBuys = card.AddBuys,
			AddCoins = card.AddCoins,
			Coins = card.Coins,
			DrawCards = card.DrawCards,
			VictoryPoints = card.VictoryPoints,
			IsAction = card.IsAction,
			IsReaction = card.IsReaction,
			IsTreasure = card.IsTreasure,
			IsVictory = card.IsVictory
		};
	}

	// todo i might not need this since card already have id
	public CardDto ToCardDtoWithIndex(CardInstance card, int index, PlayerState ps = null)
	{
		return ToCardDto(card.Card, ps) with { Index = index };
	}

	public IEnumerable<CardDto> ToCardDto(IEnumerable<Card> cards, PlayerState ps = null)
	{
		return cards.Select(c => ToCardDto(c, ps));
	}
}
