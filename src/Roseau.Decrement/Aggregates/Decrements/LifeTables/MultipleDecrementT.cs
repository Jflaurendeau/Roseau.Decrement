using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;
using Roseau.Decrement.SeedWork;
using System;
using System.Reflection;

namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public class MultipleDecrement<TIndividual, TDecrementBetweenIntegralAge, TDecrement> : IMultipleDecrement<TIndividual, TDecrementBetweenIntegralAge, TDecrement>
	where TIndividual : IIndividual
	where TDecrementBetweenIntegralAge : IDecrementBetweenIntegralAgeStrategy
	where TDecrement : IDecrementBetweenIntegralAge<TIndividual, TDecrementBetweenIntegralAge>
{
	private const int TIMESPAN = 300;
	private readonly TDecrementBetweenIntegralAge _DecrementBetweenIntegralAge;
	private readonly TDecrement? _Disability;
	private readonly TDecrement? _Lapse;
	private readonly TDecrement? _Mortality;
	protected readonly IMemoryCache _Cache;

	public MultipleDecrement(TDecrementBetweenIntegralAge decrementBetweenIntegralAge, TDecrement? disabilityDecrement, TDecrement? lapseDecrement, TDecrement? mortalityDecrement, IMemoryCache? memoryCache)
	{
		if (decrementBetweenIntegralAge == null)
			throw new ArgumentNullException(nameof(decrementBetweenIntegralAge));
		_DecrementBetweenIntegralAge = decrementBetweenIntegralAge;
		_Disability = disabilityDecrement;
		_Lapse = lapseDecrement;
		_Mortality = mortalityDecrement;
		_Cache = memoryCache ?? new MemoryCache(new MemoryCacheOptions() { SizeLimit = 5000, });
	}

	#region Associated Single Decrement
	public TDecrement? Disability => _Disability;
	public TDecrement? Lapse => _Lapse;
	public TDecrement? Mortality => _Mortality;
	#endregion
	protected static MemoryCacheEntryOptions GetMemoryCacheEntryOptions() => new MemoryCacheEntryOptions().SetSize(1)
																								 .SetSlidingExpiration(TimeSpan.FromSeconds(TIMESPAN))
																								 .SetPriority(CacheItemPriority.Low);
	protected virtual int GetHashCode(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => HashCode.Combine(this, individual, calculationDate, dates);
	protected MultipleDecrementProbabilities GetProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates)
	{
		int key = GetHashCode(individual, calculationDate, dates);
		if (!_Cache.TryGetValue(key, out MultipleDecrementProbabilities? result))
		{
			if (calculationDate > dates[0]) throw new ArgumentException($"{nameof(calculationDate)} must be before the first date of {nameof(dates)}");
			if (individual.DateOfBirth > calculationDate) throw new ArgumentException($"{nameof(individual.DateOfBirth)} must be before {nameof(calculationDate)}");
			result = Probabilities(individual, in calculationDate, dates);
			_Cache.Set(key, result, GetMemoryCacheEntryOptions());
		}
		return result!;
	}
	private MultipleDecrementProbabilities Probabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates)
	{
		if (Disability is null && Lapse is null && Mortality is null)
		{
			int length = dates.Count;
			decimal[] survivalProbabilities = new decimal[length];
			for (int k = 0; k < length; k++)
				survivalProbabilities[k] = Decimal.One;
			return new(survivalProbabilities, null, null, null);
		}
		if (Disability is null && Lapse is null && Mortality is not null) return ProbabilitiesWithOneDecrement(individual, in calculationDate, dates, Mortality!);
		if (Disability is not null && Lapse is null && Mortality is null) return ProbabilitiesWithOneDecrement(individual, in calculationDate, dates, Disability);
		if (Disability is null && Lapse is not null && Mortality is null) return ProbabilitiesWithOneDecrement(individual, in calculationDate, dates, Lapse);

		return ProbabilitiesWithTreeDecrements(individual, in calculationDate, dates);

	}
	private MultipleDecrementProbabilities ProbabilitiesWithOneDecrement(TIndividual individual, in DateOnly calculationDate, OrderedDates dates, TDecrement firstDecrement)
	{
		int length = dates.Count;
		decimal[] survivalProbabilities = new decimal[length];
		decimal[]? firstDecrementProbabilities = new decimal[length];
		MultipleDecrementProbabilities multipleDecrementProbabilities;
		if (Disability is not null) multipleDecrementProbabilities = new(survivalProbabilities, firstDecrementProbabilities, null, null);
		else if (Lapse is not null) multipleDecrementProbabilities = new(survivalProbabilities, null, firstDecrementProbabilities, null);
		else multipleDecrementProbabilities = new(survivalProbabilities, null, null, firstDecrementProbabilities);

		DateOnly lastPossibleDecrementDate = LastPossibleDecrementDate(individual);
		int i = 0;
		survivalProbabilities[i] = firstDecrement.SurvivalProbability(individual, in calculationDate, dates[i]);
		firstDecrementProbabilities[i] = 1 - survivalProbabilities[i];
		i++;
		while (i < length
				&& dates[i] <= lastPossibleDecrementDate)
		{
			survivalProbabilities[i] = survivalProbabilities[i - 1] * firstDecrement.SurvivalProbability(individual, dates[i - 1], dates[i]);
			firstDecrementProbabilities[i] = 1 - survivalProbabilities[i]; 
			i++;
		}
		decimal lastSurvivalProbability = survivalProbabilities[i - 1];
		decimal lastFirstDecrementProbability =  firstDecrementProbabilities[i - 1];
		while (i < length)
		{
			survivalProbabilities[i] = lastSurvivalProbability;
			firstDecrementProbabilities[i] = lastFirstDecrementProbability;
			i++;
		}
		return multipleDecrementProbabilities;
	}
	private MultipleDecrementProbabilities ProbabilitiesWithTreeDecrements(TIndividual individual, in DateOnly calculationDate, OrderedDates dates)
	{
		int length = dates.Count;
		decimal[] survivalProbabilities = new decimal[length];
		decimal[]? disabilityProbabilities = new decimal[length];
		decimal[]? lapseProbabilities = new decimal[length];
		decimal[]? mortalityProbabilities = new decimal[length];
		MultipleDecrementProbabilities multipleDecrementProbabilities = new(survivalProbabilities, 
			_Disability is null ? null : disabilityProbabilities, 
			_Lapse is null ? null : lapseProbabilities, 
			_Mortality is null ? null : mortalityProbabilities);
		DateOnly lastPossibleDecrementDate = LastPossibleDecrementDate(individual);
		int i = 0;
		MultipleDecrementProbability currentProbability = GetDependentProbability(individual, calculationDate, dates[i]);
		survivalProbabilities[i] =		currentProbability.SurvivalProbability;
		if (_Disability is not null)	disabilityProbabilities[i]	= currentProbability.DisabilityProbability!.Value;
		if (_Lapse is not null)			lapseProbabilities[i]		= currentProbability.LapseProbability!.Value;
		if (_Mortality is not null)		mortalityProbabilities[i]	= currentProbability.MortalityProbability!.Value;
		i++;
		while (i < length
			&& dates[i] <= lastPossibleDecrementDate)
		{
			currentProbability *= GetDependentProbability(individual, dates[i - 1], dates[i]);
			survivalProbabilities[i] = currentProbability.SurvivalProbability;
			if (_Disability is not null)	disabilityProbabilities[i]	= currentProbability.DisabilityProbability!.Value;
			if (_Lapse is not null)			lapseProbabilities[i]		= currentProbability.LapseProbability!.Value;
			if (_Mortality is not null)		mortalityProbabilities[i]	= currentProbability.MortalityProbability!.Value;
			i++;
		}
		decimal lastSurvivalProbability = survivalProbabilities[i - 1];
		decimal lastDisabilityProbability = _Disability is null ? 0m : disabilityProbabilities[i - 1];
		decimal lastLapseProbability = _Lapse is null ? 0m : lapseProbabilities[i - 1];
		decimal lastMortalityProbability = _Mortality is null ? 0m : mortalityProbabilities[i - 1];
		while (i < length)
		{
			survivalProbabilities[i] = lastSurvivalProbability;
			disabilityProbabilities[i] = lastDisabilityProbability;
			lapseProbabilities[i] = lastLapseProbability;
			mortalityProbabilities[i] = lastMortalityProbability;
			if (_Disability is not null)	disabilityProbabilities[i]	= lastDisabilityProbability;
			if (_Lapse is not null)			lapseProbabilities[i]		= lastLapseProbability;
			if (_Mortality is not null)		mortalityProbabilities[i]	= lastMortalityProbability;
			i++;
		}
		return multipleDecrementProbabilities;
	}
	
	private MultipleDecrementProbability SurvivalBetweenIntegerAge(TIndividual individual, in DateOnly firstDate, in DateOnly secondDate)
	{
		return _DecrementBetweenIntegralAge.ThreeDecrementsProbability(firstDate,
				secondDate,
				_Disability?.DecrementRate(individual, firstDate),
				_Lapse?.DecrementRate(individual, firstDate),
				_Mortality?.DecrementRate(individual, firstDate));
	}
	private MultipleDecrementProbability YearlyMultipleDecrementRate(TIndividual individual, in DateOnly firstDate)
		=> _DecrementBetweenIntegralAge.ThreeDecrementsRate(_Disability?.DecrementRate(individual, firstDate),
			_Lapse?.DecrementRate(individual, firstDate),
			_Mortality?.DecrementRate(individual, firstDate));
	private MultipleDecrementProbability CalculateDependentProbabilities(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate)
	{
		return GetDependentProbability(individual, calculationDate, decrementDate);
	}
	private MultipleDecrementProbability GetDependentProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate)
	{
		int decrementYear = decrementDate.Year;
		if (calculationDate.Year == decrementYear) 
			return SurvivalBetweenIntegerAge(individual, calculationDate, decrementDate);

		MultipleDecrementProbability multipleDecrementProbability = SurvivalBetweenIntegerAge(individual, calculationDate, decrementDate);
		DateOnly nextDate = calculationDate.FirstDayOfFollowingYear();
		while (nextDate.Year < decrementYear)
		{
			multipleDecrementProbability *= YearlyMultipleDecrementRate(individual, nextDate);
			nextDate = nextDate.AddYears(1);
		}
		
		multipleDecrementProbability *= SurvivalBetweenIntegerAge(individual, nextDate, decrementDate);
		return multipleDecrementProbability;
	}

	public decimal DecrementRate(TIndividual individual, in DateOnly firstDate) => 1 - YearlyMultipleDecrementRate(individual, firstDate).SurvivalProbability;
	public DateOnly LastPossibleDecrementDate(TIndividual individual)
	{
		DateOnly lastPossibleDisabilityDecrementDate = Disability?.LastPossibleDecrementDate(individual) ?? new();
		DateOnly lastPossibleLapseDecrementDate = Lapse?.LastPossibleDecrementDate(individual) ?? new();
		DateOnly lastPossibleMortalityDecrementDate = Mortality?.LastPossibleDecrementDate(individual) ?? new();
		DateOnly lastPossibleDecrementDate = lastPossibleDisabilityDecrementDate < lastPossibleLapseDecrementDate ? lastPossibleLapseDecrementDate : lastPossibleDisabilityDecrementDate;
		return lastPossibleDecrementDate < lastPossibleMortalityDecrementDate ? lastPossibleMortalityDecrementDate : lastPossibleDecrementDate;

	}
	public MultipleDecrementProbability DependentProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => CalculateDependentProbabilities(individual, calculationDate, decrementDate);
	public MultipleDecrementProbabilities DependentProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => GetProbabilities(individual, calculationDate, dates);
	

}
