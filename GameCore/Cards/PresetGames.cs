using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;

namespace GameCore.Cards;
public static class PresetGames
{
	public static List<Card> AvailableCards { get; } =
	[
		// base cards
		Adventurer.Get(),
		Artisan.Get(),
		Bandit.Get(),
		Bureaucrat.Get(),
		Cellar.Get(),
		CouncilRoom.Get(),
		Feast.Get(),
		Festival.Get(),
		Gardens.Get(),
		Harbinger.Get(),
		Chancellor.Get(),
		Chapel.Get(),
		Laboratory.Get(),
		Library.Get(),
		Market.Get(),
		Merchant.Get(),
		Militia.Get(),
		Mine.Get(),
		Moat.Get(),
		Moneylender.Get(),
		Poacher.Get(),
		Remodel.Get(),
		Smithy.Get(),
		Spy.Get(),
		Thief.Get(),
		ThroneRoom.Get(),
		Village.Get(),
		Witch.Get(),
		Woodcutter.Get(),
		Workshop.Get(),

		// intrique cards
		Baron.Get(),
		Bridge.Get(),
		Conspirator.Get(),
		Coppersmith.Get(),
		Courtier.Get(),
		Courtyard.Get(),
		Diplomat.Get(),
		Duke.Get(),
		GreatHall.Get(),
		Harem.Get(),
		Ironworks.Get(),
		Lurker.Get(),
		Masquerade.Get(),
		Mill.Get(),
		MiningVillage.Get(),
		Minion.Get(),
		Nobles.Get(),
		Patrol.Get(),
		Pawn.Get(),
		Replace.Get(),
		Saboteur.Get(),
		Scout.Get(),
		SecretChamber.Get(),
		ShantyTown.Get(),
		Steward.Get(),
		Swindler.Get(),
		Torturer.Get(),
		TradingPost.Get(),
		Tribute.Get(),
		Upgrade.Get(),
		WishingWell.Get()
	];

	public static List<Card> VictoryAndTreasures =>
	[
		Copper.Get(),
		Silver.Get(),
		Gold.Get(),
		Estate.Get(),
		Duchy.Get(),
		Province.Get()
	];
}