namespace GameCore.Cards.Base;
public class Chapel : Card
{
	private static Chapel chapel = null;
	private Chapel() : base
	(
		name: "Chapel",
		type: CardType.Chapel,
		price: 2,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
		chapel = this;
		Description = "Trash up to 4 cards from your hand.";
		Message = "Trash up to 4 cards from your hand.";
	}

	public static Chapel Get() => chapel ?? new Chapel();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		player.User.ChapelTrash(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList()).ForEach(player.Trash);
	}
}

