using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public class Decrement<TIndividual> : IDecrement<TIndividual>
	where TIndividual : IIndividual
{
	#region Fields
	private const int TIMESPAN = 300;
	protected readonly IDecrementTable<TIndividual> _Table;
	private readonly IImprovement<TIndividual> _ImprovementScale;
	private readonly IAdjustment<TIndividual> _Adjustment;
	private readonly IMemoryCache _Cache;
	#endregion

	#region Constructors
	public Decrement(IDecrementTable<TIndividual> table) : this(table, default!, default!, default!) {	}
	public Decrement(IDecrementTable<TIndividual> table, IImprovement<TIndividual> improvementScale) : this(table, improvementScale, default!, default!) {	}
	public Decrement(IDecrementTable<TIndividual> table, IAdjustment<TIndividual> adjustment) : this(table, default!, adjustment, default!) {	}
	public Decrement(IDecrementTable<TIndividual> table, IImprovement<TIndividual> improvementScale, IAdjustment<TIndividual> adjustment) : this(table, improvementScale, adjustment, default!) { }
	public Decrement(IDecrementTable<TIndividual> table, IImprovement<TIndividual> improvementScale, IAdjustment<TIndividual> adjustment, IMemoryCache memoryCache)
	{
		if (table is null)
			throw new ArgumentNullException(nameof(table));
		_Table = table;
		_ImprovementScale = improvementScale ?? IImprovement<TIndividual>.Default;
		_Adjustment = adjustment ?? IAdjustment<TIndividual>.Default;
		_Cache = memoryCache ?? new MemoryCache(new MemoryCacheOptions() { SizeLimit = 5000, });
	}
	#endregion

	protected delegate decimal DecrementOrSurvivalProbabilityIn(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);

	#region private methods
	private static MemoryCacheEntryOptions GetMemoryCacheEntryOptions() => new MemoryCacheEntryOptions().SetSize(1)
																								 .SetSlidingExpiration(TimeSpan.FromSeconds(TIMESPAN))
																								 .SetPriority(CacheItemPriority.Low);
	private decimal SurvivalBetweenIntegerAge(TIndividual individual, in DateOnly firstDate, in DateOnly secondDate)
	{
		decimal deathProbability = _Table.GetRate(individual, firstDate);
		if (deathProbability.Equals(Decimal.One))
			return 1 - IDecrement.UniformDecrementDistribution(1, firstDate, secondDate);
		decimal improvementFactor = _ImprovementScale.ImprovementFactor(individual, _Table.BaseYear, firstDate);
		decimal adjustmentFactor = _Adjustment.AdjustmentFactor(individual);

		return 1 - IDecrement.UniformDecrementDistribution(adjustmentFactor * deathProbability * improvementFactor, firstDate, secondDate);
	}
	private decimal YearlySurvival(TIndividual individual, in DateOnly decrementDate)
	{
		decimal deathProbability = _Table.GetRate(individual, decrementDate);
		if (deathProbability.Equals(Decimal.One))
			return 0m;

		decimal improvementFactor = _ImprovementScale.ImprovementFactor(individual, _Table.BaseYear, decrementDate);
		decimal adjustmentFactor = _Adjustment.AdjustmentFactor(individual);
		return 1 - deathProbability * improvementFactor * adjustmentFactor;
	}
	private decimal GetSurvivalProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate)
	{
		int decrementYear = decrementDate.Year;
		if (calculationDate.Year == decrementYear)
			return SurvivalBetweenIntegerAge(individual, calculationDate, decrementDate);

		decimal survivalProbability = SurvivalBetweenIntegerAge(individual, calculationDate, decrementDate);
		DateOnly nextDate = calculationDate.FirstDayOfFollowingYear();
		while (nextDate.Year < decrementYear)
		{
			survivalProbability *= YearlySurvival(individual, nextDate);
			nextDate = nextDate.AddYears(1);
		}
		survivalProbability *= SurvivalBetweenIntegerAge(individual, nextDate, decrementDate);
		return survivalProbability;
	}
	private decimal GetDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => 1 - GetSurvivalProbability(individual, calculationDate, decrementDate);
	private decimal GetProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, DecrementOrSurvivalProbabilityIn decrementOrSurvivalProbability)
	{
		if (calculationDate > decrementDate) throw new ArgumentException($"{nameof(calculationDate)} must be before {nameof(decrementDate)}");
		if (individual.DateOfBirth > calculationDate) throw new ArgumentException($"{nameof(individual.DateOfBirth)} must be before {nameof(calculationDate)}");
		if (!_Table.IsOlderThanLastAgeOfTheTable(individual, decrementDate))
			return decrementOrSurvivalProbability(individual, calculationDate, decrementDate);
		if (decrementOrSurvivalProbability == GetSurvivalProbability)
			return 0m;
		return 1m;
	}
	private decimal[] GetProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalProbabilityIn decrementOrSurvivalProbability)
	{
		int key = GetHashCode(individual, calculationDate, dates, decrementOrSurvivalProbability);
		if (!_Cache.TryGetValue(key, out decimal[]? result))
		{
			if (calculationDate > dates[0]) throw new ArgumentException($"{nameof(calculationDate)} must be before the first date of {nameof(dates)}");
			if (individual.DateOfBirth > calculationDate) throw new ArgumentException($"{nameof(individual.DateOfBirth)} must be before {nameof(calculationDate)}");
			result = Probabilities(individual, in calculationDate, dates, decrementOrSurvivalProbability);
			_Cache.Set(key, result, GetMemoryCacheEntryOptions());
		}
		return result!;
	}
	private decimal[] Probabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalProbabilityIn decrementOrSurvivalProbability)
	{
		int length = dates.Count;
		decimal[] probabilities = new decimal[length];
		DateOnly lastPossibleDecrementDate = _Table.LastPossibleDecrementDate(individual);
		int i = 0;
		probabilities[i] = decrementOrSurvivalProbability(individual, in calculationDate, dates[i]);
		i++;
		while (i < length
			&& dates[i] < lastPossibleDecrementDate)
		{
			probabilities[i] = probabilities[i - 1] * decrementOrSurvivalProbability(individual, dates[i - 1], dates[i]);
			i++;
		}
		while (i < length)
		{
			probabilities[i] = 0m;
			i++;
		}
		return probabilities;
	}
	protected virtual int GetHashCode(TIndividual individual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalProbabilityIn decrementOrSurvivalProbability) => HashCode.Combine(_Table, individual, calculationDate, dates, decrementOrSurvivalProbability);
	#endregion

	public decimal SurvivalProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate)
		=> GetProbability(individual, in calculationDate, in decrementDate, GetSurvivalProbability);
	public decimal[] SurvivalProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates)
		=> GetProbabilities(individual, in calculationDate, dates, GetSurvivalProbability);
	public decimal[] DecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates)
		=> GetProbabilities(individual, in calculationDate, dates, GetDecrementProbability);
}
