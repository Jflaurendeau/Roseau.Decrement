using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;
using Roseau.Mathematics;
using System;

namespace Roseau.Decrement.Aggregates.Decrements.ImprovementScales;

public class ImprovementCached<TIndividual> : IImprovement<TIndividual>
	where TIndividual : IIndividual
{
	#region Fields
	private const int TIMESPAN = 300;
	private readonly IImprovement<TIndividual> _Improvement;
	private readonly IMemoryCache _MemoryCache;
	#endregion

	#region Constructors
	public ImprovementCached(IImprovement<TIndividual> improvement) : this(improvement, default!) { }
	public ImprovementCached(IImprovement<TIndividual> improvement, IMemoryCache memoryCache)
	{
		if (improvement is null)
			throw new ArgumentNullException(nameof(improvement));
		_Improvement = improvement;
		_MemoryCache = memoryCache ?? new MemoryCache(new MemoryCacheOptions() { SizeLimit = 14000, });
	}
	#endregion

	private static MemoryCacheEntryOptions GetMemoryCacheEntryOptions() => new MemoryCacheEntryOptions().SetSize(1)
																								 .SetSlidingExpiration(TimeSpan.FromSeconds(TIMESPAN))
																								 .SetPriority(CacheItemPriority.Low);

	#region Interface and Overrided Methods
	public int FirstYear => _Improvement.FirstYear;
	public decimal ImprovementRate(TIndividual individual, int improvementAge, int improvementYear)
		=> _Improvement.ImprovementRate(individual, improvementAge, improvementYear);
	public decimal ImprovementFactor(TIndividual individual, int tableBaseYear, DateOnly decrementDate)
	{
		var decrementYear = decrementDate.Year;
		if (individual.DateOfBirth > decrementDate)
			throw new ArgumentOutOfRangeException(nameof(decrementDate), $"The decrement date ({decrementDate}) can not be before the date of birth.");
		if (decrementYear < FirstYear - 1)
			throw new ArgumentOutOfRangeException(nameof(decrementDate), $"The date of calculation ({decrementDate}) can not be before 1999-01-01.");
		if (tableBaseYear < FirstYear - 1)
			throw new ArgumentOutOfRangeException(nameof(tableBaseYear), $"The base year of the underlying mortality base table ({tableBaseYear}) can not be before 1999.");
		if (decrementYear == tableBaseYear) return 1m;
		int key = GetHashCode(individual, tableBaseYear, decrementDate);
		if (!_MemoryCache.TryGetValue(key, out decimal? result))
		{
			result = _Improvement.ImprovementFactor(individual, tableBaseYear, decrementDate);
			_MemoryCache.Set(key, result, GetMemoryCacheEntryOptions());
		}
		return result!.Value;
	}
	public int GetHashCode(TIndividual individual, int tableBaseYear, DateOnly decrementDate) => _Improvement.GetHashCode(individual, tableBaseYear, decrementDate);
	#endregion
}
