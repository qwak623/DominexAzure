namespace Dominex.Contracts.Game;
public record InfoDto
{
	public GameInfoDto GameInfo { get; set; }
	public PlayerInfoDto PlayerInfo { get; set; }
}
