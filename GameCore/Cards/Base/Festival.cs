namespace GameCore.Cards.Base;
public class Festival : Card
{
	private static Festival festival;
	private Festival() : base
	(
		type: CardName.Festival,
		price: 5,
		addActions: 2,
		addBuys: 1,
		addCoins: 2,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	) => festival = this;

	public static Festival Get() => festival ?? new Festival();
}
