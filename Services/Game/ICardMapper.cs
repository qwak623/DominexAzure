using Dominex.Contracts.Game;
using GameCore.Cards;

namespace Dominex.Services.Game;
public interface ICardMapper
{
	CardDto ToCardDto(Card card);
	CardDto ToCardDtoWithIndex(CardInstance cardInstance, int index);
	IEnumerable<CardDto> ToCardDto(IEnumerable<Card> card);
}