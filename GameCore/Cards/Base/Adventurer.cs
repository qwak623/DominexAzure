using GameCore.GameCore;

namespace GameCore.Cards.Base;

public class Adventurer : Card
{
	private static Adventurer adventurer;
	private Adventurer() : base(CardType.Action)
	{
		Name = CardName.Adventurer;
		DefaultPrice = 6;
		adventurer = this;
		Description = "Reveal cards from your deck until you reveal 2 Treasure cards. " +
			"Put those Treasure cards into your hand and discard the other revealed cards.";
	}

	public static Adventurer Get() => adventurer ?? new Adventurer();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var revealedCards = new Pile();
		int treasuresDrawn = 0;
		while (treasuresDrawn < 2)
		{
			var cardsShown = player.Show(1);
			if (cardsShown.Count == 0)
			{
				break;
			}
			var card = cardsShown.Single();

			if (card.IsTreasure)
			{
				player.Game.Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} draws {card.Card.Name.ToDisplayName()}" });
				player.PlayerState.Hand.Move(card);
				treasuresDrawn++;
			}
			else
			{
				revealedCards.Move(card);
			}
		}
		player.PlayerState.DiscardPile.MoveAll(revealedCards);
	}
}
