namespace GameCore.Cards.GeneralCards;

public class Duchy : Card
{
	private static Duchy duchy;
	private Duchy() : base
	(
		name: "Duchy",
		type: CardType.Duchy,
		price: 5,
		addBuys: 0,
		coins: 0,
		isVictory: true,
		isTreasure: false
	)
	{
		duchy = this;
		VictoryPoints = 3;
	}

	public static Duchy Get() => duchy ?? new Duchy();
}
