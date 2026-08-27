using System.Runtime.CompilerServices;
using GameCore.Cards;
[assembly: InternalsVisibleTo("GameCoreTests")]

namespace GameCore;
public interface IPlayer
{
	string Name { get; }
	PlayerState PlayerState { get; }
	int CardCount { get; }
	IUser User { get; }
	IGame Game { get; }

	CardInstance Buy();
	void Cleanup();
	void DealAttack(IPlayer attacker, Card attackCard);
	void Discard(CardInstance card);
	void Draw(int count);
	void FinalCleanup();
	// TODO nebude tohle zbytečné?
	int GetVictoryPoints();
	void Gain(CardName type);
	void Gain(CardInstance card);
	void GainToDrawPile(CardName type);
	void GainToHand(CardName type);
	void GainToHand(CardInstance card);
	void Notify();
	CardInstance PlayActionCard();
	void PlayTreasure();
	void PlayTurn(int drawCount);
	void ReturnToDrawPile(CardInstance card);
	Pile Show(int count);
	string ToString();
	void Trash(CardInstance card);
}