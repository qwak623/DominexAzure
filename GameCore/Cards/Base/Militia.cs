namespace GameCore.Cards.Base;
public class Militia : Card
{
	private static Militia militia;
	private Militia() : base
	(
		name: "Militia",
		type: CardType.Militia,
		price: 4,
		addActions: 0,
		addBuys: 0,
		addCoins: 2,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: true,
		message: "You have to discard down to 3 cards in your hand."
	) => militia = this;

	public static Militia Get() => militia ?? new Militia();

	public override void Attack(Player defender, Player attacker)
	{
		if (defender.ps.Hand.Count <= 3)
			return;
		var cards = defender.User.MilitiaDiscard(defender.ps, defender.Game.Kingdom, defender.ps.Hand.Count - 3);
		cards.ForEach(card => defender.Discard(card));
	}
}

