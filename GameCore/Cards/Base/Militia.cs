namespace GameCore.Cards.Base;
public class Militia : Card
{
	private static Militia militia;
	private Militia() : base([CardType.Action, CardType.Attack])
	{
		Name = CardName.Militia;
		DefaultPrice = 4;
		AddCoins = 2;
		militia = this;
		Description = "Each other player discards down to 3 cards in hand.";
		Message = "You have to discard down to 3 cards in your hand.";
	}

	public static Militia Get() => militia ?? new Militia();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		TriggerAttacks(player);
	}

	public override void Attack(IPlayer defender, IPlayer attacker)
	{
		if (defender.PlayerState.Hand.Count <= 3)
		{
			return;
		}

		var cards = defender.User.MilitiaDiscard(this, defender.PlayerState, defender.Game.Kingdom, defender.PlayerState.Hand.ToList(), defender.PlayerState.Hand.Count - 3);
		cards.ForEach(defender.Discard);
	}
}

