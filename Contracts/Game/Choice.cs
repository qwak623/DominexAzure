using GameCore.Cards;

namespace Dominex.Contracts.Game;
public class Choice
{
	public State State { get; set; }
	public List<string> Cards { get; set; } // choice
}
