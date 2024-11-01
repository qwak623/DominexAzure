using GameCore.GameCore;

namespace GameCore.Observers;

public interface IGameLogger
{
	List<GameLog> LogHistory { get; }
	Task Log(GameLog gameLog);
}
