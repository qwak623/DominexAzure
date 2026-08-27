namespace GameCore.Cards.Intrique;
public class Patrol : Card
{
	private static Patrol patrol = null;
	private Patrol() : base
	(
		type: CardName.Patrol,
		price: 5,
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
		patrol = this;
		Description = "Reveal the top 4 cards of your deck. Put the Victory cards and Curses into your hand. Put the rest back in any order.";
	}

	public static Patrol Get() => patrol ?? new Patrol();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		Pile revealedCards = player.Show(4);
		revealedCards.Where(c => c.IsVictory || c.Card.Name == CardName.Curse).ToList().ForEach(player.GainToHand);
		List<CardInstance> orderedCards = player.User.PatrolOrderCards(this, player.PlayerState, player.Game.Kingdom, [.. revealedCards]);
		player.PlayerState.DrawPile.MoveRange(orderedCards);
	}
}

