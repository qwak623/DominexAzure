namespace GameCore.Cards.Base;
public class Cellar : Card
{
	private static Cellar cellar;
	private Cellar() : base(CardType.Action)
	{
		Name = CardName.Cellar;
		DefaultPrice = 2;
		AddActions = 1;
		cellar = this;
		Description = $"Discard any number of cards.{Environment.NewLine}+1 Card per card discarded.";
		Message = "Discard any number of cards, then draw that many.";
	}

	public static Cellar Get() => cellar ?? new Cellar();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var selectedCards = player.User.CellarDiscard(
			this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList());
		selectedCards.ForEach(player.Discard);
		player.Draw(selectedCards.Count);
	}
}

