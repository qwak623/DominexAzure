using GameCore.Cards;
using GameCore.GameCore;

namespace GameCore;
public class Player : IPlayer
{
	private readonly string name;
	public string Name => name;

	public IUser User { get; private set; } // todo proc ukazuju tohle verejne? nejspis by tohle nemelo byt tady vubec

	public IGame Game { get; private set; }

	private readonly PlayerState ps;
	public PlayerState PlayerState => ps;

	// todo tohle se mi moc nelibi, na konci hry by bylo nejlepsi udelat jeden balicek a ten prochazet
	public int CardCount => ps.DrawPile.Count + ps.DiscardPile.Count + ps.Hand.Count + ps.CardsPlayed.Count;

	private int? victoryPoints;

	/// <summary>
	/// Returns earned victory points. Working properly only at the end of the game.
	/// </summary>
	public int VictoryPoints
	{
		get
		{   // points can be counted only at the end of the game
			if (Game.GameEnd && victoryPoints is null)
			{
				// it will be better to have all cards in discard kingdomPile before counting
				Cleanup();
				PlayerState.DiscardPile.MoveAll(PlayerState.DrawPile);
				victoryPoints = ps.DiscardPile.Select(c => c.Card.CountPoints(this)).Sum();
			}
			return victoryPoints.GetValueOrDefault();
		}
	}

	// todo je treba abstrahovat nahodnost
	public Player(IGame game, IUser user)
	{
		name = user.GetName();
		Game = game;
		User = user;

		ps = new PlayerState(user.GetPlayerStateObserver(), name);

		// todo tohle by mohlo být parametrizovatelné
		InitiatePiles();
	}

	public void PlayTurn(int drawCount)
	{
		#region action phase
		Game.Logger?.Log(new GameLog { PlayerId = Name, Message = $"{Name}'s turn:" });
		Game.Logger?.Log(new GameLog { Message = $"Action phase" });
		Game.Logger?.Log(new GameLog
		{
			PlayerId = Name,
			Message =
			$"Hand: {string.Join(", ", PlayerState.Hand.Select(c => c.Name))}"
		});

		// todo tohle neni moc hezke
		PlayerState.Buys = 1;
		PlayerState.Actions = 1;
		PlayerState.Coins = 0;

		CardInstance card;
		do
		{
			card = PlayActionCard();
		}
		while (card is not null);
		#endregion

		#region buy phase
		// treasure phase
		PlayTreasure();

		// buy phase
		Game.Logger?.Log(new GameLog { Message = $"Buy phase" });
		Game.Logger?.Log(new GameLog { PlayerId = Name, Message = "Hand: " + string.Join(", ", PlayerState.Hand.Select(c => c.Name)) });
		Game.Logger?.Log(new GameLog { PlayerId = Name, Message = $"{Name} has ${PlayerState.Coins}." });

		do
		{
			card = Buy();
		}
		while (card is not null);
		#endregion

		#region cleanup
		Cleanup();
		#endregion

		#region draw phase
		Draw(drawCount);
		#endregion
	}

	/// <summary>
	///     Null pointer indicates that player cand or doesnt want to play any action card.
	///     Player asks user on every decision (card selection, card related decision etc.)
	///     when play actions and attack acitons are executed here.
	/// </summary>
	/// <returns>The played card.</returns>
	public CardInstance PlayActionCard()
	{
		// if player has no actions left or he doesnt have any action cards, he cant select an action card
		if (ps.Actions == 0 || ps.Hand.All(c => !c.IsAction))
		{
			return null;
		}

		// user selects card to play, card is removed from hand and added to played cards
		var card = User.PlayCard(ps.Hand.Where(c => c.IsAction), ps, Game.Kingdom, Phase.Action);
		if (card is null)
		{
			return null;
		}

		PlayActionCardInternal(card);
		return card;
	}

