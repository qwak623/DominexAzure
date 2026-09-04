using System.ComponentModel.DataAnnotations;

namespace Dominex.Model.Game;
public class PresetKingdom
{
	[Key]
	[Required]
	public int Id { get; set; }

	[Required]
	public string Name { get; set; }

	[Required]
	public List<string> Cards { get; set; }
	public bool AddColonyAndPlatinum { get; set; } = false;
}
