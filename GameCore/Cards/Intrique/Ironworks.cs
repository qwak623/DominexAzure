namespace GameCore.Cards.Intrique;
public class Ironworks : Card
{
	private static Ironworks ironworks;
	private Ironworks() : base(CardType.Action)
	{
		Name = CardName.Ironworks;
		DefaultPrice = 4;
		ironworks = this;
		Description = $"Gain a card costing up to $4. If the gained card is an...{Environment.NewLine}Action card, +1 Action{Environment.NewLine}Treasure card, +$1{Environment.NewLine}Victory card, +1 Card";
	}

	public static Ironworks Get() => ironworks ?? new Ironworks();

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		var availableCards = p.Game.Kingdom.GetWrapper(p.PlayerState, 4).AvailableCards.ToList();
		var card = p.User.SelectCardToGain(this, p.PlayerState, p.Game.Kingdom, availableCards);
		if (card is null)
		{
			return;
		}
		p.Gain(card);
		if (card.Card.IsAction)
		{
			p.PlayerState.Actions++;
		}
		if (card.Card.IsTreasure)
		{
			p.PlayerState.Coins++;
		}
		if (card.Card.IsVictory)
		{
			p.Draw(1);
		}
	}
}
