
using Dominex.Model.Common;

namespace Dominex.DataLayer.Repositories.Common;

public interface ICountryByIsoCodeLookupService
{
	Country GetCountryByIsoCode(string isoCode);
}
