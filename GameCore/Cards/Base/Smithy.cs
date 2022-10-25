namespace GameCore.Cards.Base;
public class Smithy : Card
{
	private static Smithy smithy;
	private Smithy() : base
	(
		name: "Smithy",
		type: CardType.Smithy,
		price: 4,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 3,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	) => smithy = this;

	public static Smithy Get() => smithy ?? new Smithy();
}
