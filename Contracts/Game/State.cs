using GameCore;

// todo přesunout někam kam to dává smysl
namespace Dominex.Contracts.Game;
public class State
{
	public GameResults GameResults { get; set; }
	public int CurrentPlayer { get; set; }
	public int Phase { get; set; } // todo enum
}
