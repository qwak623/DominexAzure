using Dominex.Contracts.Game;
using Dominex.Contracts.Menu;
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
		return Task.FromResult(cardMapper.ToCardDto(PresetGames.Get(PresetGameType.AllCards1stEdition)).ToList());
	}

	public Task<List<PresetKingdomDto>> RequestPresetKingdoms()
	{
		// TODO validovat, jestli je to v available cards? 
		List<PresetKingdomDto> presetGames = Enum.GetValues<PresetGameType>()
			.Select(pgt => new PresetKingdomDto
			{
				Name = pgt.ToString(),
				Cards = cardMapper.ToCardDto(PresetGames.Get(pgt)).ToList()
			}).ToList();

		return Task.FromResult(presetGames);
	}
}