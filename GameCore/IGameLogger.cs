using GameCore.GameCore;

namespace GameCore;

public interface IGameLogger
{
	List<GameLog> LogHistory { get; }
	Task Log(GameLog gameLog);
}
