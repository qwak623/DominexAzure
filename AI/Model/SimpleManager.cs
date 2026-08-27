using GameCore;
using GameCore.Cards;
using GameCore.Observers;
using Utils;

namespace AI.Model;
public class SimpleManager : BuyAgendaManager
{
	private string directoryPath;
	private string prefix;
	private object _lock = new object();

	public SimpleManager(string directoryPath, string prefix)
	{
		this.directoryPath = directoryPath;
		this.prefix = prefix;
	}

	/// <summary>
	/// This method does not saves other kingdoms.
	/// </summary>
	/// <param name="cards"></param>
	/// <returns></returns>
	public override BuyAgenda? Load(IEnumerable<Card> cards)
	{
		var id = cards.ToId();
		int i = (int)cards.OrderBy(c => c.Name).First().Name;

		lock (_lock)
		{
			try
			{
				using (var reader = new StreamReader($"{directoryPath}{prefix}{i}.txt"))
				{
					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine();
						if (!string.IsNullOrEmpty(line))
						{
							if (line.Split(':')[0] == id)
							{
								return BuyAgenda.FromString(line);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
			}
		}
		return null;
	}

	public override BuyAgenda LoadBest(List<Card> cards, IGameLogger? logger = null) => Load(cards);

	public override void Save(IEnumerable<Card> cards, BuyAgenda agenda)
	{
		var id = cards.ToId();
		int i = cards.OrderBy(p => p.Name).Select(p => (int)p.Name).First();

		if (Load(cards) is null)
		{
			lock (_lock)
			{
				using (var writer = File.AppendText($"{directoryPath}{prefix}{i}.txt"))
				{
					writer.WriteLine(agenda.ToString(id));
				}
			}
		}
		else
		{
			var dict = new Dictionary<string, string>();
			lock (_lock)
			{
				if (File.Exists($"{directoryPath}{prefix}{i}.txt"))
				{
					foreach (var line in File.ReadAllLines($"{directoryPath}{prefix}{i}.txt").Select(l => l.Split(':')))
					{
						dict[line[0]] = $"{line[0]}:{line[1]}";
					}
				}

				dict[id] = agenda.ToString(id);
				using (var writer = new StreamWriter($"{directoryPath}{prefix}{i}.txt"))
				{
					foreach (var a in dict.OrderBy(d => d.Key))
					{
						writer.WriteLine(a.Value);
					}
				}
			}
		}
	}

	public List<Card> RandomKingdom()
	{
		var list = new List<List<Card>>();
		FileInfo[] files = new DirectoryInfo($"{directoryPath}").GetFiles($"{prefix}*.txt");

		lock (_lock)
		{
			foreach (var f in files)
			{
				using (var reader = f.OpenText())
				{
					while (!reader.EndOfStream)
					{
						var cards = reader.ReadLine().Split(':')[0].ToCardList();
						if (cards is not null)
						{
							list.Add(cards);
						}
					}
				}
			}
		}
		return list[ThreadSafeRandom.Next(list.Count)];
	}

	public override IEnumerator<BuyAgenda> GetEnumerator()
	{
		for (int i = 8; i < 32; i++)
		{
			lock (_lock)
			{
				if (File.Exists($"{directoryPath}{prefix}{i}.txt"))
				{
					using (var reader = new StreamReader($"{directoryPath}{prefix}{i}.txt"))
					{
						while (!reader.EndOfStream)
						{
							yield return BuyAgenda.FromString(reader.ReadLine());
						}
					}
				}
			}
		}
	}
}
