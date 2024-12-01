using Dominex.Contracts.Game;
using GameCore.Cards;

namespace Dominex.Services.Game;
public interface ICardMapper
{
	CardDto ToCardDto(Card card);
	CardDto ToCardDtoWithIndex(Card card, int index);
}