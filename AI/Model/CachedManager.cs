using AI.Provincial;
using GameCore;
using GameCore.Cards;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Utils;
using GameCore.Observers;

namespace AI.Model;
public class CachedManager : BuyAgendaManager
{
	string directoryPath;
	string prefix;
	int subsetSize;
	Dictionary<string, string>[] files = new Dictionary<string, string>[33];
	object[] locks = new object[33];

	public CachedManager(string directoryPath, int subsetSize, string prefix)
	{
		this.directoryPath = directoryPath;
		this.subsetSize = subsetSize;
		this.prefix = prefix;
		for (int i = 0; i < locks.Length; i++)
			locks[i] = new object();
	}

	/// <summary>
	/// This method does saves other kingdoms.
	/// </summary>
	/// <param name="cards"></param>
	/// <returns></returns>
	public override BuyAgenda Load(IEnumerable<Card> cards)
	{
		var id = cards.ToId();
		int i = cards.OrderBy(p => p.Type).Select(p => (int)p.Type).First();

		try
		{
			if (files[i] is null)
				LoadAllAgendas(i);

			lock (locks[i])
				return BuyAgenda.FromString(files[i][id]);
		}
		catch (Exception)
		{
			return null;
		}
	}

	void LoadAllAgendas(int i)
	{
		files[i] = new Dictionary<string, string>();

		lock (locks[i])
		{
			using (var reader = new StreamReader($"{directoryPath}{prefix}{i}.txt"))
			{
				while (!reader.EndOfStream)
				{
					string line = reader.ReadLine();
					if (!string.IsNullOrEmpty(line))
						files[i].Add(line.Split(':')[0], line);
				}
			}
		}
	}

	public override void Save(IEnumerable<Card> cards, BuyAgenda agenda)
	{
		string id = cards.ToId();
		int i = cards.OrderBy(p => p).Select(p => (int)p.Type).First();

		if (Load(cards) is null)
			lock (locks[i])
				File.AppendAllText($"{directoryPath}{prefix}{i}.txt", agenda.ToString(id) + Environment.NewLine);
		else
		{
			var dict = new Dictionary<string, string>();
			lock (locks[i])
			{
				files[i] = null;
				foreach (var line in File.ReadAllLines($"{directoryPath}{prefix}{i}.txt").Select(l => l.Split(':')))
					dict[line[0]] = $"{line[0]}:{line[1]}";
				dict[id] = agenda.ToString(id);
				using (var writer = new StreamWriter($"{directoryPath}{prefix}{i}.txt"))
					foreach (var a in dict.OrderBy(d => d.Key))
						writer.WriteLine(a.Value);
			}
		}
	}

	/// <summary>
	/// Loads all possible agendas wich fits to specified kingdom.
	/// Then the best one is selected.
	/// </summary>
	/// <param name="k"></param>
	/// <param name="logger"></param>
	/// <returns></returns>
	public override BuyAgenda LoadBest(List<Card> k, IGameLogger logger = null)
	{
		k = k.OrderBy(c => c.Type).ToList();

		// loading all fives
		var sw = new Stopwatch();
		sw.Start();

		var agendas = new List<BuyAgendaTournament.Tuple>();
		foreach (var five in new Subsets(k.Where(c => c.Type > CardType.Curse).Select(c => (int)c.Type).ToList(), subsetSize))
		{
			var cards = five.Select(i => Card.Get((CardType)i)).ToList();

			var agenda = Load(cards);
			if (agenda is not null)
				agendas.Add(new BuyAgendaTournament.Tuple { Agenda = agenda, Wins = 0, Id = cards.ToId() });
		}
		Console.WriteLine("AgendaCount " + agendas.Count);

		// if there are no agendas fitting with this kingdom
		if (agendas.Count == 0)
			return null;

		//agendas.Add(new Tuple { Agenda = new Tens(directoryPath).Load(k), Wins = 0, Cards = k });

		sw.Stop();
		logger?.Log($"Loading time: {sw.Elapsed.TotalMilliseconds}ms");
		logger?.Log($"Agendas count: {agendas.Count}");

		// tournament
		var agendasNextRound = new List<BuyAgendaTournament.Tuple>();

		// first round
		sw.Restart();
		var group = agendas.Take(16).ToList();
		agendas = agendas.Skip(16).ToList();
		int groupId = 0;
		while (group.Any())
		{
			group.Tournament(k, 5);
			logger?.Log($"group {groupId++}");
			group.ShowResults(logger);

			agendasNextRound.AddRange(group.OrderByDescending(a => a.Wins).Take(4));
			group = agendas.Take(16).ToList();
			agendas = agendas.Skip(16).ToList();
		}

		sw.Stop();
		logger?.Log($"Tournament first round time: {sw.Elapsed.TotalMilliseconds}ms");

		// second round
		sw.Restart();
		group = agendasNextRound.Take(16).ToList();
		agendasNextRound = agendasNextRound.Skip(16).ToList();
		groupId = 0;
		while (group.Any())
		{
			group.Tournament(k, 5);
			logger?.Log($"group {groupId++}");
			group.ShowResults(logger);

			agendas.AddRange(group.OrderByDescending(a => a.Wins).Take(4));
			group = agendasNextRound.Take(16).ToList();
			agendasNextRound = agendasNextRound.Skip(16).ToList();
		}

		sw.Stop();
		logger?.Log($"Tournament second round time: {sw.Elapsed.TotalMilliseconds}ms");

		// third round
		sw.Restart();
		agendas.Tournament(k, 5);
		sw.Stop();
		logger?.Log($"Tournament third round time: {sw.Elapsed.TotalMilliseconds}ms");

		agendas.ShowResults(logger);

		// final round
		sw.Restart();
		agendas = agendas.OrderByDescending(a => a.Wins).Take(5).ToList();
		agendas.Tournament(k, 100);
		sw.Stop();
		logger?.Log($"Tournament final round time: {sw.Elapsed.TotalMilliseconds}ms");

		agendas.ShowResults(logger);

		return agendas.OrderByDescending(x => x.Wins).FirstOrDefault().Agenda;
	}

	public override IEnumerator<BuyAgenda> GetEnumerator()
	{
		for (int i = 8; i < 32; i++)
		{
			lock (locks[i])
			{
				try
				{
					if (files[i] is null)
						LoadAllAgendas(i);
				}
				catch
				{
					continue;
				}

				foreach (var item in files[i])
					yield return BuyAgenda.FromString(item.Value);
			}
		}

		// free memory
		files = new Dictionary<string, string>[33];
	}
}