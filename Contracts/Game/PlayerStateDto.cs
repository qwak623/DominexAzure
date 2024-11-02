namespace Dominex.Contracts.Game;
public class PlayerStateDto
{
	public List<CardDto> Hand { get; set; }
	public string GamePhase { get; set; } // todo enum
	public int Actions { get; set; }
	public int Buys { get; set; }
	public int Coins { get; set; }
}
