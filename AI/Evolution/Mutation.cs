using AI.Model;
using GameCore.Cards;
using Utils;

namespace AI.Evolution;

// this class is not thread safe
internal abstract class Mutation
{
	/// <summary>
	/// Mutation changes agenda in parameter based on kingdom kards and it does not create new buyAgenda.
	/// </summary>
	/// <param name="agenda"></param>
	/// <param name="kingdom"></param>
	public abstract void Mutate(BuyAgenda agenda, List<Card> kingdom);
}

internal class ReplaceSupplyCardMutation : Mutation
{
	public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
	{
		if (agenda.BuyMenu.Count == 0)
		{
			return;
		}

		int i = ThreadSafeRandom.Next(agenda.BuyMenu.Count);
		int j = ThreadSafeRandom.Next(kingdom.Count);

		var tuple = agenda.BuyMenu[i];
		tuple.Card = kingdom[j].Name;
		agenda.BuyMenu[i] = tuple;
	}
}

internal class ModifyPurchaseCountMutation : Mutation
{
	public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
	{
		if (agenda.BuyMenu.Count == 0)
		{
			return;
		}

		int i = ThreadSafeRandom.Next(agenda.BuyMenu.Count);

		var tuple = agenda.BuyMenu[i];

		tuple.Number += ThreadSafeRandom.NextSign();

		// if number = 0 card is never bought anyway
		if (tuple.Number == 0)
		{
			agenda.BuyMenu.RemoveAt(i);
			return;
		}

		agenda.BuyMenu[i] = tuple;
	}
}

internal class SwapSupplyCardsMutation : Mutation
{
	public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
	{
		if (agenda.BuyMenu.Count == 0)
		{
			return;
		}

		int i = ThreadSafeRandom.Next(agenda.BuyMenu.Count);
		int j = ThreadSafeRandom.Next(agenda.BuyMenu.Count);

		var tuple = agenda.BuyMenu[i];
		agenda.BuyMenu[i] = agenda.BuyMenu[j];
		agenda.BuyMenu[j] = tuple;
	}
}

internal class VictoryCardPurchaseMutation : Mutation
{
	public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
	{
		int i = ThreadSafeRandom.Next(3);

		switch (i)
		{
			case 0:
				agenda.Estates += ThreadSafeRandom.NextSign();
				break;
			case 1:
				agenda.Duchies += ThreadSafeRandom.NextSign();
				break;
			case 2:
				agenda.Provinces += ThreadSafeRandom.NextSign();
				break;
			default:
				break;
		}
	}
}

internal class AddCardMutation : Mutation
{
	public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
	{
		if (agenda.BuyMenu.Count == 0)
		{
			return;
		}

		int i = ThreadSafeRandom.Next(agenda.BuyMenu.Count);
		int j = ThreadSafeRandom.Next(kingdom.Count);

		agenda.BuyMenu.Insert(i, (kingdom[j].Name, ThreadSafeRandom.Next(9) + 1));
	}
}

internal class RemoveCardMutation : Mutation
{
	public override void Mutate(BuyAgenda agenda, List<Card> kingdom)
	{
		if (agenda.BuyMenu.Count <= 1)
		{
			return;
		}

		int i = ThreadSafeRandom.Next(agenda.BuyMenu.Count);

		agenda.BuyMenu.RemoveAt(i);
	}
}
