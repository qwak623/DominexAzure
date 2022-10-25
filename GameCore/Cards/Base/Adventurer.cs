using System.Linq;

namespace GameCore.Cards.Base;

public class Adventurer : Card
{
	private static Adventurer adventurer;
	private Adventurer() : base
	(
		name: "Adventurer",
		type: CardType.Adventurer,
		price: 6,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	) => adventurer = this;

	public static Adventurer Get() => adventurer ?? new Adventurer();

	protected override void ActionEffect(Player player)
	{
		for (int i = 0; i < 2;)
		{
			var card = player.Show(1).SingleOrDefault();
			if (card == null)
			{
				break;
			}
			if (card.IsTreasure)
			{
				player.Game.Logger?.Log($"{Name} draws {card.Name}");
				player.ps.Hand.Add(card);
				i++;
			}
			else
			{
				player.ps.PlayedCards.Add(card);
			}
		}
	}
}
