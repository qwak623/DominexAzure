using AI.Model;
using GameCore;
using GameCore.Cards;
using GameCore.Cards.GeneralCards;

namespace AI.Provincial;
public class ProvincialAI : User
{
	PlayerInfo playerInfo = new PlayerInfo();
	BuyAgenda buyAgenda;
	string name;

	public override string GetName() => name;

	public ProvincialAI(BuyAgenda buyAgenda, string name = nameof(ProvincialAI))
	{
		this.buyAgenda = buyAgenda.Clone();
		// todo vyřešit co se s agendou stane když je null (neměla by tu být vyjimka)
		this.name = name;
	}

	public override Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card attackCard)
	{
		if (phase == Phase.Treasure)
			return cards.FirstOrDefault(c => c.IsTreasure);

		float maxScore = 0;
		Card bestCard = null;
		foreach (var c in cards)
		{
			var neco = Data.GetPriorityList()[(int)c.Type];

			float score = c.Score(cards, ps, phase);
			if (score >= maxScore)
			{
				maxScore = score;
				bestCard = c;
			}
		}

		return bestCard;
	}

	public override Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
	{
		var provinces = k.GetPile(CardType.Province);
		if (buyAgenda.Provinces > provinces.Count && wrapper.GetCard(CardType.Province) != null)
			return Province.Get();

		var duchies = k.GetPile(CardType.Duchy);
		if (buyAgenda.Duchies > provinces.Count && wrapper.GetCard(CardType.Duchy) != null)
			return Duchy.Get();

		var estates = k.GetPile(CardType.Estate);
		if (buyAgenda.Estates > provinces.Count && wrapper.GetCard(CardType.Estate) != null)
			return Estate.Get();

		for (int i = 0; i < buyAgenda.BuyMenu.Count; i++)
		{
			var tuple = buyAgenda.BuyMenu[i];
			if (tuple.Number <= 0)
				continue;

			var card = wrapper.GetCard(tuple.Card);
			if (card == null)
				continue;

			tuple.Number--;
			if (tuple.Number == 0)
				buyAgenda.BuyMenu.RemoveAt(i);
			else
				buyAgenda.BuyMenu[i] = tuple; // this is a value type, i have to return the value back

			if (card.IsTreasure)
				playerInfo.TreasureTotal += card.Coins;
			if (card.Type == CardType.Moneylender)
				playerInfo.TreasureTotal -= 1;
			else if (card.Type == CardType.Bureaucrat)
				playerInfo.TreasureTotal += 2;
			else if (card.Type == CardType.Mine)
				playerInfo.TreasureTotal += 1;

			return card;
		}
		return null;
	}

	#region cards base
	public override Card BureaucratPutOnTop(Card c, PlayerState ps, Kingdom k) => ps.Hand.Where(c => c.IsVictory).First();

	public override List<Card> CellarDiscard(Card c, PlayerState ps, Kingdom k) => ps.Hand.Where(c => c.IsVictory && !c.IsTreasure && !c.IsAction).ToList();

	public override bool ChancellorDiscard(Card c, PlayerState ps, Kingdom k) => false;

	public override List<Card> ChapelTrash(Card c, PlayerState ps, Kingdom k)
	{
		var cards = ps.Hand;

		// allways trash curse
		var trash = cards.Where(c => c.Type == CardType.Curse);

		var neco = trash.ToString();

		// in the beginnig trash estate as well
		var provinces = k.GetPile(CardType.Province);
		if (buyAgenda.Estates <= provinces.Count)
			trash = trash.Concat(cards.Where(c => c.Type == CardType.Estate));

		if (trash.Count() >= 4)
			return trash.Take(4).ToList();

		// trash only unnecesary coppers
		int coins = cards.Select(c => c.Coins).Sum() + ps.Coins;
		var card = SelectCardToGain(k.GetWrapper(coins), ps, k, Phase.Buy);
		int price = card == null ? 0 : card.Price;

		if (playerInfo.TreasureTotal > 3)
		{
			var coppers = cards.Where(c => c.Type == CardType.Copper).Take(coins - price);
			trash = trash.Concat(coppers);
			// player info update
			playerInfo.TreasureTotal -= coppers.Count();
		}

		return trash.Take(4).ToList();
	}

	public override bool LibrarySkip(Card c, PlayerState ps, Kingdom k, Card card)
	{
		if (ps.Actions == 0)
			return true;
		if (ps.Actions == 1)
		{
			if (card.AddActions > 0)
				return false;
			if (ps.Hand.Any(c => c.AddActions > 1))
				return false;
			if (ps.Hand.Any(c => c.AddActions == 0))
				return true;
			return false;
		}
		return false;
	}

	public override List<Card> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount)
	{
		var hand = ps.Hand.ToList();
		var discards = new List<Card>();
		while (discards.Count < discardCount)
		{
			// first choice is random victory card
			var card = hand.FirstOrDefault(c => c.IsVictory);

			// kdyz nemam victory tak vyberu nejzbytecnejsi kartu
			if (card == null)
				card = (from c in hand
						let m = hand.Min(a => a.Score(hand, ps, Phase.Attack))
						where m == c.Score(hand, ps, Phase.Attack)
						select c).FirstOrDefault();

			discards.Add(card);
			hand.Remove(card);
		}

		return discards;
	}

	public override Card MineTrash(Card card, PlayerState ps, Kingdom k, IList<Card> cardSelection)
	{
		var c = ps.Hand.Where(a => a.Type == CardType.Copper).FirstOrDefault();
		return c ?? ps.Hand.Where(a => a.Type == CardType.Silver).FirstOrDefault();
	}

	public override bool MoatDefend(Card c, PlayerState ps, Kingdom k) => true;

	public override bool MoneylenderTrash(Card cardPlayed, PlayerState ps, Kingdom k) => true;

	public override Card RemodelTrash(Card c, PlayerState ps, Kingdom k)
	{
		var trash = ps.Hand.Where(c => c.Type == CardType.Curse);

		// at the end it will transform gold to province
		var provinces = k.GetPile(CardType.Province);
		if (buyAgenda.Provinces > provinces.Count && provinces.Count > 0)
			trash = trash.Concat(ps.Hand.Where(c => c.Type == CardType.Gold));

		if (buyAgenda.Estates <= provinces.Count)
			trash = trash.Concat(ps.Hand.Where(c => c.Type == CardType.Estate));

		trash = trash.Concat(ps.Hand.Where(c => c.Type == CardType.Copper));
		trash = trash.Concat(ps.Hand.OrderBy(c => c.Score(ps.Hand, ps, Phase.Action)));

		return trash.FirstOrDefault();
	}

	public override bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, Card c, Phase p)
	{
		if (p == Phase.Attack)
			return !(c.IsVictory || c.Type == CardType.Copper);
		else // if (p == Phase.Action)
			return (c.IsVictory || c.Type == CardType.Copper);
	}

	public override Card ThiefChoose(Card c, PlayerState ps, Kingdom k, IEnumerable<Card> cards) => cards.OrderByDescending(c => c.Price).First();

	public override bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, Card c) => c.Price >= 3;

	public override Card ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<Card> cards)
	{
		return (from c in cards
				let m = cards.Max(a => a.Score(ps.Hand, ps, Phase.Action))
				where m == c.Score(ps.Hand, ps, Phase.Action)
				select c).FirstOrDefault();
	}
	#endregion
}

