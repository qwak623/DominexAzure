using GameCore.Cards;
using GameCore.Observers;

namespace GameCore;

/// <summary>
/// Whatever what window needs to show (but it is not important for functionality).
/// </summary>
public class PlayerState
{
	public string Name { get; private set; }
	private readonly IPlayerStateObserver playerStateObserver;

	public List<Card> DrawPile = new(); // todo asi by to měly být pily
	public List<Card> DiscardPile = new();
	public List<Card> Hand = new();
	public List<Card> PlayedCards = new(10);

	private int actions;
	public int Actions
	{
		get { return actions; }
		set
		{
			actions = value;
			playerStateObserver?.Notify(this);
		}
	}

	private int buys;
	public int Buys
	{
		get { return buys; }
		set
		{
			buys = value;
			playerStateObserver?.Notify(this);
		}
	}

	private int coins;
	public int Coins
	{
		get { return coins; }
		set
		{
			coins = value;
			playerStateObserver?.Notify(this);
		}
	}

	public PlayerState(IPlayerStateObserver playerStateObserver, string name)
	{
		this.playerStateObserver = playerStateObserver;
		Name = name;
	}

}
