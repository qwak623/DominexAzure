namespace GameCore.Cards.Base;
public class Bandit : Card
{
	private static Bandit bandit;

	private Bandit() : base([CardType.Action, CardType.Attack])
	{
		Name = CardName.Bandit;
		DefaultPrice = 5;
		bandit = this;
		Description = "Gain a Gold. " +
			"Each other player reveals the top 2 cards of their deck, trashes a revealed Treasure other than Copper, and discards the rest.";
	}

	public static Bandit Get() => bandit ?? new Bandit();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		player.Gain(CardName.Gold);
		TriggerAttacks(player);
	}

	public override void Attack(IPlayer defender, IPlayer attacker)
	{
		var cards = defender.Show(2);
		var treasures = cards.Where(c => c.IsTreasure && c.Card.Name != CardName.Copper).ToList();
		var card = attacker.User.BanditTrash(this, defender.PlayerState, defender.Game.Kingdom, treasures);
		defender.Trash(card);
		defender.PlayerState.DiscardPile.MoveAll(cards);
	}
}