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

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var cardsAside = new Pile();

		while (player.PlayerState.Hand.Count < 7)
		{
			var cardsShown = player.Show(1);
			if (cardsShown.Count == 0)
			{
				break;
			}
			var card = cardsShown.Single();

			if (card.IsAction && player.User.LibrarySkip(this, player.PlayerState, player.Game.Kingdom, card))
			{
				cardsAside.Move(card);
			}
			else
			{
				player.Game.Logger?.Log(new GameLog { PlayerId = Name, Message = $"{Name} draws {card.Name}" });
				player.PlayerState.Hand.Move(card);
			}
		}

		player.PlayerState.DiscardPile.MoveAll(cardsAside);
	}
}
