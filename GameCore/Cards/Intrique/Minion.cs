namespace GameCore.Cards.Intrique;
public class Minion : Card
{
	private static Minion minion;
	private Minion() : base([CardType.Action, CardType.Attack])
	{
		Name = CardName.Minion;
		DefaultPrice = 5;
		AddActions = 1;
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
