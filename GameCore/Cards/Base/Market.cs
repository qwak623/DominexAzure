namespace GameCore.Cards.Base;
public class Market : Card
{
	private static Market market;
	private Market() : base
	(
		name: "Market",
		type: CardType.Market,
		price: 5,
		addActions: 1,
		addBuys: 1,
		addCoins: 1,
		drawCards: 1,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	) => market = this;

	public static Market Get() => market ?? new Market();
}
