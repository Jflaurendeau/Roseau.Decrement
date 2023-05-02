using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Decrements.Adjustments;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public class GenderedDecrement<TGenderedIndividual> : Decrement<TGenderedIndividual>, IUnisexDecrementT<TGenderedIndividual>
	where TGenderedIndividual : IGenderedIndividual
{
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table) : base(table, default!, default!, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IImprovement<TGenderedIndividual> improvementScale) : base(table, improvementScale, default!, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IAdjustment<TGenderedIndividual> adjustment) : base(table, default!, adjustment, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IImprovement<TGenderedIndividual> improvementScale, IAdjustment<TGenderedIndividual> adjustment) : base(table, improvementScale, adjustment, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IImprovement<TGenderedIndividual> improvementScale, IAdjustment<TGenderedIndividual> adjustment, IMemoryCache memoryCache) : base(table, improvementScale, adjustment, memoryCache) { }

	protected delegate decimal DecrementOrSurvivalUnisexProbabilityIn(TGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, in decimal manProportion);
	protected int GetHashCode(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalUnisexProbabilityIn decrementOrSurvivalProbability, decimal manProportion) => HashCode.Combine(_Table, individual.Gender, individual.DateOfBirth, calculationDate, dates, decrementOrSurvivalProbability, manProportion);

	#region Private methods
	private decimal SurvivalUnisexBetweenIntegerAge(TGenderedIndividual individual, in DateOnly firstDate, in DateOnly secondDate, in decimal manProportion)
	{
		TGenderedIndividual manIndividual = individual.Gender == Gender.Man ? individual : (TGenderedIndividual)individual.AsOtherGender();
		TGenderedIndividual womanIndividual = individual.Gender == Gender.Woman ? individual : (TGenderedIndividual)individual.AsOtherGender();
		decimal manDecrement = DecrementRate(manIndividual, firstDate);
		decimal womanDecrement = DecrementRate(womanIndividual, firstDate);
		return 1 - IDecrement.UniformDecrementDistribution(manDecrement * manProportion + (1 - manProportion) * womanDecrement, firstDate, secondDate);
	}
	private decimal YearlyUnisexSurvival(TGenderedIndividual individual, in DateOnly decrementDate, in decimal manProportion)
	{
		TGenderedIndividual manIndividual = individual.Gender == Gender.Man ? individual : (TGenderedIndividual)individual.AsOtherGender();
		TGenderedIndividual womanIndividual = individual.Gender == Gender.Woman ? individual : (TGenderedIndividual)individual.AsOtherGender();
		decimal manDecrement = DecrementRate(manIndividual, decrementDate);
		decimal womanDecrement = DecrementRate(womanIndividual, decrementDate);
		return 1 - manDecrement * manProportion + (1 - manProportion) * womanDecrement;
	}
	private decimal GetSurvivalUnisexProbability(TGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, in decimal manProportion)
	{
		int decrementYear = decrementDate.Year;
		if (calculationDate.Year == decrementYear)
			return SurvivalUnisexBetweenIntegerAge(individual, calculationDate, decrementDate, manProportion);

		decimal survivalProbability = SurvivalUnisexBetweenIntegerAge(individual, calculationDate, decrementDate, manProportion);
		DateOnly nextDate = calculationDate.FirstDayOfFollowingYear();
		while (nextDate.Year < decrementYear)
		{
			survivalProbability *= YearlyUnisexSurvival(individual, nextDate, manProportion);
			nextDate = nextDate.AddYears(1);
		}
		survivalProbability *= SurvivalUnisexBetweenIntegerAge(individual, nextDate, decrementDate, manProportion);
		return survivalProbability;
	}
	private decimal GetDecrementUnisexProbability(TGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, in decimal manProportion) => 1 - GetSurvivalUnisexProbability(individual, calculationDate, decrementDate, manProportion);
	private decimal GetUnisexProbability(TGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, DecrementOrSurvivalUnisexProbabilityIn decrementOrSurvivalProbability, in decimal manProportion)
	{
		if (calculationDate > decrementDate) throw new ArgumentException($"{nameof(calculationDate)} must be before {nameof(decrementDate)}");
		if (individual.DateOfBirth > calculationDate) throw new ArgumentException($"{nameof(individual.DateOfBirth)} must be before {nameof(calculationDate)}");
		if (!_Table.IsOlderThanLastAgeOfTheTable(individual, decrementDate))
			return decrementOrSurvivalProbability(individual, calculationDate, decrementDate, manProportion);
		if (decrementOrSurvivalProbability == GetSurvivalUnisexProbability)
			return 0m;
		return 1m;
	}
	private decimal[] GetUnisexProbabilities(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalUnisexProbabilityIn decrementOrSurvivalProbability, in decimal manProportion)
	{
		int key = GetHashCode(individual, calculationDate, dates, decrementOrSurvivalProbability, manProportion);
		if (!_Cache.TryGetValue(key, out decimal[]? result))
		{
			if (calculationDate > dates[0]) throw new ArgumentException($"{nameof(calculationDate)} must be before the first date of {nameof(dates)}");
			if (individual.DateOfBirth > calculationDate) throw new ArgumentException($"{nameof(individual.DateOfBirth)} must be before {nameof(calculationDate)}");
			result = UnisexProbabilities(individual, in calculationDate, dates, decrementOrSurvivalProbability, manProportion);
			_Cache.Set(key, result, GetMemoryCacheEntryOptions());
		}
		return result!;
	}
	private decimal[] UnisexProbabilities(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalUnisexProbabilityIn decrementOrSurvivalProbability, in decimal manProportion)
	{
		int length = dates.Count;
		decimal[] probabilities = new decimal[length];
		DateOnly lastPossibleDecrementDate = _Table.LastPossibleDecrementDate(individual);
		int i = 0;
		probabilities[i] = decrementOrSurvivalProbability(individual, in calculationDate, dates[i], manProportion);
		i++;
		while (i < length
			&& dates[i] < lastPossibleDecrementDate)
		{
			probabilities[i] = probabilities[i - 1] * decrementOrSurvivalProbability(individual, dates[i - 1], dates[i], manProportion);
			i++;
		}
		while (i < length)
		{
			probabilities[i] = 0m;
			i++;
		}
		return probabilities;
	}
	#endregion

	#region IUnisexDecrement interface
	public decimal[] DecrementUnisexProbabilities(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, decimal manProportion)
	{
		if (manProportion < 0 || manProportion > 1)
			throw new ArgumentOutOfRangeException(nameof(manProportion), $"The man proportion variable ({manProportion}) must be between 0 and 1 inclusively");
		if (manProportion == Decimal.One)
			return GetProbabilities(individual.Gender == Gender.Man ? individual : (TGenderedIndividual)individual.AsOtherGender(), calculationDate, dates, GetDecrementProbability);
		if (manProportion == Decimal.Zero)
			return GetProbabilities(individual.Gender == Gender.Woman ? individual : (TGenderedIndividual)individual.AsOtherGender(), calculationDate, dates, GetDecrementProbability);
		return GetUnisexProbabilities(individual, calculationDate, dates, GetDecrementUnisexProbability, manProportion);
	}
	public decimal[] SurvivalUnisexProbabilities(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, decimal manProportion)
	{
		if (manProportion < 0 || manProportion > 1)
			throw new ArgumentOutOfRangeException(nameof(manProportion), $"The man proportion variable ({manProportion}) must be between 0 and 1 inclusively");
		if (manProportion == Decimal.One)
			return GetProbabilities(individual.Gender == Gender.Man ? individual : (TGenderedIndividual)individual.AsOtherGender(), calculationDate, dates, GetSurvivalProbability);
		if (manProportion == Decimal.Zero)
			return GetProbabilities(individual.Gender == Gender.Woman ? individual : (TGenderedIndividual)individual.AsOtherGender(), calculationDate, dates, GetSurvivalProbability);
		return GetUnisexProbabilities(individual, calculationDate, dates, GetSurvivalUnisexProbability, manProportion);
	}
	public decimal SurvivalUnisexProbability(TGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, decimal manProportion)
	{
		if (manProportion < 0 || manProportion > 1)
			throw new ArgumentOutOfRangeException(nameof(manProportion), $"The man proportion variable ({manProportion}) must be between 0 and 1 inclusively");
		if (manProportion == Decimal.One)
			return GetProbability(individual.Gender == Gender.Man ? individual : (TGenderedIndividual)individual.AsOtherGender(), calculationDate, decrementDate, GetSurvivalProbability);
		if (manProportion == Decimal.Zero)
			return GetProbability(individual.Gender == Gender.Woman ? individual : (TGenderedIndividual)individual.AsOtherGender(), calculationDate, decrementDate, GetSurvivalProbability);
		return GetUnisexProbability(individual, calculationDate, decrementDate, GetSurvivalUnisexProbability, manProportion);
	}
	#endregion
}
