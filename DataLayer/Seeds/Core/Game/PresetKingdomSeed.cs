using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using Dominex.Model.Game;
using Havit.Data.Patterns.DataSeeds;

namespace Dominex.DataLayer.Seeds.Core.Game;

public class PresetKingdomSeed : DataSeed<CoreProfile>
{
	public override void SeedData()
	{
		var kingdoms = new[]
		{
			// Dominion Base Game (1E) Preset Kingdoms
			new PresetKingdom
			{
				Name = "First Game (1E)",
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
				Name = "Size Distortion (1E)",
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

			// Prosperity Expansion Preset Kingdoms
			new PresetKingdom
			{
				Name = "Beginners",
				Cards =
				[
					"Bank",
					"Clerk",
					"CrystalBall",
					"Expand",
					"Magnate",
					"Monument",
					"Rabble",
					"Tiara",
					"Watchtower",
					"WorkesVillage",
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "Friendly Interactive",
				Cards =
				[
					"Bishop",
					"City",
					"Collection",
					"Forge",
					"Hoard",
					"Peddler",
					"Tiara",
					"Vault",
					"WarChest",
					"WorkersVillage",
				],
				AddColonyAndPlatinum = true,
			},

			// Prosperity Expansion (1E) Preset Kingdoms
			new PresetKingdom
			{
				Name = "Beginners (1E)",
				Cards =
				[
					"Bank",
					"CountingHouse",
					"Expand",
					"Goons",
					"Monument",
					"Rabble",
					"RoyalSeal",
					"Venture",
					"Watchtower",
					"WorkesVillage",
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "Friendly Interactive (1E)",
				Cards =
				[
					"Bishop",
					"City",
					"Contraband",
					"Forge",
					"Hoard",
					"Peddler",
					"RoyalSeal",
					"TradeRoute",
					"Vault",
					"WorkersVillage",
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "Big Actions",
				Cards =
				[
					"City",
					"Expand",
					"GrandMarket",
					"KingsCourt",
					"Loan",
					"Mint",
					"Quarry",
					"Rabble",
					"Talisman",
					"Vault",
				],
				AddColonyAndPlatinum = true,
			},

			// Prosperity & Dominion Expansion Preset Kingdoms
			new PresetKingdom
			{
				Name = "Biggest Money",
				Cards =
				[
					"Bank",
					"CrystalBall",
					"GrandMarket",
					"Mint",
					"Tiara",
					"Artisan",
					"Harbinger",
					"Laboratory",
					"Mine",
					"Moneylender",
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "The King's Army",
				Cards =
				[
					"Collection",
					"Expand",
					"KingsCourt",
					"Rabble",
					"Vault",
					"Bureaucrat",
					"CouncilRoom",
					"Merchant",
					"Moat",
					"Village",
				],
				AddColonyAndPlatinum = true,
			},

			// Dominion (1E) & Prosperity (1E) Expansion Preset Kingdoms
			new PresetKingdom
			{
				Name = "Biggest Money (1E)",
				Cards =
				[
					"Bank",
					"GrandMarket",
					"Mint",
					"RoyalSeal",
					"Venture",
					"Adventurer",
					"Laboratory",
					"Mine",
					"Moneylender",
					"Spy"
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "The King's Army (1E)",
				Cards =
				[
					"Expand",
					"Goons",
					"KingsCourt",
					"Rabble",
					"Vault",
					"Bureaucrat",
					"CouncilRoom",
					"Moat",
					"Spy",
					"Village"
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "The Good Life (1E)",
				Cards =
				[
					"Contraband",
					"CountingHouse",
					"Hoard",
					"Monument",
					"Mountebank",
					"Bureaucrat",
					"Cellar",
					"Chancellor",
					"Gardens",
					"Village"
				],
				AddColonyAndPlatinum = true,
			},

			// Dominion (2E) & Prosperity (1E) Expansion Preset Kingdoms
			new PresetKingdom
			{
				Name = "Biggest Money (1E & 2E)",
				Cards =
				[
					"Bank",
					"GrandMarket",
					"Mint",
					"RoyalSeal",
					"Venture",
					"Artisan",
					"Harbinger",
					"Laboratory",
					"Mine",
					"Moneylender"
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "The King's Army (1E & 2E)",
				Cards =
				[
					"Expand",
					"Goons",
					"KingsCourt",
					"Rabble",
					"Vault",
					"Bureaucrat",
					"CouncilRoom",
					"Merchant",
					"Moat",
					"Village"
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "The Good Life (1E & 2E)",
				Cards =
				[
					"Contraband",
					"CountingHouse",
					"Hoard",
					"Monument",
					"Mountebank",
					"Artisan",
					"Bureaucrat",
					"Cellar",
					"Gardens",
					"Village"
				],
				AddColonyAndPlatinum = true,
			},

			// Prosperity & Intrique Expansion Preset Kingdoms
			new PresetKingdom
			{
				Name = "Paths to Victory",
				Cards =
				[
					"Bishop",
					"Collection",
					"Magnate",
					"Monument",
					"Peddler",
					"Baron",
					"Harem",
					"Pawn",
					"ShantyTown",
					"Upgrade"
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "Lucky Seven",
				Cards =
				[
					"Bank",
					"Expand",
					"Forge",
					"KingsCourt",
					"Tiara",
					"Baron",
					"MiningVillage",
					"Patrol",
					"Upgrade",
					"WishingWell"
				],
				AddColonyAndPlatinum = true,
			},

			// Prosperity (1E) & Intrigue (1E) Expansion Preset Kingdoms
			new PresetKingdom
			{
				Name = "Paths to Victory (1E)",
				Cards =
				[
					"Bishop",
					"CountingHouse",
					"Goons",
					"Monument",
					"Peddler",
					"Baron",
					"Harem",
					"Pawn",
					"ShantyTown",
					"Upgrade"
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "All Along the Watchtower (1E)",
				Cards =
				[
					"Hoard",
					"Talisman",
					"TradeRoute",
					"Vault",
					"Watchtower",
					"Bridge",
					"GreatHall",
					"MiningVillage",
					"Pawn",
					"Torturer"
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "Lucky Seven (1E)",
				Cards =
				[
					"Bank",
					"Expand",
					"Forge",
					"KingsCourt",
					"Vault",
					"Bridge",
					"Coppersmith",
					"Swindler",
					"Tribute",
					"WishingWell"
				],
				AddColonyAndPlatinum = true,
			},

			// Prosperity (1E) & Intrigue (2E) Expansion Preset Kingdoms
			new PresetKingdom
			{
				Name = "All Along the Watchtower (1E & 2E)",
				Cards =
				[
					"Hoard",
					"Talisman",
					"TradeRoute",
					"Vault",
					"Watchtower",
					"Bridge",
					"Mill",
					"MiningVillage",
					"Pawn",
					"Torturer"
				],
				AddColonyAndPlatinum = true,
			},
			new PresetKingdom
			{
				Name = "Lucky Seven (1E & 2E)",
				Cards =
				[
					"Bank",
					"Expand",
					"Forge",
					"KingsCourt",
					"Vault",
					"Bridge",
					"Lurker",
					"Patrol",
					"Swindler",
					"WishingWell"
				],
				AddColonyAndPlatinum = true,
			},
		};


		Seed(For(kingdoms).PairBy(k => k.Name));
	}
}
