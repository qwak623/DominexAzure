using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore.Cards;
public static class PresetGames
{
    static Dictionary<Games, List<Card>> games = new Dictionary<Games, List<Card>>();

    // BASE
    static PresetGames()
    { 
        games.Add(Games.AllCards1stEdition, new List<Card>
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
        });

        games.Add(Games.FirstGame, new List<Card>
        {
            Cellar.Get(),
            Moat.Get(),
            Village.Get(),
            Woodcutter.Get(),
            Workshop.Get(),
            Militia.Get(),
            Remodel.Get(),
            Smithy.Get(),
            Market.Get(),
            Mine.Get(),
        });

        games.Add(Games.BigMoney, new List<Card>
        {
            Adventurer.Get(),
            Bureaucrat.Get(),
            Chancellor.Get(),
            Chapel.Get(),
            Feast.Get(),
            Laboratory.Get(),
            Market.Get(),
            Mine.Get(),
            Moneylender.Get(),
            ThroneRoom.Get(),
        });

        games.Add(Games.Interaction, new List<Card>
        {
            Bureaucrat.Get(),
            Chancellor.Get(),
            CouncilRoom.Get(),
            Festival.Get(),
            Library.Get(),
            Militia.Get(),
            Moat.Get(),
            Spy.Get(),
            Thief.Get(),
            Village.Get()
        });

        games.Add(Games.SizeDistortion, new List<Card>
        {
            Cellar.Get(),
            Chapel.Get(),
            Feast.Get(),
            Gardens.Get(),
            Laboratory.Get(),
            Thief.Get(),
            Village.Get(),
            Witch.Get(),
            Woodcutter.Get(),
            Workshop.Get(),
        });

        games.Add(Games.VillageSquare, new List<Card>
        {
            Bureaucrat.Get(),
            Cellar.Get(),
            Festival.Get(),
            Library.Get(),
            Market.Get(),
            Remodel.Get(),
            Smithy.Get(),
            ThroneRoom.Get(),
            Village.Get(),
            Woodcutter.Get(),
        });

        games.Add(Games.ThrashHeap, new List<Card>
        {
            Chapel.Get(),
            Village.Get(),
            Workshop.Get(),
            Woodcutter.Get(),
            Feast.Get(),
            Moneylender.Get(),
            Remodel.Get(),
            Mine.Get(),
            Festival.Get(),
            Market.Get(),
        });
    }

    public static List<Card> Get(Games game) => games[game];

    public static List<Card> VictoryAndTreasures()
    {
        return new List<Card>
        {
            Copper.Get(),
            Silver.Get(),
            Gold.Get(),
            Estate.Get(),
            Duchy.Get(),
            Province.Get()
        };
    }
}
public enum Games
{
    AllCards1stEdition = 0,
    FirstGame = 1,
    BigMoney = 2,
    Interaction = 3,
    SizeDistortion = 4,
    VillageSquare = 5,
    ThrashHeap = 6,
}