namespace GameCore.Cards.Intrique;
public class Minion : Card
{
	private static Minion minion;
	private Minion() : base
	(
		name: "Minion",
		type: CardType.Minion,
		price: 5,
		addActions: 1,
		addBuys: 0,
		addCoins: 0,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: true
	)
	{
		minion = this;
		Description = "Choose one: +$2; " +
			"or discard your hand, +4 Cards, and each other player with at least 5 cards in hand discards their hand and draws 4 cards.";
	}

	public static Minion Get() => minion ?? new Minion();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.User.MinionDiscard(this, player.PlayerState, player.Game.Kingdom))
		{
			// TODO hook on discard
			player.PlayerState.DiscardPile.MoveAll(player.PlayerState.Hand);
			player.Draw(4);
			TriggerAttacks(player);
		}
		else
		{
			player.PlayerState.Coins += 2;
		}
	}

	public override void Attack(IPlayer defender, IPlayer attacker)
	{
		if (defender.PlayerState.Hand.Count < 5)
		{
			return;
		}
		// TODO hook on discard
		defender.PlayerState.DiscardPile.MoveAll(defender.PlayerState.Hand);
		defender.Draw(4);
	}
}
