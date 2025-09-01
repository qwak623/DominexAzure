using Dominex.Model.Game;
using Havit.Data.Patterns.DataSeeds;

namespace Dominex.DataLayer.Seeds.Core.Game;

public class PresetKingdomSeed : DataSeed<CoreProfile>
{
	public override void SeedData()
	{
		var kingdoms = new[]
		{
			new PresetKingdom
			{
				Name = "First Game (First Edition)",
				Cards = new()
				{
					"Cellar",
					"Market",
					"Militia",
					"Mine",
					"Moat",
					"Remodel",
					"Smithy",
					"Village",
					"Woodcutter",
					"Workshop",
				}
			},
			new PresetKingdom
			{
				Name = "Big Money",
				Cards = new()
				{
					"Adventurer",
					"Bureaucrat",
					"Chancellor",
					"Chapel",
					"Feast",
					"Laboratory",
					"Market",
					"Mine",
					"Moneylender",
					"ThroneRoom",
				}
			},
			new PresetKingdom
			{
				Name = "Interaction",
				Cards = new()
				{
					"Bureaucrat",
					"Chancellor",
					"CouncilRoom",
					"Festival",
					"Library",
					"Militia",
					"Moat",
					"Spy",
					"Thief",
					"Village",
				}
			},
			new PresetKingdom
			{
				Name = "Size Distortion (First Edition)",
				Cards = new()
				{
					"Cellar",
					"Chapel",
					"Feast",
					"Gardens",
					"Laboratory",
					"Thief",
					"Village",
					"Witch",
					"Woodcutter",
					"Workshop",
				}
			},
			new PresetKingdom
			{
				Name = "Village Square",
				Cards= new()
				{
					"Bureaucrat",
					"Cellar",
					"Festival",
					"Library",
					"Market",
					"Remodel",
					"Smithy",
					"ThroneRoom",
					"Village",
					"Woodcutter",
				}
			},
			new PresetKingdom
			{
				Name = "Thrash Heap",
				Cards = new()
				{
					"Chapel",
					"Feast",
					"Festival",
					"Market",
					"Mine",
					"Moneylender",
					"Remodel",
					"Village",
					"Woodcutter",
					"Workshop",
				}
			}
		};

		Seed(For(kingdoms).PairBy(k => k.Name));
	}
}
