using Havit.Data.EntityFrameworkCore.Patterns.QueryServices;
using Dominex.Contracts;

namespace Dominex.DataLayer;

// todo what is ths for?
public static class DataFragmentExtensions
{
	public static DataFragmentResult<TItem> ToDataFragmentResult<TItem>(this DataFragment<TItem> source)
	{
		return new DataFragmentResult<TItem>
		{
			Data = source.Data,
			TotalCount = source.TotalCount
		};
	}
}