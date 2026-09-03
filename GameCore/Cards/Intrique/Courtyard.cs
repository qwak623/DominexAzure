namespace GameCore.Cards.Intrique;

public class Courtyard : Card
{
	private static Courtyard courtyard;
	private Courtyard() : base(CardType.Action)
	{
		Name = CardName.Courtyard;
		DefaultPrice = 2;
		DrawCards = 3;
		courtyard = this;
		Description = "Put a card from your hand onto your deck.";
		Message = "Choose a card to put on top of your deck.";
	}

	public static Courtyard Get() => courtyard ?? new Courtyard();
	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var card = player.User.CourtyardPutOnTop(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList());
		player.ReturnToDrawPile(card);
	}
}
