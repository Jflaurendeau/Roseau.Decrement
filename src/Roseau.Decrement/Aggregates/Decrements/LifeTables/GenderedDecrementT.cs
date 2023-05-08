using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Decrements.Adjustments;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public class GenderedDecrement<TGenderedIndividual> : Decrement<TGenderedIndividual>, IUnisexDecrement<TGenderedIndividual>
	where TGenderedIndividual : IGenderedIndividual
{
	#region Constructors
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table) : base(table, default!, default!, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IImprovement<TGenderedIndividual> improvementScale) : base(table, improvementScale, default!, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IAdjustment<TGenderedIndividual> adjustment) : base(table, default!, adjustment, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IImprovement<TGenderedIndividual> improvementScale, IAdjustment<TGenderedIndividual> adjustment) : base(table, improvementScale, adjustment, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IImprovement<TGenderedIndividual> improvementScale, IAdjustment<TGenderedIndividual> adjustment, IMemoryCache memoryCache) : base(table, improvementScale, adjustment, memoryCache) { }
	#endregion

	private delegate decimal DecrementOrSurvivalUnisexProbabilityIn(TGenderedIndividual manIndividual, TGenderedIndividual womanIndividual, in DateOnly calculationDate, in DateOnly decrementDate, in decimal manProportion);
	#region Private and protected methods
	protected override int GetHashCode(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalProbabilityIn decrementOrSurvivalProbability) => HashCode.Combine(_Table, individual.Gender, individual.DateOfBirth, calculationDate, dates, decrementOrSurvivalProbability);


	private int GetHashCode(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalUnisexProbabilityIn decrementOrSurvivalProbability, in decimal manProportion) => HashCode.Combine(_Table, individual.DateOfBirth, calculationDate, dates, decrementOrSurvivalProbability, manProportion);
	private decimal SurvivalUnisexBetweenIntegerAge(TGenderedIndividual manIndividual, TGenderedIndividual womanIndividual, in DateOnly firstDate, in DateOnly secondDate, in decimal manProportion)
	{
		decimal manDecrement = DecrementRate(manIndividual, firstDate);
		decimal womanDecrement = DecrementRate(womanIndividual, firstDate);
		return 1 - IDecrement.UniformDecrementDistribution(manDecrement * manProportion + (1 - manProportion) * womanDecrement, firstDate, secondDate);
	}
	private decimal YearlyUnisexSurvival(TGenderedIndividual manIndividual, TGenderedIndividual womanIndividual, in DateOnly decrementDate, in decimal manProportion)
	{
		decimal manDecrement = DecrementRate(manIndividual, decrementDate);
		decimal womanDecrement = DecrementRate(womanIndividual, decrementDate);
		return 1 - (manDecrement * manProportion + (1 - manProportion) * womanDecrement);
	}
	private decimal GetSurvivalUnisexProbability(TGenderedIndividual manIndividual, TGenderedIndividual womanIndividual, in DateOnly calculationDate, in DateOnly decrementDate, in decimal manProportion)
	{
		int decrementYear = decrementDate.Year;
		if (calculationDate.Year == decrementYear)
			return SurvivalUnisexBetweenIntegerAge(manIndividual, womanIndividual, calculationDate, decrementDate, manProportion);

		decimal survivalProbability = SurvivalUnisexBetweenIntegerAge(manIndividual, womanIndividual, calculationDate, decrementDate, manProportion);
		DateOnly nextDate = calculationDate.FirstDayOfFollowingYear();
		while (nextDate.Year < decrementYear)
		{
			survivalProbability *= YearlyUnisexSurvival(manIndividual, womanIndividual, nextDate, manProportion);
			nextDate = nextDate.AddYears(1);
		}
		survivalProbability *= SurvivalUnisexBetweenIntegerAge(manIndividual, womanIndividual, nextDate, decrementDate, manProportion);
		return survivalProbability;
	}
	private decimal GetDecrementUnisexProbability(TGenderedIndividual manIndividual, TGenderedIndividual womanIndividual, in DateOnly calculationDate, in DateOnly decrementDate, in decimal manProportion) => 1 - GetSurvivalUnisexProbability(manIndividual, womanIndividual, calculationDate, decrementDate, manProportion);
	private decimal GetUnisexProbability(TGenderedIndividual manIndividual, TGenderedIndividual womanIndividual, in DateOnly calculationDate, in DateOnly decrementDate, DecrementOrSurvivalUnisexProbabilityIn decrementOrSurvivalProbability, in decimal manProportion)
	{
		if (calculationDate > decrementDate) throw new ArgumentException($"{nameof(calculationDate)} must be before {nameof(decrementDate)}");
		if (manIndividual.DateOfBirth > calculationDate) throw new ArgumentException($"{nameof(manIndividual.DateOfBirth)} must be before {nameof(calculationDate)}");
		if (!_Table.IsOlderThanLastAgeOfTheTable(manIndividual, decrementDate))
			return decrementOrSurvivalProbability(manIndividual, womanIndividual, calculationDate, decrementDate, manProportion);
		return 0m;
	}
	private decimal[] GetUnisexProbabilities(TGenderedIndividual manIndividual, TGenderedIndividual womanIndividual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalUnisexProbabilityIn decrementOrSurvivalProbability, in decimal manProportion)
	{
		int key = GetHashCode(manIndividual, calculationDate, dates, decrementOrSurvivalProbability, manProportion);
		if (!_Cache.TryGetValue(key, out decimal[]? result))
		{
			if (calculationDate > dates[0]) throw new ArgumentException($"{nameof(calculationDate)} must be before the first date of {nameof(dates)}");
			if (manIndividual.DateOfBirth > calculationDate) throw new ArgumentException($"{nameof(manIndividual.DateOfBirth)} must be before {nameof(calculationDate)}");
			result = UnisexProbabilities(manIndividual, womanIndividual, in calculationDate, dates, decrementOrSurvivalProbability, manProportion);
			_Cache.Set(key, result, GetMemoryCacheEntryOptions());
		}
		return result!;
	}
	private decimal[] UnisexProbabilities(TGenderedIndividual manIndividual, TGenderedIndividual womanIndividual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalUnisexProbabilityIn decrementOrSurvivalProbability, in decimal manProportion)
	{
		int length = dates.Count;
		decimal[] probabilities = new decimal[length];
		DateOnly lastPossibleDecrementDate = _Table.LastPossibleDecrementDate(manIndividual);
		int i = 0;
		probabilities[i] = decrementOrSurvivalProbability(manIndividual, womanIndividual, in calculationDate, dates[i], manProportion);
		i++;
		while (i < length
			&& dates[i] < lastPossibleDecrementDate)
		{
			probabilities[i] = probabilities[i - 1] * decrementOrSurvivalProbability(manIndividual, womanIndividual, dates[i - 1], dates[i], manProportion);
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
		TGenderedIndividual manIndividual = individual.Gender == Gender.Man ? individual : (TGenderedIndividual)individual.AsOtherGender();
		TGenderedIndividual womanIndividual = individual.Gender == Gender.Woman ? individual : (TGenderedIndividual)individual.AsOtherGender();
		return GetUnisexProbabilities(manIndividual, womanIndividual, calculationDate, dates, GetDecrementUnisexProbability, manProportion);
	}
	public decimal[] SurvivalUnisexProbabilities(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, decimal manProportion)
	{
		if (manProportion < 0 || manProportion > 1)
			throw new ArgumentOutOfRangeException(nameof(manProportion), $"The man proportion variable ({manProportion}) must be between 0 and 1 inclusively");
		if (manProportion == Decimal.One)
			return GetProbabilities(individual.Gender == Gender.Man ? individual : (TGenderedIndividual)individual.AsOtherGender(), calculationDate, dates, GetSurvivalProbability);
		if (manProportion == Decimal.Zero)
			return GetProbabilities(individual.Gender == Gender.Woman ? individual : (TGenderedIndividual)individual.AsOtherGender(), calculationDate, dates, GetSurvivalProbability);
		TGenderedIndividual manIndividual = individual.Gender == Gender.Man ? individual : (TGenderedIndividual)individual.AsOtherGender();
		TGenderedIndividual womanIndividual = individual.Gender == Gender.Woman ? individual : (TGenderedIndividual)individual.AsOtherGender();
		return GetUnisexProbabilities(manIndividual, womanIndividual, calculationDate, dates, GetSurvivalUnisexProbability, manProportion);
	}
	public decimal SurvivalUnisexProbability(TGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, decimal manProportion)
	{
		if (manProportion < 0 || manProportion > 1)
			throw new ArgumentOutOfRangeException(nameof(manProportion), $"The man proportion variable ({manProportion}) must be between 0 and 1 inclusively");
		if (manProportion == Decimal.One)
			return GetProbability(individual.Gender == Gender.Man ? individual : (TGenderedIndividual)individual.AsOtherGender(), calculationDate, decrementDate, GetSurvivalProbability);
		if (manProportion == Decimal.Zero)
			return GetProbability(individual.Gender == Gender.Woman ? individual : (TGenderedIndividual)individual.AsOtherGender(), calculationDate, decrementDate, GetSurvivalProbability);
		TGenderedIndividual manIndividual = individual.Gender == Gender.Man ? individual : (TGenderedIndividual)individual.AsOtherGender();
		TGenderedIndividual womanIndividual = individual.Gender == Gender.Woman ? individual : (TGenderedIndividual)individual.AsOtherGender();
		return GetUnisexProbability(manIndividual, womanIndividual, calculationDate, decrementDate, GetSurvivalUnisexProbability, manProportion);
	}
	#endregion
}
