using Dominex.Facades.Game.Hubs;
using GameCore.GameCore;
using GameCore.Observers;
using Havit.Extensions.DependencyInjection.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Dominex.Facades.Game.Observers;

// todo vyresit jestli toto ma byt singleton (mel by byt jeden pro kazdou hru
[Service(Lifetime = ServiceLifetime.Singleton)]
internal class GameLogger : IGameLogger
{
	//private readonly string _fileName = "Log.txt";
	//private readonly StreamWriter _writer;

	private readonly IHubContext<LogHub> logHubContext;
	public List<GameLog> LogHistory { get; private set; } = new();

	public GameLogger(IHubContext<LogHub> logHubContext)
	{
		this.logHubContext = logHubContext;
		//_writer = new StreamWriter(_fileName);
	}

	public async Task Log(GameLog gameLog)
	{
		LogHistory.Add(gameLog);
		await logHubContext.Clients.All.SendAsync("AppendLog", gameLog, LogHistory.Count);

		//_writer.WriteLine(str);
		//_writer.Flush(); // todo - přes destructor?
	}

	public async Task Log(string message) // todo temporary
	{
		var gameLog = new GameLog
		{
			PlayerId = "ai",
			Message = message
		};

		LogHistory.Add(gameLog);
		await logHubContext.Clients.All.SendAsync("AppendLog", gameLog, LogHistory.Count);
	}
}
