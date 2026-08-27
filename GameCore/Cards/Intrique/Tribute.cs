namespace GameCore.Cards.Intrique;
public class Tribute : Card
{
	private static Tribute tribute;
	private Tribute() : base(CardType.Action)
	{
		Name = CardName.Tribute;
		DefaultPrice = 5;
		tribute = this;
		Description = $"The player to your left reveals then discards the top 2 cards of their deck. " +
			$"For each differently named card revealed, if it is an…{Environment.NewLine}" +
			$"Action Card, +2 Actions{Environment.NewLine}" +
			$"Treasure Card, +$2{Environment.NewLine}" +
			$"Victory Card, +2 Cards";
	}

	public static Tribute Get() => tribute ?? new Tribute();

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		var otherPlayer = p.Game.Players[(p.Game.Players.IndexOf(p) + 1) % p.Game.Players.Count];

		var revealedCards = otherPlayer?.Show(2);
		foreach (var card in revealedCards.DistinctBy(c => c.Name))
		{
			if (card.Card.IsAction)
			{
				p.PlayerState.Actions += 2;
			}
			if (card.Card.IsTreasure)
			{
				p.PlayerState.Coins += 2;
			}
			if (card.Card.IsVictory)
			{
				p.Draw(2);
			}
		}
		otherPlayer.PlayerState.DiscardPile.MoveAll(revealedCards);
	}
}
