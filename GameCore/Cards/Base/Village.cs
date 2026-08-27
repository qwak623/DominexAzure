namespace GameCore.Cards.Base;
public class Village : Card
{
	private static Village village;
	private Village() : base
	(
		type: CardName.Village,
		price: 3,
		addActions: 2,
		addBuys: 0,
		addCoins: 0,
		drawCards: 1,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	) => village = this;

	public static Village Get() => village ?? new Village();
}
