namespace GameCore.Cards.Base;

public class Woodcutter : Card
{
	private static Woodcutter woodcutter;
	private Woodcutter() : base
	(
		name: "Woodcutter",
		type: CardType.Woodcutter,
		price: 3,
		addActions: 0,
		addBuys: 1,
		addCoins: 2,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	) => woodcutter = this;

	public static Woodcutter Get() => woodcutter ?? new Woodcutter();
}
