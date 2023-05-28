using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public abstract class MultipleDecrement<TIndividual, TDecrementBetweenIntegralAge, TDecrement> : IMultipleDecrement<TIndividual, TDecrementBetweenIntegralAge, TDecrement>
	where TIndividual : IIndividual
	where TDecrementBetweenIntegralAge : IDecrementBetweenIntegralAgeStrategy, new()
	where TDecrement : IDecrement<TIndividual>, IDecrementBetweenIntegralAge<TDecrementBetweenIntegralAge>
{
	private const int TIMESPAN = 300;
	private readonly TDecrement? _Disability;
	private readonly TDecrement? _Lapse;
	private readonly TDecrement? _Mortality;
	protected readonly IMemoryCache _Cache;

	protected MultipleDecrement(TDecrement? disabilityDecrement, TDecrement? lapseDecrement, TDecrement? mortalityDecrement, IMemoryCache memoryCache)
	{
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

	protected IMultipleDecrement<TIndividual, TDecrementBetweenIntegralAge, TDecrement> AsMultipleDecrement => this;
	protected delegate decimal DecrementOrSurvivalProbabilityIn(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);
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
		int length = dates.Count;
		decimal[] survivalProbabilities = new decimal[length];
		decimal[]? disabilityProbabilities = Disability is null ? null : new decimal[length];
		decimal[]? lapseProbabilities = Lapse is null ? null : new decimal[length];
		decimal[]? mortalityProbabilities = Mortality is null ? null : new decimal[length];
		MultipleDecrementProbabilities multipleDecrementProbabilities = new(survivalProbabilities, disabilityProbabilities, lapseProbabilities, mortalityProbabilities);
		DateOnly lastPossibleDecrementDate = LastPossibleDecrementDate(individual);
		int i = 0;
		decimal defaultValue = 0m;
		DependentProbabilities(individual, calculationDate, dates[i], out survivalProbabilities[i], out ReferenceOrDefault(ref defaultValue, i, ref disabilityProbabilities), out ReferenceOrDefault(ref defaultValue, i, ref lapseProbabilities), out ReferenceOrDefault(ref defaultValue, i, ref mortalityProbabilities));
		i++;
		while (i < length
			&& dates[i] <= lastPossibleDecrementDate)
		{
			DependentProbabilities(individual, dates[i - 1], dates[i], out survivalProbabilities[i], out ReferenceOrDefault(ref defaultValue, i, ref disabilityProbabilities), out ReferenceOrDefault(ref defaultValue, i, ref lapseProbabilities), out ReferenceOrDefault(ref defaultValue, i, ref mortalityProbabilities));
			survivalProbabilities[i] *= survivalProbabilities[i - 1];
			if (disabilityProbabilities is not null) disabilityProbabilities[i] = disabilityProbabilities[i - 1] + disabilityProbabilities[i] * survivalProbabilities[i - 1];
			if (lapseProbabilities is not null) lapseProbabilities[i] = lapseProbabilities[i - 1] + lapseProbabilities[i] * survivalProbabilities[i - 1];
			if (mortalityProbabilities is not null) mortalityProbabilities[i] = mortalityProbabilities[i - 1] + mortalityProbabilities[i] * survivalProbabilities[i - 1];
			i++;
		}
		decimal lastSurvivalProbability = survivalProbabilities[i - 1];
		decimal lastDisabilityProbability = disabilityProbabilities is not null ? disabilityProbabilities[i - 1] : 0m;
		decimal lastLapseProbability = lapseProbabilities is not null ? lapseProbabilities[i - 1] : 0m;
		decimal lastMortalityProbability = mortalityProbabilities is not null ? mortalityProbabilities[i - 1] : 0m;
		while (i < length)
		{
			survivalProbabilities[i] = lastSurvivalProbability;
			if (disabilityProbabilities is not null) disabilityProbabilities[i] = lastDisabilityProbability;
			if (lapseProbabilities is not null) lapseProbabilities[i] = lastLapseProbability;
			if (mortalityProbabilities is not null) mortalityProbabilities[i] = lastMortalityProbability;
			i++;
		}
		return multipleDecrementProbabilities;
	}
	private static ref decimal ReferenceOrDefault(ref decimal defaultValue, in int index, ref decimal[]? array) => ref (array is null ? ref defaultValue : ref array[index]);
	protected abstract void CalculateDependentProbabilities(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, ref decimal survivalProbability, ref decimal disabilityProbability, ref decimal lapseProbability, ref decimal mortalityProbability);
	private MultipleDecrementProbability CalculateDependentProbabilities(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate)
	{
		decimal disabilityProbability = Disability?.DecrementProbability(individual, calculationDate, decrementDate) ?? 0m;
		decimal lapseProbability = Lapse?.DecrementProbability(individual, calculationDate, decrementDate) ?? 0m;
		decimal mortalityProbability = Mortality?.DecrementProbability(individual, calculationDate, decrementDate) ?? 0m;
		decimal survivalProbability = (1 - disabilityProbability) * (1 - lapseProbability) * (1 - mortalityProbability);
		CalculateDependentProbabilities(individual, calculationDate, decrementDate, ref survivalProbability, ref disabilityProbability, ref lapseProbability, ref mortalityProbability);
		return new MultipleDecrementProbability(survivalProbability, Disability is null ? null : disabilityProbability, Lapse is null ? null : lapseProbability, Mortality is null ? null : mortalityProbability);
	}
	
	public DateOnly LastPossibleDecrementDate(TIndividual individual)
	{
		DateOnly lastPossibleDisabilityDecrementDate = Disability?.LastPossibleDecrementDate(individual) ?? new();
		DateOnly lastPossibleLapseDecrementDate = Lapse?.LastPossibleDecrementDate(individual) ?? new();
		DateOnly lastPossibleMortalityDecrementDate = Mortality?.LastPossibleDecrementDate(individual) ?? new();
		DateOnly lastPossibleDecrementDate = lastPossibleDisabilityDecrementDate < lastPossibleLapseDecrementDate ? lastPossibleLapseDecrementDate : lastPossibleDisabilityDecrementDate;
		return lastPossibleDecrementDate < lastPossibleMortalityDecrementDate ? lastPossibleMortalityDecrementDate : lastPossibleDecrementDate;

	}
	public decimal[] DecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates)
	{
		throw new NotImplementedException();
	}
	public decimal[] SurvivalProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates)
	{
		throw new NotImplementedException();
	}
	public MultipleDecrementProbability DependentProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => CalculateDependentProbabilities(individual, calculationDate, decrementDate);
	private void DependentProbabilities(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, out decimal survivalProbability, out decimal disabilityProbability, out decimal lapseProbability, out decimal mortalityProbability)
	{
		disabilityProbability = Disability?.DecrementProbability(individual, calculationDate, decrementDate) ?? 0m;
		lapseProbability = Lapse?.DecrementProbability(individual, calculationDate, decrementDate) ?? 0m;
		mortalityProbability = Mortality?.DecrementProbability(individual, calculationDate, decrementDate) ?? 0m;
		survivalProbability = (1 - disabilityProbability) * (1 - lapseProbability) * (1 - mortalityProbability);
		CalculateDependentProbabilities(individual, calculationDate, decrementDate, ref survivalProbability, ref disabilityProbability, ref lapseProbability, ref mortalityProbability);
	}
	public abstract decimal? DependentDisabilityDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);
	public abstract decimal[]? DependentDisabilityDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates);
	public abstract decimal? DependentLapseDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);
	public abstract decimal[]? DependentLapseDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates);
	public abstract decimal? DependentMortalityDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);
	public abstract decimal[]? DependentMortalityDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates);

}
