namespace GameCore.Cards.Intrique;
public class Scout : Card
{
	private static Scout scout = null;
	private Scout() : base(CardType.Action)
	{
		Name = CardName.Scout;
		DefaultPrice = 4;
		AddActions = 1;
		scout = this;
		Description = "Reveal the top 4 cards of your deck. Put the revealed Victory cards into your hand. " +
			"Put the other cards on top of your deck in any order.";
	}

	public static Scout Get() => scout ?? new Scout();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		Pile revealedCards = player.Show(4);
		revealedCards.Where(c => c.IsVictory).ToList().ForEach(player.GainToHand);
		List<CardInstance> orderedCards = player.User.ScoutOrderCards(this, player.PlayerState, player.Game.Kingdom, [.. revealedCards]);
		player.PlayerState.DrawPile.MoveRange(orderedCards);
	}
}

