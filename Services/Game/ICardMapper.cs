using Dominex.Contracts.Game;
using GameCore;
using GameCore.Cards;

namespace Dominex.Services.Game;
public interface ICardMapper
{
	CardDto ToCardDto(Card card, PlayerState ps = null);
	CardDto ToCardDtoWithIndex(CardInstance cardInstance, int index, PlayerState ps = null);
	IEnumerable<CardDto> ToCardDto(IEnumerable<Card> card, PlayerState ps = null);
}