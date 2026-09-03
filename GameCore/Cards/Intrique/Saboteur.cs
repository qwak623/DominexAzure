namespace GameCore.Cards.Intrique;
public class Saboteur : Card
{
	private static Saboteur saboteur;
	private Saboteur() : base([CardType.Action, CardType.Attack])
	{
		Name = CardName.Saboteur;
		DefaultPrice = 5;
		saboteur = this;
		Description = "Each other player reveals cards from the top of their deck until revealing one costing $3 or more. " +
			"They trash that card and may gain a card costing at most $2 less than it. They discard the other revealed cards.";
	}

	public static Saboteur Get() => saboteur ?? new Saboteur();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		TriggerAttacks(player);
	}

	public override void Attack(IPlayer defender, IPlayer attacker)
	{
		var revealedCards = new Pile();

		while (true)
		{
			var cardsShown = defender.Show(1);
			if (cardsShown.Count == 0)
			{
				break;
			}
			var card = cardsShown.Single();

			if (card.Card.GetPrice(defender.PlayerState) >= 3)
			{
				defender.Trash(card);
				var maxGainPrice = card.Card.GetPrice(defender.PlayerState) - 2;

				var availableCards = defender.Game.Kingdom.GetWrapper(defender.PlayerState, maxGainPrice).AvailableCards.ToList();
				var cardToGain = defender.User.SelectOptionalCardToGain(this, defender.PlayerState, defender.Game.Kingdom, availableCards);
				defender.Gain(cardToGain);
				break;
			}
			else
			{
				revealedCards.Move(card);
			}
		}

		defender.PlayerState.DiscardPile.MoveAll(revealedCards);
	}
}

