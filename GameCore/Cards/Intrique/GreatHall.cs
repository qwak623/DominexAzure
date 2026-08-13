namespace GameCore.Cards.Intrique;
public class GreatHall : Card
{
	private static GreatHall greatHall;
	private GreatHall() : base
	(
		name: "Great Hall",
		type: CardType.GreatHall,
		price: 3,
		addActions: 1,
		addBuys: 0,
		addCoins: 0,
		drawCards: 1,
		isVictory: true,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
		greatHall = this;
		VictoryPoints = 1;
	}

	public static GreatHall Get() => greatHall ?? new GreatHall();
}
