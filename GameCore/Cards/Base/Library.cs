using GameCore.GameCore;

namespace GameCore.Cards.Base;
public class Library : Card
{
	private static Library library;
	private Library() : base
	(
		name: "Library",
		type: CardType.Library,
		price: 5,
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
		library = this;
		Description = "Draw until you have 7 cards in hand, skipping any Action cards you choose to; set those aside, discarding them afterwards.";
		Message = "You may skip any action card you choose to.";
	}

	public static Library Get() => library ?? new Library();

	protected override void ActionEffect(Player player)
	{
		var cardsAside = new List<Card>();

		while (player.ps.Hand.Count < 7)
		{
			var card = player.Show(1).SingleOrDefault();
			if (card == null)
			{
				break;
			}

			if (card.IsAction && player.User.LibrarySkip(player.ps, player.Game.Kingdom, card))
			{
				cardsAside.Add(card);
			}
			else
			{
				player.Game.Logger?.Log(new GameLog { PlayerId = Name, Message = $"{Name} draws {card.Name}" });
				player.ps.Hand.Add(card);
			}
		}

		cardsAside.ForEach(c => player.ps.DiscardPile.Add(c));
	}
}
