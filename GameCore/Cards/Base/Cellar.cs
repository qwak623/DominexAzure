namespace GameCore.Cards.Base;
public class Cellar : Card
{
	private static Cellar cellar;
	private Cellar() : base
	(
		name: "Cellar",
		type: CardType.Cellar,
		price: 2,
		addActions: 1,
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
		cellar = this;
		Description = $"Discard any number of cards.{Environment.NewLine}+1 Card per card discarded.";
		Message = "Discard any number of cards, then draw that many.";
	}

	public static Cellar Get() => cellar ?? new Cellar();

	protected override void ActionEffect(IPlayer player)
	{
		// todo - lze discardnout cellar?
		var selectedCards = player.User.CellarDiscard(this, player.PlayerState, player.Game.Kingdom);

		selectedCards.ForEach(player.Discard);
		player.Draw(selectedCards.Count);
	}
}

