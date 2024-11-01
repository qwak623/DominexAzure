using GameCore.Cards;

namespace GameCore.Observers;
public interface IKingdomObserver
{
	Task Notify(Kingdom kingdom);
}