	/// <summary>
	///		Plays the given card, substract one from number of actions,
	///		puts the card from hand to the played cards and deals an attack in case of an attack card.
	/// </summary>
	/// <param name="card">Card to play.</param>
	internal void PlayActionCardInternal(CardInstance cardInstance)
	{
		Game.Logger?.Log(new GameLog { PlayerId = Name, Message = $"{Name} plays {cardInstance.Card.Name}." });

		ps.CardsPlayed.Move(cardInstance);
		ps.ActionsPlayed.Add(cardInstance.Card);
		ps.Actions--;

		cardInstance.WhenPlayAction(this);

		if (cardInstance.Card.IsAttack)
		{
			foreach (var player in Game.Players.Where(p => p != this))
			{
				player.DealAttack(this, cardInstance.Card);
			}
		}
	}

	/// <summary>
	/// All treasure cards are automatically played for now. 
	/// </summary>
	public void PlayTreasure()
	{
		foreach (var card in ps.Hand.Select(c => c.Card).Where(c => c.IsTreasure))
		{
			card.WhenPlayTreasure(this);
		}
	}

	/// <summary>
	///     Null means end of buy phase.
	///     Allowed buys player counts by himself.
	/// </summary>
	/// <returns>Purchased card.</returns>
	public CardInstance Buy()
	{
		if (ps.Buys == 0)
		{
			return null;
		}

		// buy
		var card = User.SelectCardToGain(Game.Kingdom.GetWrapper(ps.Coins), ps, Game.Kingdom, Phase.Buy);
		if (card is null)
		{
			return null;
		}

		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} pays ${card.Card.Price}." });

		Gain(card);
		ps.Buys--;
		ps.Coins -= card.Price;

