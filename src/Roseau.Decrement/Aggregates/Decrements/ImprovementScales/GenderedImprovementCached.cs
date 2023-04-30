using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;
using Roseau.Mathematics;
using System;

namespace Roseau.Decrement.Aggregates.Decrements.ImprovementScales;

public class GenderedImprovementCached : IImprovement<IGenderedIndividual>
{
	#region Fields
	private const int TIMESPAN = 300;
	private readonly IImprovementTable<IGenderedIndividual> _Table;
	private readonly IAdjustment<IGenderedIndividual> _AdjustmentFactor;
	private readonly IMemoryCache _MemoryCache;
	#endregion

	#region Constructors
	public GenderedImprovementCached(IImprovementTable<IGenderedIndividual> table) : this(table, default!, default!) { }
	public GenderedImprovementCached(IImprovementTable<IGenderedIndividual> table, IAdjustment<IGenderedIndividual> adjustmentFactor) : this(table, adjustmentFactor, default!) { }
	public GenderedImprovementCached(IImprovementTable<IGenderedIndividual> table, IMemoryCache memoryCache) : this(table, default!, memoryCache) { }
	public GenderedImprovementCached(IImprovementTable<IGenderedIndividual> table, IAdjustment<IGenderedIndividual> adjustmentFactor, IMemoryCache memoryCache)
	{
		if (table is null)
			throw new ArgumentNullException(nameof(table));
		_Table = table;
		_AdjustmentFactor = adjustmentFactor ?? IAdjustment<IGenderedIndividual>.Default;
		_MemoryCache = memoryCache ?? new MemoryCache(new MemoryCacheOptions() { SizeLimit = 14000, });
	}
	#endregion

	private static MemoryCacheEntryOptions GetMemoryCacheEntryOptions() => new MemoryCacheEntryOptions().SetSize(1)
																								 .SetSlidingExpiration(TimeSpan.FromSeconds(TIMESPAN))
																								 .SetPriority(CacheItemPriority.Low);

	#region Interface and Overrided Methods
	public decimal ImprovementRate(IGenderedIndividual individual, int improvementAge, int improvementYear)
	{
		if (improvementYear < _Table.FirstYear - 1)
			throw new ArgumentOutOfRangeException(nameof(improvementYear), $"The improvement year ({improvementYear}) can not be more than a year before the improvement scale's first year ({_Table.FirstYear}).");
		return _AdjustmentFactor.AdjustmentFactor(individual) * _Table.GetImprovementRate(individual, improvementAge, improvementYear);
	}
	public decimal ImprovementFactor(IGenderedIndividual individual, int tableBaseYear, DateOnly decrementDate)
	{
		var decrementYear = decrementDate.Year;
		if (individual.DateOfBirth > decrementDate)
			throw new ArgumentOutOfRangeException(nameof(decrementDate), $"The decrement date ({decrementDate}) can not be before the date of birth.");
		if (decrementYear < _Table.FirstYear - 1)
			throw new ArgumentOutOfRangeException(nameof(decrementDate), $"The date of calculation ({decrementDate}) can not be before 1999-01-01.");
		if (tableBaseYear < _Table.FirstYear - 1)
			throw new ArgumentOutOfRangeException(nameof(tableBaseYear), $"The base year of the underlying mortality base table ({tableBaseYear}) can not be before 1999.");
		if (decrementYear == tableBaseYear) return 1m;
		var nearestBirth = _Table.AgeLimitedByScale(individual, decrementDate);
		int key = HashCode.Combine(_Table, individual.Gender, nearestBirth, decrementYear);
		if (!_MemoryCache.TryGetValue(key, out decimal? result))
		{
			decimal singleImprovementFactor = 1m;
			decimal improvementFactor = 1m;
			ReadOnlySpan<decimal> improvementRates = _Table.GetImprovementRatesAsMemory(individual, tableBaseYear, decrementDate).Span;
			decimal adjustementFactor = _AdjustmentFactor.AdjustmentFactor(individual);
			int i = -1;
			while(++i < improvementRates.Length)
			{
				singleImprovementFactor = 1 - adjustementFactor * improvementRates[i];
				improvementFactor *= singleImprovementFactor;
			}
			int numberOfImprovementYears = Math.Abs(decrementYear - tableBaseYear);
			while (i < numberOfImprovementYears)
			{
				improvementFactor *= singleImprovementFactor;
				i++;
			}
			if (decrementYear < tableBaseYear)
				return _MemoryCache.Set(key, 1 / improvementFactor, GetMemoryCacheEntryOptions());
			return _MemoryCache.Set(key, improvementFactor, GetMemoryCacheEntryOptions());
		}
		return result!.Value;
	}
	#endregion
}
