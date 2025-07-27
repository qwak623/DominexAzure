using GameCore.Cards;
using Utils;

namespace AI.Model;

public class BuyAgenda
{
	public static char sep = Path.DirectorySeparatorChar;
	public static string DirectoryPath = $"..{sep}..{sep}..{sep}qwak623{sep}DominexAzure{sep}AI{sep}Provincial{sep}data{sep}kingdoms{sep}";

	private BuyAgenda(string id) => Id = id;

	public string Id { get; set; }
	public List<(CardType Card, int Number)> BuyMenu { get; set; } = new List<(CardType, int)>();
	public int Provinces { get; set; }
	public int Duchies { get; set; }
	public int Estates { get; set; }
	public bool Loaded { get; set; }


	public static BuyAgenda FromString(string str)
	{
		var line = str.Split(':');

		try
		{
			var agendaArray = line[1].Split(';');

			var agenda = new BuyAgenda(line[0]);
			agenda.Loaded = true;
			agenda.Provinces = int.Parse(agendaArray[0]);
			agenda.Duchies = int.Parse(agendaArray[1]);
			agenda.Estates = int.Parse(agendaArray[2]);

			foreach (var item in agendaArray[3].Split(',').Select(i => i.Split()))
			{
				Enum.TryParse(item[0], out CardType type);
				agenda.BuyMenu.Add((type, int.Parse(item[2])));
			}

			return agenda;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public string ToString(string id)
	{
		var buyMenuString = BuyMenu.Select(b => $"{b.Card} ({(int)b.Card}) {b.Number}").Aggregate((a, b) => a + "," + b);
		return $"{id}:{Provinces};{Duchies};{Estates};{buyMenuString}";
	}

	public BuyAgenda Clone()
	{
		return new BuyAgenda(Id)
		{
			Provinces = this.Provinces,
			Duchies = this.Duchies,
			Estates = this.Estates,
			BuyMenu = this.BuyMenu.ToList()
		};
	}


	/// <summary>
	/// Creates random individual with length max 10. on index 2 is (gold, 99) and on index 7 is (silver, 10).
	/// </summary>
	/// <param name="k"></param>
	/// <returns></returns>
	public static BuyAgenda CreateRandom(List<Card> k)
	{
		string id = k.Where(c => c.Type > CardType.Curse).OrderBy(c => c.Type).Select(c => ((int)c.Type).ToString()).Aggregate((a, b) => a + "_" + b);
		var agenda = new BuyAgenda(id);

		var randomKingdom = k.Where(c => c.Type != CardType.Curse || c.Type != CardType.Copper)
			.OrderBy(c => ThreadSafeRandom.Next())
			.Take(9)
			.Select(c => (c.Type, ThreadSafeRandom.Next(10) + 1));

		int i = 0;
		foreach (var item in randomKingdom)
		{
			agenda.BuyMenu.Add(item);
			if (i == 2)
				agenda.BuyMenu.Add((CardType.Gold, 99));
			if (i == 7)
				agenda.BuyMenu.Add((CardType.Silver, 10));
			i++;
		}

		agenda.Provinces = ThreadSafeRandom.Next(8);
		agenda.Duchies = ThreadSafeRandom.Next(8);
		agenda.Estates = ThreadSafeRandom.Next(8);

		return agenda;
	}
}