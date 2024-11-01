namespace GameCore.Observers;

// todo je tohle idealni nazev?
public interface IPlayerStateObserver
{
	Task Notify(PlayerState playerState);
}
