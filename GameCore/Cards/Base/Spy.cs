using GameCore.GameCore;

namespace GameCore.Cards.Base;
public class Spy : Card
{
	private static Spy spy;
	private Spy() : base([CardType.Action, CardType.Attack])
	{
		Name = CardName.Spy;
		DefaultPrice = 4;
		AddActions = 1;
		DrawCards = 1;
		spy = this;
		Description = "Each player (including you) reveals the top card of his deck and either discards it or puts it back, your choice.";
		Message = "You may discard card at top of the draw pile.";
	}

	public static Spy Get() => spy ?? new Spy();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var cardsShown = player.Show(1);
		if (cardsShown.Count == 0)
		{
			return;
		}

		var card = cardsShown.Single();

		if (player.User.SpyDiscard(this, player.PlayerState, player.Game.Kingdom, card, Phase.Action))
		{
			player.Game.Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} discards {card.Name}" });
			player.Discard(card);
		}
		else
		{
			player.PlayerState.DrawPile.Move(card);
		}
		TriggerAttacks(player);
	}

	public override void Attack(IPlayer defender, IPlayer attacker)
	{
		var cardsShown = defender.Show(1);
		if (cardsShown.Count == 0)
		{
			return;
		}

		var card = cardsShown.Single();

		// TODO není poznat, komu odhazujeme kartu
		if (attacker.User.SpyDiscard(this, attacker.PlayerState, attacker.Game.Kingdom, card, Phase.Attack))
		{
			defender.Game.Logger?.Log(new GameLog { PlayerId = defender.Name, Message = $"{defender.Name} discards {card.Name}" });
			defender.Discard(card);
		}
		else
		{
			defender.PlayerState.DrawPile.Move(card);
		}
	}
}
