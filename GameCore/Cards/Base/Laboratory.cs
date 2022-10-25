namespace GameCore.Cards.Base;
public class Laboratory : Card
{
	private static Laboratory laboratory;
	private Laboratory() : base
	(
		name: "Laboratory",
		type: CardType.Laboratory,
		price: 5,
		addActions: 1,
		addBuys: 0,
		addCoins: 0,
		drawCards: 2,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	) => laboratory = this;

	public static Laboratory Get() => laboratory ?? new Laboratory();
}

