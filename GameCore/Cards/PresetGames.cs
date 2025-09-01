using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;

namespace GameCore.Cards;
public static class PresetGames
{
	public static List<Card> AvailableCards { get; } = new()
	{
		Adventurer.Get(),
		Bureaucrat.Get(),
		Cellar.Get(),
		CouncilRoom.Get(),
		Feast.Get(),
		Festival.Get(),
		Gardens.Get(),
		Chancellor.Get(),
		Chapel.Get(),
		Laboratory.Get(),
		Library.Get(),
		Market.Get(),
		Militia.Get(),
		Mine.Get(),
		Moat.Get(),
		Moneylender.Get(),
		Remodel.Get(),
		Smithy.Get(),
		Spy.Get(),
		Thief.Get(),
		ThroneRoom.Get(),
		Village.Get(),
		Witch.Get(),
		Woodcutter.Get(),
		Workshop.Get(),
	};

	public static List<Card> VictoryAndTreasures() => new()
	{
		Copper.Get(),
		Silver.Get(),
		Gold.Get(),
		Estate.Get(),
		Duchy.Get(),
		Province.Get()
	};
}