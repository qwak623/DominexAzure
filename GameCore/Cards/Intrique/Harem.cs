namespace GameCore.Cards.Intrique;
public class Harem : Card
{
	// TODO Use 8 Harems/Farms for games with 2 players, 12 for games with 3 or more players.
	private static Harem harem;
	private Harem() : base
	(
		name: "Harem",
		type: CardType.Harem,
		price: 0,
		addBuys: 0,
		coins: 2,
		isVictory: true,
		isTreasure: true
	)
	{
		harem = this;
		VictoryPoints = 2;
	}

	public static Harem Get() => harem ?? new Harem();
}
