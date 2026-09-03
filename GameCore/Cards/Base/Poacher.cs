namespace GameCore.Cards.Base;
public class Poacher : Card
{
	private static Poacher poacher;
	private Poacher() : base(CardType.Action)
	{
		Name = CardName.Poacher;
		DefaultPrice = 4;
		AddActions = 1;
		AddCoins = 1;
		DrawCards = 1;
		poacher = this;
		Description = $"Discard a card per empty Supply pile.";
	}

	public static Poacher Get() => poacher ?? new Poacher();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		int discardCount = player.Game.Kingdom.Count(kp => kp.Empty);
		var selectedCards = player.User.PoacherDiscard(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList(), discardCount);
		selectedCards.ForEach(player.Discard);
	}
}

