using Dominex.Model.Game;
using Havit.Data.Patterns.DataSeeds;

namespace Dominex.DataLayer.Seeds.Core.Game;

public class PresetKingdomSeed : DataSeed<CoreProfile>
{
	public override void SeedData()
	{
		var kingdoms = new[]
		{
			// Dominion Base Game (First Edition) Preset Kingdoms
			new PresetKingdom
			{
				Name = "First Game (First Edition)",
				Cards =
				[
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
				]
			},
			new PresetKingdom
			{
				Name = "Big Money",
				Cards =
				[
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
				]
			},
			new PresetKingdom
			{
				Name = "Interaction",
				Cards =
				[
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
				]
			},
			new PresetKingdom
			{
				Name = "Size Distortion (First Edition)",
				Cards =
				[
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
				]
			},
			new PresetKingdom
			{
				Name = "Village Square",
				Cards=
				[
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
				]
			},
			new PresetKingdom
			{
				Name = "Thrash Heap",
				Cards =
				[
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
				]
			},

			// Intrique Expansion Preset Kingdoms
			new PresetKingdom
			{
				Name = "Victory Dance",
				Cards =
				[
					"Baron",
					"Courtier",
					"Duke",
					"Harem",
					"Ironworks",
					"Masquerade",
					"Mill",
					"Nobles",
					"Patrol",
					"Replace",
				]
			},
			new PresetKingdom
			{
				Name = "The Plot Thickens",
				Cards =
				[
					"Conspirator",
					"Ironworks",
					"Lurker",
					"MiningVillage",
					"Pawn",
					"SecretPassage",
					"Steward",
					"Swindler",
					"Torturer",
					"TradingPost",
				]
			},
			new PresetKingdom
			{
				Name = "Best Wishes",
				Cards =
				[
					"Baron",
					"Conspirator",
					"Courtyard",
					"Diplomat",
					"Duke",
					"SecretPassage",
					"ShantyTown",
					"Torturer",
					"Upgrade",
					"WishingWell",
				]
			},

			// Intrique Expansion (1E) Preset Kingdoms
			new PresetKingdom
			{
				Name = "Victory Dance (1E)",
				Cards =
				[
					"Baron",
					"Duke",
					"GreatHall",
					"Harem",
					"Ironworks",
					"Masquerade",
					"Nobles",
					"Pawn",
					"Scout",
					"Replace",
				]
			},
			new PresetKingdom
			{
				Name = "The Plot Thickens (1E)",
				Cards =
				[
					"Conspirator",
					"Harem",
					"Ironworks",
					"Pawn",
					"Saboteur",
					"ShantyTown",
					"Steward",
					"Swindler",
					"TradingPost",
					"Tribute",
				]
			},
			new PresetKingdom
			{
				Name = "Best Wishes (1E)",
				Cards =
				[
					"Coppersmith",
					"Courtyard",
					"Masquerade",
					"Scout",
					"ShantyTown",
					"Steward",
					"Torturer",
					"TradingPost",
					"Upgrade",
					"WishingWell",
				]
			},

			// Intrique & Dominion Expansion Preset Kingdoms
			new PresetKingdom
			{
				Name = "Underlings",
				Cards =
				[
					"Courtier",
					"Diplomat",
					"Minion",
					"Nobles",
					"Pawn",
					"Cellar",
					"Festival",
					"Library",
					"Sentry",
					"Vassal",
				]
			},
			new PresetKingdom
			{
				Name = "Grand Scheme",
				Cards =
				[
					"Bridge",
					"Mill",
					"MiningVillage",
					"Patrol",
					"ShantyTown",
					"Artisan",
					"CouncilRoom",
					"Market",
					"Militia",
					"Workshop",
				]
			},
			new PresetKingdom
			{
				Name = "Deconstruction",
				Cards =
				[
					"Diplomat",
					"Harem",
					"Lurker",
					"Replace",
					"Swindler",
					"Bandit",
					"Mine",
					"Remodel",
					"ThroneRoom",
					"Village",
				]
			},

			// Intrique (1E) & Dominion (1E) Expansion Preset Kingdoms
			new PresetKingdom
			{
				Name = "Underlings (1E)",
				Cards =
				[
					"Baron",
					"Masquerade",
					"Minion",
					"Nobles",
					"Pawn",
					"Steward",
					"Cellar",
					"Festival",
					"Library",
					"Witch",
				]
			},
			new PresetKingdom
			{
				Name = "Hand Scheme",
				Cards =
				[
					"Courtyard",
					"Minion",
					"Nobles",
					"Steward",
					"Torturer",
					"Bureaucrat",
					"Chancellor",
					"CouncilRoom",
					"Militia",
					"Mine",
				]
			},
			new PresetKingdom
			{
				Name = "Deconstruction (1E)",
				Cards =
				[
					"Bridge",
					"MiningVillage",
					"Saboteur",
					"SecretChamber",
					"Swindler",
					"Torturer",
					"Remodel",
					"Spy",
					"Thief",
					"ThroneRoom",
				]
			},
		};

		Seed(For(kingdoms).PairBy(k => k.Name));
	}
}
