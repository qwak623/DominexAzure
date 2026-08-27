namespace GameCore.Cards.Intrique;
public class Swindler : Card
{
	private static Swindler swindler;
	private Swindler() : base([CardType.Action, CardType.Attack])
	{
		Name = CardName.Swindler;
		DefaultPrice = 3;
		AddCoins = 2;
		swindler = this;
		Description = "Each other player trashes the top card of their deck and gains a card with the same cost that you choose.";
	}

	public static Swindler Get() => swindler ?? new Swindler();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		TriggerAttacks(player);
	}

	public override void Attack(IPlayer defender, IPlayer attacker)
	{
		var cardToTrash = defender.Show(1).SingleOrDefault();
		if (cardToTrash == null)
		{
			return;
		}

		defender.Trash(cardToTrash);
		var price = cardToTrash.Card.GetPrice(defender.PlayerState);
		var cardToGain = attacker.User.SelectCardToGain(
			new KingdomWrapper() { Kingdom = defender.Game.Kingdom, MinPrice = price, MaxPrice = price, PlayerState = defender.PlayerState },
			defender.PlayerState, defender.Game.Kingdom, Phase.Action);
		if (cardToGain != null)
		{
			defender.Gain(cardToGain);
		}
	}
}
