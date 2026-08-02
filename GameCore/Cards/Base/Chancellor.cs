using GameCore.GameCore;

namespace GameCore.Cards.Base;
public class Chancellor : Card
{
	private static Chancellor chancellor;
	private Chancellor() : base
	(
		name: "Chancellor",
		type: CardType.Chancellor,
		price: 3,
		addActions: 0,
		addBuys: 0,
		addCoins: 2,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
		chancellor = this;
		Description = "You may immediately put your deck into your discard pile.";
	}

	public static Chancellor Get() => chancellor ?? new Chancellor();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.User.ChancellorDiscard(this, player.PlayerState, player.Game.Kingdom))
		{
			player.Game.Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} discards draw pile." });
			player.PlayerState.DiscardPile.MoveAll(player.PlayerState.DrawPile);
		}
	}
}
