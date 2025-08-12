using GameCore.Cards;
using GameCore.Observers;

namespace GameCore;
public interface IGame
{
	bool GameEnd { get; }
	Kingdom Kingdom { get; set; }
	IGameLogger Logger { get; set; }
	List<IPlayer> Players { get; set; }
	List<Card> Trash { get; set; }

	Task<GameResults> Play(int maxRounds = 50);
	void RequestPlayerNotifications();
}