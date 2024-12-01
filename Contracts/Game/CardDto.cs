namespace Dominex.Contracts.Game;
public record CardDto
{
	public int? Index { get; set; }
	public string Name { get; set; }
	public string Type { get; set; }
	public string Description { get; set; }
	public string Message { get; set; }
	public int Price { get; set; }
	public int AddActions { get; set; }
	public int AddBuys { get; set; }
	public int AddCoins { get; set; }
	public int Coins { get; set; }
	public int DrawCards { get; set; }
	public int VictoryPoints { get; set; }
}