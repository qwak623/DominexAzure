namespace GameCore.Cards.Base;
public class Gardens : Card
{
	private static Gardens gardens;
	private Gardens() : base
	(
		name: "Gardens",
		type: CardType.Gardens,
		price: 4,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 0,
		isVictory: true,
		isTreasure: false,
		isAction: false,
		isReaction: false,
		isAttack: false
	)
	{
		gardens = this;
		Description = "Worth 1 VP per 10 cards you have (round down).";
	}

	public static Gardens Get() => gardens ?? new Gardens();

	public override int CountPoints(IPlayer player) => player.CardCount / 10;
}
