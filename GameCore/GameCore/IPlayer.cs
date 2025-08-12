using System.Runtime.CompilerServices;
using GameCore.Cards;
[assembly: InternalsVisibleTo("GameCoreTests")]

namespace GameCore;
public interface IPlayer
{
	string Name { get; }
	PlayerState PlayerState { get; }
	int CardCount { get; }
	int VictoryPoints { get; }
	IUser User { get; }
	IGame Game { get; }

	Card Buy();
	void Cleanup();
	void DealAttack(IPlayer attacker, Card attackCard);
	void Discard(Card card);
	void DiscardDrawPile();
	void Draw(int count);
	void Gain(CardType type);
	void GainToDrawPile(CardType type);
	void GainToHand(CardType type);
	void Notify();
	Card PlayActionCard();
	void PlayTreasure();
	void PlayTurn(int drawCount);
	void ReturnToDrawPile(Card card);
	List<Card> Show(int count);
	string ToString();
	void Trash(Card card);
}