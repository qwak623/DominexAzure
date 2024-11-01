using Dominex.Facades.Infrastructure;
using GameCore;
using Havit.Extensions.DependencyInjection.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Dominex.Facades.Game;

// todo vyresit jestli toto ma byt singleton (mel by byt jeden pro kazdou hru
[Service(Lifetime = ServiceLifetime.Singleton)]
internal class GameLogger : IGameLogger
{
	//private readonly string _fileName = "Log.txt";
	//private readonly StreamWriter _writer;

	private readonly IHubContext<LogHub> logHubContext;
	public List<string> LogHistory { get; private set; } = new();

	public GameLogger(IHubContext<LogHub> logHubContext)
	{
		this.logHubContext = logHubContext;
		//_writer = new StreamWriter(_fileName);
	}

	public async Task Log(string log)
	{
		LogHistory.Add(log);
		await logHubContext.Clients.All.SendAsync("AppendLog", log, LogHistory.Count);

		//_writer.WriteLine(str);
		//_writer.Flush(); // todo - přes destructor?
	}
}
