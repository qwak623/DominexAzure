using Dominex.Contracts.Game;
using GameCore.Cards;

namespace Dominex.Services.Game;
public interface ICardMapper
{
	CardDto ToCardDto(Card card, int index);
}