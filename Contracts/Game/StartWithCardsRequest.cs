namespace Dominex.Contracts.Game;

public class StartWithCardsRequest
{
	public List<string> CardTypes { get; set; }
	public bool AddColonyAndPlatinum { get; set; }
}
