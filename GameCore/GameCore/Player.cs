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
	public int CardCount => ps.DrawPile.Count + ps.DiscardPile.Count + ps.Hand.Count + ps.PlayedCards.Count;

	private int? victoryPoints;

	/// <summary>
	/// Returns earned victory points. Working properly only at the end of the game.
	/// </summary>
	public int VictoryPoints
	{
		get
		{   // points can be counted only at the end of the game
			if (Game.GameEnd && victoryPoints == null)
			{
				// it will be better to have all cards in discard pile before counting
				Cleanup();
				DiscardDrawPile();
				victoryPoints = ps.DiscardPile.Select(c => c.CountPoints(this)).Sum();
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

		Card card;
		do
		{
			card = PlayActionCard();
		}
		while (card != null);
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
		while (card != null);
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
	public Card PlayActionCard()
	{
		// if player has no actions left or he doesnt have any action cards, he cant select an action card
		if (ps.Actions == 0 || ps.Hand.All(c => !c.IsAction))
		{
			return null;
		}

		// user selects card to play, card is removed from hand and added to played cards
		var card = User.PlayCard(ps.Hand.Where(c => c.IsAction), ps, Game.Kingdom, Phase.Action);
		if (card == null)
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
	internal void PlayActionCardInternal(Card card)
	{
		Game.Logger?.Log(new GameLog { PlayerId = Name, Message = $"{Name} plays {card.Name}." });

		ps.Hand.Remove(card);
		ps.PlayedCards.Add(card);
		ps.Actions--;

		card.WhenPlayAction(this);

		if (card.IsAttack)
		{
			foreach (var player in Game.Players.Where(p => p != this))
			{
				player.DealAttack(this, card);
			}
		}
	}

	/// <summary>
	/// All treasure cards are automatically played for now. 
	/// </summary>
	public void PlayTreasure()
	{
		foreach (var card in ps.Hand.Where(c => c.IsTreasure))
		{
			card.WhenPlayTreasure(this);
		}
	}

	/// <summary>
	///     Null means end of buy phase.
	///     Allowed buys player counts by himself.
	/// </summary>
	/// <returns>Purchased card.</returns>
	public Card Buy()
	{
		if (ps.Buys == 0)
		{
			return null;
		}

		// buy
		var card = User.SelectCardToGain(Game.Kingdom.GetWrapper(ps.Coins), ps, Game.Kingdom, Phase.Buy);
		if (card == null)
		{
			return null;
		}

		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} pays ${card.Price}." });

		Gain(card.Type);
		ps.Buys--;
		ps.Coins -= card.Price;

		return card;
	}

	/// <summary>
	///     Hand and all played and purchased cards are placed to discard pile.
	/// </summary>
	public void Cleanup()
	{
		ps.Hand.ForEach(card => ps.DiscardPile.Add(card));
		ps.Hand.Clear();
		ps.PlayedCards.ForEach(card => ps.DiscardPile.Add(card));
		ps.PlayedCards.Clear();
	}

	/// <summary>
	///     Draws cards from draw pile to hand. If there are no cards 
	///     shuffles the discard pile and places it onto draw pile place.
	///     Then draws cards.
	/// </summary>
	/// <param name="count">Count of cards to draw.</param>
	public void Draw(int count)
	{
		for (; count > 0; count--)
		{
			ShuffleIfNeeded();

			// there are no cards to draw
			if (!ps.DrawPile.Any())
			{
				Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} doesn't have any cards left to draw." });
				return;
			}

			// draw one card
			var card = ps.DrawPile[^1];
			Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} draws {card.Name}" });
			ps.Hand.Add(card);
			ps.DrawPile.RemoveAt(ps.DrawPile.Count - 1);
		}
	}

	/// <summary>
	/// Takes specified card from hand and adds it to common trash list.
	/// </summary>
	/// <param name="card"></param>
	public void Trash(Card card)
	{
		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} trashes {card.Name}." });
		ps.Hand.Remove(card);
		Game.Trash.Add(card);
	}

	/// <summary>
	/// Takes specified card from hand and adds it to discard pile.
	/// </summary>
	/// <param name="card"></param>
	public void Discard(Card card)
	{
		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} discards {card.Name}." });
		ps.Hand.Remove(card);
		ps.DiscardPile.Add(card);
	}

	/// <summary>
	/// Gains card to the discard pile if possible.
	/// </summary>
	/// <param name="type">Type of card to gain</param>
	public void Gain(CardType type)
	{
		var card = GainCard(type);
		if (card == null)
		{
			return;
		}

		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} gains {card.Name}." });
		ps.DiscardPile.Add(card);
	}

	/// <summary>
	/// Gains card to the hand if possible.
	/// </summary>
	/// <param name="type">Type of card to gain</param>
	public void GainToHand(CardType type)
	{
		var card = GainCard(type);
		if (card == null)
		{
			return;
		}

		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} gains {card.Name} to hand." });
		ps.Hand.Add(card);
	}

	/// <summary>
	/// Gains card to the draw pile if possible.
	/// </summary>
	/// <param name="type">Type of card to gain</param>
	public void GainToDrawPile(CardType type)
	{
		var card = GainCard(type);
		if (card == null)
		{
			return;
		}

		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} gains {card.Name} to the draw pile." });
		ps.DrawPile.Add(card);
	}


	/// <summary>
	/// Returns card from hand to the draw pile.
	/// </summary>
	/// <param name="card">Card to return to the draw pile.</param>
	public void ReturnToDrawPile(Card card)
	{
		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} returns {card.Name} to the draw pile." });
		ps.Hand.Remove(card);
		ps.DrawPile.Add(card);
	}

	/// <summary>
	/// Discards all cards in draw pile.
	/// </summary>
	public void DiscardDrawPile()
	{
		Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} discards draw pile." });
		ps.DiscardPile.AddRange(ps.DrawPile);
		ps.DrawPile.Clear();
	}

	/// <summary>
	/// Shows cards on top of the draw pile and returns them.
	/// </summary>
	/// <param name="count">Count of cards to show.</param>
	/// <returns>List of the shown cards that were removed from the draw pile.</returns>
	public List<Card> Show(int count)
	{
		var list = new List<Card>(count);

		for (; count > 0; count--)
		{
			ShuffleIfNeeded();

			// no cards to draw
			if (!ps.DrawPile.Any())
			{
				Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} doesn't have any cards left to show." });
				return list;
			}

			// draw one card
			var card = ps.DrawPile[^1];
			ps.DrawPile.RemoveAt(ps.DrawPile.Count - 1);
			Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} shows {card.Name}" });
			list.Add(card);
		}
		return list;
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
		Card card = null;
		bool defended = false;
		var reactions = new LinkedList<Card>(ps.Hand.Where(c => c.IsReaction));

		// TODO tenhle while se mi nelíbí
		while (reactions.Count > 0)
		{
			card = User.PlayCard(reactions, ps, Game.Kingdom, Phase.Reaction, attackCard);
			reactions.Remove(card);
			if (card == null)
			{
				break;
			}

			defended |= card.Reaction(this);
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
	private Card GainCard(CardType type)
	{
		var pile = Game.Kingdom.GetPile(type);
		var card = pile.GainCard();

		// counts empty piles without enumerating 
		if (pile.Empty)
		{
			Game.Kingdom.EmptyPilesCount++;
		}

		return card;
	}

	private void InitiatePiles()
	{
		// TODO 5 2 mód, teď je to takhle divně kvůli 4 3 módu
		// TODO vzít ty karty z kingdomu, ne vykouzlit
		// Adds 3 estates and 7 coppers to the draw pile
		ps.DrawPile.AddRange(Enumerable.Repeat(Cards.GeneralCards.Estate.Get(), 2));
		ps.DrawPile.AddRange(Enumerable.Repeat(Cards.GeneralCards.Copper.Get(), 7));
		ps.DrawPile.AddRange(Enumerable.Repeat(Cards.GeneralCards.Estate.Get(), 1));
	}

	/// <summary>
	/// Shuffles the discard pile and puts it on the draw pile, if the draw pile is empty.
	/// </summary>
	private void ShuffleIfNeeded()
	{
		if (!ps.DrawPile.Any() && ps.DiscardPile.Any())
		{
			Game.Logger?.Log(new GameLog { PlayerId = name, Message = $"{name} shuffles the pile." });

			// shuffle
			ps.DiscardPile.Shuffle();

			// swap
			(ps.DiscardPile, ps.DrawPile) = (ps.DrawPile, ps.DiscardPile);
		}
	}
}
