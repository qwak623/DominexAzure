using GameCore.GameCore;

namespace GameCore.Cards.Base;
public class Thief : Card
{
	private static Thief thief;
	private Thief() : base
	(
		name: "Thief",
		type: CardType.Thief,
		price: 4,
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
		thief = this;
		Description = $"Each other player reveals the top 2 cards of his deck.{Environment.NewLine}If they revealed any Treasure cards, they trash one of them that you choose. You may gain any or all of these trashed cards. They discard the other revealed cards.";
		Message = "Choose treasure to steal or trash.";
	}

	public static Thief Get() => thief ?? new Thief();

	public override void Attack(IPlayer defender, IPlayer attacker)
	{
		// show two cards
		var cards = defender.Show(2);
		// selecting treasures
		var treasures = cards.Where(c => c.IsTreasure);
		// if there are treasure cards
		if (treasures.Count() > 0)
		{
			// TODO sjednotit thief choose a thief steal
			// attacker has to pick one
			var card = attacker.User.ThiefChoose(this, attacker.PlayerState, attacker.Game.Kingdom, treasures);
			// the other one is discarded (if there is one)
			cards.Remove(card);

			var otherCard = cards.SingleOrDefault();
			if (otherCard != null)
			{
				attacker.Game.Logger?.Log(new GameLog { PlayerId = defender.Name, Message = $"{defender.Name} discards {otherCard.Name}" });
				defender.PlayerState.DiscardPile.Add(otherCard);
			}

			// attaker chooses if he will trash or steal
			string steal = $"Steal {card.Name}";
			string trash = $"Trash {card.Name}";
			if (attacker.User.ThiefSteal(this, attacker.PlayerState, attacker.Game.Kingdom, card))
			{
				attacker.Game.Logger?.Log(new GameLog { PlayerId = attacker.Name, Message = $"{attacker.Name} steals {card.Name}" });
				attacker.PlayerState.PlayedCards.Add(card);
			}
			else
			{
				attacker.Game.Logger?.Log(new GameLog { PlayerId = defender.Name, Message = $"{defender.Name} trashes {card.Name}" });
				attacker.Game.Trash.Add(card);
			}
		}
		else
		{
			foreach (var card in cards)
			{
				attacker.Game.Logger?.Log(new GameLog { PlayerId = defender.Name, Message = $"{defender.Name} discards {card.Name}" });
				defender.PlayerState.DiscardPile.Add(card);
			}
		}
	}
}
