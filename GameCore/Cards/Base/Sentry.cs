namespace GameCore.Cards.Base;
public class Sentry : Card
{
	private static Sentry sentry;
	private Sentry() : base(CardType.Action)
	{
		Name = CardName.Sentry;
		AddActions = 1;
		DrawCards = 1;
		DefaultPrice = 5;
		sentry = this;
		Description = $"Look at the top 2 cards of your deck. " +
			$"Trash and/or discard any number of them. Put the rest back on top in any order.";
	}

	public static Sentry Get() => sentry ?? new Sentry();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var revealed = player.Show(2);
		if (revealed.Count == 0)
		{
			return;
		}

		var toTrash = player.User.SentryTrash(this, player.PlayerState, player.Game.Kingdom, revealed.ToList());
		toTrash.ForEach(player.Trash);
		if (revealed.Count == 0)
		{
			return;
		}

		var toDiscard = player.User.SentryDiscard(this, player.PlayerState, player.Game.Kingdom, revealed.ToList());
		toDiscard.ForEach(player.Discard);
		if (revealed.Count == 0)
		{
			return;
		}

		var ordered = player.User.SentryOrderCards(this, player.PlayerState, player.Game.Kingdom, revealed.ToList());
		player.PlayerState.DrawPile.MoveRange(ordered);
	}
}
