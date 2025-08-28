using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using Dominex.Services.Game;
using GameCore.Cards;
using Havit.Extensions.DependencyInjection.Abstractions;

namespace Dominex.Facades.Menu;

[Service]
public class GameSetupFacade : IGameSetupFacade
{
	private readonly ICardMapper cardMapper;

	public GameSetupFacade(ICardMapper cardMapper)
	{
		this.cardMapper = cardMapper;
	}

	public Task<List<CardDto>> RequestAvailableCards()
	{
		// todo async?
		return Task.FromResult(PresetGames.Get(PresetGameType.AllCards1stEdition).Select(cardMapper.ToCardDto).ToList());
	}
}
