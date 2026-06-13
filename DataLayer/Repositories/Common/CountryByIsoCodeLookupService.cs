using System.Linq.Expressions;
using Havit.Data.EntityFrameworkCore.Patterns.Lookups;
using Havit.Data.EntityFrameworkCore.Patterns.SoftDeletes;
using Havit.Data.Patterns.DataSources;
using Havit.Data.Patterns.Infrastructure;
using Havit.Data.Patterns.Repositories;
using Dominex.Model.Common;
using Havit.Data.EntityFrameworkCore;

namespace Dominex.DataLayer.Repositories.Common;

public class CountryByIsoCodeLookupService : LookupServiceBase<string, Country>, ICountryByIsoCodeLookupService
{
	public CountryByIsoCodeLookupService(
		IEntityLookupDataStorage lookupStorage,
		ICountryRepository repository,
		IDbContext dbContext,
		IEntityKeyAccessor entityKeyAccessor,
		ISoftDeleteManager softDeleteManager) : base(lookupStorage, repository, dbContext, entityKeyAccessor, softDeleteManager)
	{
	}

	public Country GetCountryByIsoCode(string isoCode)
	{
		Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(isoCode));

		return GetEntityByLookupKey(isoCode.ToUpper());
	}

	protected override Expression<Func<Country, string>> LookupKeyExpression => country => country.IsoCode;
	protected override LookupServiceOptimizationHints OptimizationHints => LookupServiceOptimizationHints.EntityIsReadOnly;
}
