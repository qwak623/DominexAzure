using Dominex.Contracts.Game;
using Dominex.Contracts.Menu;
using Dominex.Contracts.ServerApi;
using Dominex.DataLayer.Repositories.Game;
using Dominex.Services.Game;
using GameCore.Cards;
using Havit.Extensions.DependencyInjection.Abstractions;

namespace Dominex.Facades.Menu;

[Service]
public class GameSetupFacade : IGameSetupFacade
{
	private readonly ICardMapper cardMapper;
	private readonly IPresetKingdomRepository presetKingdomRepository;

	public GameSetupFacade(ICardMapper cardMapper, IPresetKingdomRepository presetKingdomRepository)
	{
		this.cardMapper = cardMapper;
		this.presetKingdomRepository = presetKingdomRepository;
	}

	public Task<List<CardDto>> RequestAvailableCards()
	{
		// todo async?
		return Task.FromResult(cardMapper.ToCardDto(PresetGames.AvailableCards).ToList());
	}

	public async Task<List<PresetKingdomDto>> RequestPresetKingdoms()
	{
		// TODO validovat, jestli je to v available cards? 
		var presetKingdoms = new List<Model.Game.PresetKingdom>(); // TODO await presetKingdomRepository.GetAllAsync();

		// todo move mapper somewhere
		return presetKingdoms.Select(k => new PresetKingdomDto
		{
			Name = k.Name,
			Cards = cardMapper.ToCardDto(k.Cards.Select(c => Card.Get(Enum.Parse<CardType>(c)))).ToList(),
		}).ToList();
	}

	public Task<GetRandomCardsResponse> GetRandomCards(GetRandomCardsRequest request)
	{
		// TODO ThreadSafeRandom? 
		Random rnd = new();
		var randomCards = request.AvailableCards.OrderBy(_ => rnd.Next()).Take(request.Count).ToList();
		return Task.FromResult(new GetRandomCardsResponse { Cards = randomCards });
	}
}