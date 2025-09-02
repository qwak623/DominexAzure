namespace Dominex.Contracts.Results;

public class PlayerResultsDto
{
	public string Name { get; set; }
	public int Points { get; set; }
	public List<CardResultsDto> Cards { get; set; }
}
