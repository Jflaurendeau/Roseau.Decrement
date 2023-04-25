using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Decrements.Adjustments;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public abstract class Cpm2014 : IDecrement<IGenderedIndividual>
{
	#region Fields
	private const int TIMESPAN = 300;
	private readonly IImprovement<IGenderedIndividual> _ImprovementScale;
	private readonly IAdjustment<IGenderedIndividual> _Adjustment;
	private readonly IMemoryCache _Cache;
	#endregion

	#region Constructors
	protected Cpm2014() : this(default!, default!, default!) {	}
	protected Cpm2014(IImprovement<IGenderedIndividual> improvementScale) : this(improvementScale, default!, default!) {	}
	protected Cpm2014(IAdjustment<IGenderedIndividual> adjustment) : this(default!, adjustment, default!) {	}
	protected Cpm2014(IMemoryCache memoryCache) : this(default!, default!, memoryCache) { }
	protected Cpm2014(IImprovement<IGenderedIndividual> improvementScale, IAdjustment<IGenderedIndividual> adjustment) : this(improvementScale, adjustment, default!) { }
	protected Cpm2014(IImprovement<IGenderedIndividual> improvementScale, IMemoryCache memoryCache) : this(improvementScale, default!, memoryCache) { }
	protected Cpm2014(IAdjustment<IGenderedIndividual> adjustment, IMemoryCache memoryCache) : this(default!, adjustment, memoryCache) { }
	protected Cpm2014(IImprovement<IGenderedIndividual> improvementScale, IAdjustment<IGenderedIndividual> adjustment, IMemoryCache memoryCache)
	{
		_ImprovementScale = improvementScale ?? IImprovement<IGenderedIndividual>.Default;
		_Adjustment = adjustment ?? IAdjustment<IGenderedIndividual>.Default;
		_Cache = memoryCache ?? new MemoryCache(new MemoryCacheOptions() { SizeLimit = 5000, });
	}
	#endregion
	
	protected delegate decimal DecrementOrSurvivalProbabilityIn(IGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, IImprovement<IGenderedIndividual> improvementScale, IAdjustment<IGenderedIndividual> adjustment, decimal[][] decrementTable);

	#region private methods
	private static MemoryCacheEntryOptions GetMemoryCacheEntryOptions() => new MemoryCacheEntryOptions().SetSize(1)
																								 .SetSlidingExpiration(TimeSpan.FromSeconds(TIMESPAN))
																								 .SetPriority(CacheItemPriority.Low);
	private static bool IsOlderThanLastAgeOfTheTable(IGenderedIndividual individual, DateOnly calculationDate)
	{
		int age = individual.DateOfBirth.AgeNearestBirthday(new DateOnly(calculationDate.Year, 1, 1));
		return age > LASTAGE;
	}
	private static int AgeLimitedByLifeTable(IGenderedIndividual individual, DateOnly calculationDate)
	{
		int age = individual.DateOfBirth.AgeNearestBirthday(new DateOnly(calculationDate.Year, 1, 1));

		return Math.Min(Math.Max(FIRSTAGE, age), LASTAGE);
	}
	private static DateOnly DecrementDateLimitedByLifeTable(IGenderedIndividual individual, DateOnly decrementDate)
	{
		if (!IsOlderThanLastAgeOfTheTable(individual, decrementDate))
			return decrementDate;
		return LastPossibleDecrementDate(individual);

	}
	private static DateOnly LastPossibleDecrementDate(IGenderedIndividual individual)
	{
		DateOnly date = new(individual.DateOfBirth.Year + 1, 1, 1);
		date = individual.DateOfBirth.AgeNearestBirthday(date) == 0
			? date.AddYears(LASTAGE + 1)
			: date.AddYears(LASTAGE);
		return date;
	}
	private static decimal SurvivalBetweenIntegerAge(IGenderedIndividual individual, in DateOnly firstDate, in DateOnly secondDate, IImprovement<IGenderedIndividual> improvementScale, IAdjustment<IGenderedIndividual> adjustment, decimal[][] decrementTable)
	{
		int ageForLifeTable = AgeLimitedByLifeTable(individual, new DateOnly(firstDate.Year, 1, 1));
		if (ageForLifeTable == LASTAGE)
			return 1 - IDecrement.UniformDecrementDistribution(1, firstDate, secondDate);

		decimal deathProbability = decrementTable[(int)individual.Gender][ageForLifeTable - FIRSTAGE];
		decimal improvementFactor = improvementScale.ImprovementFactor(individual, BASEYEAR, firstDate);
		decimal adjustmentFactor = adjustment.AdjustmentFactor(individual);

		return 1 - IDecrement.UniformDecrementDistribution(adjustmentFactor * deathProbability * improvementFactor, firstDate, secondDate);
	}
	private static decimal YearlySurvival(IGenderedIndividual individual, in DateOnly decrementDate, IImprovement<IGenderedIndividual> improvementScale, IAdjustment<IGenderedIndividual> adjustment, decimal[][] decrementTable)
	{
		int ageForLifeTable = AgeLimitedByLifeTable(individual, decrementDate);
		if (ageForLifeTable == LASTAGE)
			return 0m;

		decimal deathProbability = decrementTable[(int)individual.Gender][ageForLifeTable - FIRSTAGE];
		decimal improvementFactor = improvementScale.ImprovementFactor(individual, BASEYEAR, decrementDate);
		decimal adjustmentFactor = adjustment.AdjustmentFactor(individual);
		return 1 - deathProbability * improvementFactor * adjustmentFactor;
	}
	protected decimal GetProbability(IGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, decimal[][] decrementTable, in DecrementOrSurvivalProbabilityIn decrementOrSurvivalProbability)
	{
		DateOnly decrementDateLimitedByLifeTable = DecrementDateLimitedByLifeTable(individual, decrementDate);
		if (calculationDate > decrementDate) throw new ArgumentException($"{nameof(calculationDate)} must be before {nameof(decrementDate)}");
		if (individual.DateOfBirth > calculationDate) throw new ArgumentException($"{nameof(individual.DateOfBirth)} must be before {nameof(calculationDate)}");
		if (!IsOlderThanLastAgeOfTheTable(individual, decrementDate))
			return decrementOrSurvivalProbability(individual, calculationDate, decrementDateLimitedByLifeTable, _ImprovementScale, _Adjustment, decrementTable);
		if (decrementOrSurvivalProbability == GetSurvivalProbability)
			return 0m;
		return 1m;
	}
	protected decimal[] GetProbabilities(IGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, decimal[][] decrementTable, in DecrementOrSurvivalProbabilityIn decrementOrSurvivalProbability)
	{
		int key = HashCode.Combine(decrementTable, individual.Gender, individual.DateOfBirth, calculationDate, dates, decrementOrSurvivalProbability);
		if (!_Cache.TryGetValue(key, out decimal[]? result))
		{
			if (calculationDate > dates[0]) throw new ArgumentException($"{nameof(calculationDate)} must be before the first date of {nameof(dates)}");
			if (individual.DateOfBirth > calculationDate) throw new ArgumentException($"{nameof(individual.DateOfBirth)} must be before {nameof(calculationDate)}");
			result = Probabilities(individual, in calculationDate, dates, _ImprovementScale, _Adjustment, decrementTable, in decrementOrSurvivalProbability);
			_Cache.Set(key, result, GetMemoryCacheEntryOptions());
		}
		return result!;
	}
	protected static decimal GetSurvivalProbability(IGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, IImprovement<IGenderedIndividual> improvementScale, IAdjustment<IGenderedIndividual> adjustment, decimal[][] decrementTable)
	{
		int decrementYear = decrementDate.Year;
		if (calculationDate.Year == decrementYear)
			return SurvivalBetweenIntegerAge(individual, calculationDate, decrementDate, improvementScale, adjustment, decrementTable);

		decimal survivalProbability = SurvivalBetweenIntegerAge(individual, calculationDate, decrementDate, improvementScale, adjustment, decrementTable);
		DateOnly nextDate = calculationDate.FirstDayOfFollowingYear();
		while (nextDate.Year < decrementYear)
		{
			survivalProbability *= YearlySurvival(individual, nextDate, improvementScale, adjustment, decrementTable);
			nextDate = nextDate.AddYears(1);
		}
		survivalProbability *= SurvivalBetweenIntegerAge(individual, nextDate, decrementDate, improvementScale, adjustment, decrementTable);
		return survivalProbability;
	}
	protected static decimal GetDecrementProbability(IGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, IImprovement<IGenderedIndividual> improvementScale, IAdjustment<IGenderedIndividual> adjustment, decimal[][] decrementTable) => 1 - GetSurvivalProbability(individual, calculationDate, decrementDate, improvementScale, adjustment, decrementTable);
	private static decimal[] Probabilities(IGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, IImprovement<IGenderedIndividual> improvementScale, IAdjustment<IGenderedIndividual> adjustment, decimal[][] decrementTable,  in DecrementOrSurvivalProbabilityIn decrementOrSurvivalProbability)
	{
		int length = dates.Count;
		decimal[] probabilities = new decimal[length];
		DateOnly lastPossibleDecrementDate = LastPossibleDecrementDate(individual);
		int i = 0;
		probabilities[i] = decrementOrSurvivalProbability(individual, in calculationDate, dates[i], improvementScale, adjustment, decrementTable);
		i++;
		while (i < length
			&& dates[i] < lastPossibleDecrementDate)
		{
			probabilities[i] = probabilities[i - 1] * decrementOrSurvivalProbability(individual, dates[i - 1], dates[i], improvementScale, adjustment, decrementTable);
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

	public abstract decimal SurvivalProbability(IGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);
	public abstract decimal[] SurvivalProbabilities(IGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates);
	public abstract decimal[] DecrementProbabilities(IGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates);

	#region The CPM2014 Tables
	// The CPM 2014 Tables were the results of studies commissioned by the Canadian Institute of Actuaries.
	// The mortality tables are mostly based on the results of the Registered Pension Plan study, prepared by Robert C.W. Howard, FCIA, FSA. Altough, the data collection and validation were made by MIB Solutions. 
	// The improvement scales are mostly based on the results of the Canadian Pension Plan and the Québec Pension Plan study, prepared by Louis Adam, FCIA, FSA.
	// The well written and easy reading report (214013) can be found on the Canadian Institute of Actuaries website: https://www.cia-ica.ca/publications/publication-details/214013
	// Reference:
	// Canadian Institute of Actuaries. 2014. Final Report: Canadian Pensioners’ Mortalitys. Pension Experience Subcommittee of the Research Committee, Canadian Institute of Actuaries. https://www.cia-ica.ca/publications/publication-details/214013
	private const int FIRSTAGE = 18;
	private const int LASTAGE = 115;
	private const int BASEYEAR = 2014;
	#endregion
}