		return card;
	}

	/// <summary>
	///     Hand and all played and purchased cards are placed to discard kingdomPile.
	/// </summary>
	public void Cleanup()
	{
		ps.DiscardPile.MoveAll(ps.Hand);
		ps.DiscardPile.MoveAll(ps.CardsPlayed);
	}

	/// <summary>
	///     Draws cards from draw kingdomPile to hand. If there are no cards 
	///     shuffles the discard kingdomPile and places it onto draw kingdomPile place.
	///     Then draws cards.
	/// </summary>
	/// <param name="count">Count of cards to draw.</param>
	public void Draw(int count)
	{
		for (; count > 0; count--)
		{
			ShuffleIfNeeded();

			// there are no cards to draw
			if (ps.DrawPile.Count == 0)
			{
				Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} doesn't have any cards left to draw." });
				return;
			}

			// draw one card
			var card = ps.DrawPile[^1];
			Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} draws {card.Name}" });
			ps.Hand.Move(card);
		}
	}

	/// <summary>
	/// Takes specified card from hand and adds it to common trash list.
	/// </summary>
	/// <param name="card"></param>
	public void Trash(CardInstance card)
	{
		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} trashes {card.Name}." });
		Game.Trash.Move(card);
	}

	/// <summary>
	/// Takes specified card from hand and adds it to discard kingdomPile.
	/// </summary>
	/// <param name="card"></param>
	public void Discard(CardInstance card)
	{
		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} discards {card.Name}." });
		ps.DiscardPile.Move(card);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="card"></param>
	public void Gain(CardInstance card)
	{
		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} gains {card.Name}." });
		// TODO hook on gain event
		ps.DiscardPile.Move(card);
	}

	/// <summary>
	/// Gains card to the discard pile if possible.
	/// </summary>
	/// <param name="type">Type of card to gain</param>
	public void Gain(CardType type)
	{
		var card = GetCardToGain(type);
		if (card is null)
		{
			return;
		}

		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} gains {card.Name}." });
		ps.DiscardPile.Move(card);
	}

	/// <summary>
	/// Gains card to the hand if possible.
	/// </summary>
	/// <param name="type">Type of card to gain</param>
	public void GainToHand(CardType type)
	{
		var card = GetCardToGain(type);
		if (card is null)
		{
			return;
		}

		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} gains {card.Name} to hand." });
		ps.Hand.Move(card);
	}

	public void GainToHand(CardInstance card)
	{
		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} gains {card.Name} to hand." });
		// TODO hook on gain event
		ps.Hand.Move(card);
	}

	/// <summary>
	/// Gains card to the draw pile if possible.
	/// </summary>
	/// <param name="type">Type of card to gain.</param>
	public void GainToDrawPile(CardType type)
	{
		var card = GetCardToGain(type);
		if (card is null)
		{
			return;
		}

		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} gains {card.Name} to the draw pile." });
		ps.DrawPile.Move(card);
	}

	/// <summary>
	/// Returns card from hand to the draw pile.
	/// </summary>
	/// <param name="card">Card to return to the draw pile.</param>
	public void ReturnToDrawPile(CardInstance card)
	{
		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} returns {card.Name} to the draw pile." });
		ps.DrawPile.Move(card);
	}

	/// <summary>
	/// Shows cards on top of the draw pile and returns them.
	/// </summary>
	/// <param name="count">Count of cards to show.</param>
	/// <returns>List of the shown cards that were removed from the draw kingdomPile.</returns>
	public Pile Show(int count)
	{
		var shownCards = new Pile();

		for (; count > 0; count--)
		{
			ShuffleIfNeeded();

			// no cards to show
			if (ps.DrawPile.Count == 0)
			{
				Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} doesn't have any cards left to show." });
				return shownCards;
			}

			// show one card
			var card = ps.DrawPile[^1];
			Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} shows {card.Name}" });
			shownCards.Move(card);
		}
		return shownCards;
	}

	/// <summary>
	/// If player deals attack, defender can reveal reaction cards.
	/// If one of revealed cards is Moat, attack effect won't be executed.
	/// </summary>
	/// <param name="attacker">Player who deals the attack</param>
	/// <param name="attackCard">Card by which the attack is dealt</param>
	public void DealAttack(IPlayer attacker, Card attackCard)
	{
		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} deals attack." });

		// before attack is executed defender can select some reaction cards.
		CardInstance card = null;
		bool defended = false;
		var reactions = new List<CardInstance>(ps.Hand.Where(c => c.IsReaction));

		// TODO tenhle while se mi nelíbí
		while (reactions.Count > 0)
		{
			card = User.PlayCard(reactions, ps, Game.Kingdom, Phase.Reaction, attackCard);
			if (card is null)
			{
				break;
			}
			else
			{
				reactions.Remove(card);
			}

			defended |= card.Card.Reaction(this);
		}

		if (!defended)
		{
			attackCard.Attack(this, attacker);
		}
	}

	public void Notify() => ps.Notify();
	public override string ToString() => name;

	/// <summary>
	/// Returns card from the kingdom if possible.
	/// </summary>
	/// <param name="type">Type of card to gain</param>
	/// <returns>Card of the given type from the kingdom</returns>
	private CardInstance GetCardToGain(CardType type)
	{
		var kingdomPile = Game.Kingdom.GetPile(type);
		return kingdomPile.CardInstance;
	}

	private void InitiatePiles()
	{
		// TODO 5 2 mód, teď je to takhle divně kvůli 4 3 módu
		// Adds 3 estates and 7 coppers to the draw pile
		for (int i = 0; i < 2; i++)
		{
			GainToDrawPile(Cards.GeneralCards.Estate.Get().Type);
		}
		for (int i = 0; i < 7; i++)
		{
			GainToDrawPile(Cards.GeneralCards.Copper.Get().Type);
		}
		for (int i = 0; i < 1; i++)
		{
			GainToDrawPile(Cards.GeneralCards.Estate.Get().Type);
		}
	}

	/// <summary>
	/// Shuffles the discard kingdomPile and puts it on the draw kingdomPile, if the draw kingdomPile is empty.
	/// </summary>
	private void ShuffleIfNeeded()
	{
		if (ps.DrawPile.Count == 0 && ps.DiscardPile.Count > 0)
		{
			Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} shuffles the pile." });

			// shuffle
			ps.DiscardPile.Shuffle();

			// swap
			(ps.DiscardPile, ps.DrawPile) = (ps.DrawPile, ps.DiscardPile);
		}
	}
}
