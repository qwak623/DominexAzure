namespace GameCore.Cards.Intrique;
public class SecretChamber : Card
{
	private static SecretChamber secretChamber;
	private SecretChamber() : base([CardType.Action, CardType.Reaction])
	{
		Name = CardName.SecretChamber;
		DefaultPrice = 2;
		secretChamber = this;
		Description = $"Discard any number of cards. +$1 per card discarded.{Environment.NewLine}" +
			$"When another player plays an Attack card, you may reveal this from your hand. " +
			$"If you do, +2 Cards, then put 2 cards from your hand on top of your deck.";
	}

	public static SecretChamber Get() => secretChamber ?? new SecretChamber();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var selectedCards = player.User.SecretChamberDiscard(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList());

		if (selectedCards.Count > 0)
		{
			selectedCards.ForEach(player.Discard);
			player.PlayerState.Coins += selectedCards.Count;
		}
	}

	public override bool Reaction(IPlayer player)
	{
		player.Draw(2);
		// TODO in any order
		var cards = player.User.SecretChamberPutOnDeck(
			this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList(), 2);
		cards.ForEach(player.ReturnToDrawPile);
		return base.Reaction(player);
	}
}

