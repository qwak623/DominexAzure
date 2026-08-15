namespace GameCore.Cards.Intrique;

public class Courtyard : Card
{
	private static Courtyard courtyard;
	private Courtyard() : base
	(
		name: "Courtyard",
		type: CardType.Courtyard,
		price: 2,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 3,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
		courtyard = this;
		Description = "Put a card from your hand onto your deck.";
		Message = "Choose a card to put on top of your deck.";
	}

	public static Courtyard Get() => courtyard ?? new Courtyard();
	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		// todo maybe it shouldnt ask player to choose a card if they have no cards in hand,
		// but for now it will just return null and do nothing
		// also the same thning is probably in other cards
		var card = player.User.CourtyardPutOnTop(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList());
		if (card is not null)
		{
			player.ReturnToDrawPile(card);
		}
	}
}
