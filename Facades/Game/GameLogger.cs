using GameCore;
using Microsoft.AspNetCore.SignalR;

namespace Dominex.Facades.Game;
internal class GameLogger : IGameLogger
{
	private readonly string _fileName = "Log.txt";
	private readonly StreamWriter _writer;

	public GameLogger()
	{
		_writer = new StreamWriter(_fileName);
	}

	public void Log(string str)
	{
		_writer.WriteLine(str);
		_writer.Flush(); // todo - přes destructor?


	}
}
