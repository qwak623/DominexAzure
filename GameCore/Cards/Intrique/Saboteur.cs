namespace GameCore.Cards.Intrique;
public class Saboteur : Card
{
	private static Saboteur saboteur;
	private Saboteur() : base
	(
		name: "Saboteur",
		type: CardType.Saboteur,
		price: 5,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: true
	)
	{
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

				var cardToGain = defender.User.SelectOptionalCardToGain(defender.Game.Kingdom.GetWrapper(defender.PlayerState, maxGainPrice),
					defender.PlayerState, defender.Game.Kingdom, Phase.Attack);
				if (cardToGain != null)
				{
					defender.Gain(cardToGain);
				}
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

