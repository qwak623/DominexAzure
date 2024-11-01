namespace GameCore;

public interface IGameLogger
{
	List<string> LogHistory { get; }
	Task Log(string str);
}
