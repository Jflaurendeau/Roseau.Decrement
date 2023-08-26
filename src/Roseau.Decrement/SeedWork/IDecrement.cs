using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Decrements.LifeTables;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Mathematics;

namespace Roseau.Decrement.SeedWork;

public interface IDecrement
{
	#region Fractional Age calculation method
	/// <summary>
	/// Determine the death probability between two dates, where the death probability is uniform.
	/// </summary>
	/// <param name="firstDate">First Date</param>
	/// <param name="secondDate">Second Date</param>
	/// <param name="decrementProbability">The probability of death</param>
	/// <returns>Probability of death</returns>
	static public decimal UniformDecrementDistribution(DateOnly firstDate, DateOnly secondDate, decimal decrementProbability)
	{
		if (secondDate < firstDate) throw new System.ArgumentException("firstDate must be before secondDate when using UniformDeathDistribution method");
		DateOnly endOfYearDate = new(firstDate.Year + 1, 1, 1);
		DateOnly lastDate = endOfYearDate < secondDate ? endOfYearDate : secondDate;
		decimal deathFirstPartOfYear = firstDate.TimeElapsedFromStartOfYear() * decrementProbability;
		decimal deathSecondPartOfYear = firstDate.DifferenceOfTimeElapsedFromStartOfYear(lastDate) * decrementProbability;
		return deathSecondPartOfYear / (1 - deathFirstPartOfYear);
	}
	/// <summary>
	/// Determine the death probability between two dates, where the all decrement probabilities is uniform. From independents decrements (each are UDD) to dependent decrements.
	/// </summary>
	/// <param name="firstDate">First Date</param>
	/// <param name="secondDate">Second Date</param>
	/// <param name="disabilityDecrementRate">Disability Decrement Probability</param>
	/// <param name="lapseDecrementRate">Lapse Decrement Probability</param>
	/// <param name="mortalityDecrementRate">Mortality Decrement Probability</param>
	/// <returns>Probability of death</returns>
	static public MultipleDecrementProbability UniformDecrementDistribution(DateOnly firstDate, DateOnly secondDate, decimal? disabilityDecrementRate, decimal? lapseDecrementRate, decimal? mortalityDecrementRate)
	{
		if (secondDate < firstDate) throw new System.ArgumentException($"{nameof(firstDate)} must be before {nameof(secondDate)} when using {nameof(UniformDecrementDistribution)} method");
		DateOnly endOfYearDate = new(firstDate.Year + 1, 1, 1);
		DateOnly lastDate = endOfYearDate < secondDate ? endOfYearDate : secondDate;
		decimal firstPartOfYear = firstDate.TimeElapsedFromStartOfYear();
		decimal secondPartOfYear = firstDate.DifferenceOfTimeElapsedFromStartOfYear(lastDate);

		var survivalFirstPartOfYear = (1 - (disabilityDecrementRate ?? 0m) * firstPartOfYear) * (1 - (lapseDecrementRate ?? 0m) * firstPartOfYear) * (1 - (mortalityDecrementRate ?? 0m) * firstPartOfYear);
		var firstDecrementProbability = UniformDecrementDistributionThreeDecrements(secondPartOfYear, survivalFirstPartOfYear, disabilityDecrementRate, lapseDecrementRate, mortalityDecrementRate);
		var secondDecrementProbability = UniformDecrementDistributionThreeDecrements(secondPartOfYear, survivalFirstPartOfYear, lapseDecrementRate, disabilityDecrementRate, mortalityDecrementRate);
		var thirdDecrementProbability = UniformDecrementDistributionThreeDecrements(secondPartOfYear, survivalFirstPartOfYear, mortalityDecrementRate, disabilityDecrementRate, lapseDecrementRate);
		var survivalProbability = 1 - (firstDecrementProbability ?? 0m) - (secondDecrementProbability ?? 0m) - (thirdDecrementProbability ?? 0m);
		return new(survivalProbability, firstDecrementProbability, secondDecrementProbability, thirdDecrementProbability);
	}
	static public MultipleDecrementProbability UniformDecrementDistributionDecrementRates(decimal? disabilityDecrementRate, decimal? lapseDecrementRate, decimal? mortalityDecrementRate)
	{
		var firstDecrementProbability = UniformDecrementDistributionThreeDecrementsCompleteYear(disabilityDecrementRate, lapseDecrementRate, mortalityDecrementRate);
		var secondDecrementProbability = UniformDecrementDistributionThreeDecrementsCompleteYear(lapseDecrementRate, disabilityDecrementRate, mortalityDecrementRate);
		var thirdDecrementProbability = UniformDecrementDistributionThreeDecrementsCompleteYear(mortalityDecrementRate, disabilityDecrementRate, lapseDecrementRate);
		var survivalProbability = 1 - (firstDecrementProbability ?? 0m) - (secondDecrementProbability ?? 0m) - (thirdDecrementProbability ?? 0m);
		return new(survivalProbability, firstDecrementProbability, secondDecrementProbability, thirdDecrementProbability);
	}
	/// <summary>
	/// Determine the survival probability between two dates, where the death probability is uniform.
	/// </summary>
	/// <param name="firstDate">First Date</param>
	/// <param name="secondDate">Second Date</param>
	/// <param name="decrementProbability">The probability of death</param>
	/// <returns>Probability of survival</returns>
	static public decimal UniformSurvivalDistribution(DateOnly firstDate, DateOnly secondDate, decimal decrementProbability) => 1 - UniformDecrementDistribution(firstDate, secondDate, decrementProbability);
	/// <summary>
	/// Determine the Survival probability between two dates, where the survival probability is use a constant force of mortality.
	/// </summary>
	/// <param name="firstDate">First Date</param>
	/// <param name="secondDate">Second Date</param>
	/// <param name="decrementProbability">The probability of death</param>
	/// <returns>Probability of survival</returns>
	static public decimal ConstantForceSurvival(DateOnly firstDate, DateOnly secondDate, decimal decrementProbability)
	{
		if (secondDate < firstDate) throw new System.ArgumentException("firstDate must be before secondDate when using SurvivalConstantForceMortality method");
		DateOnly endOfYearDate = new(firstDate.Year + 1, 1, 1);
		DateOnly lastDate = endOfYearDate < secondDate ? endOfYearDate : secondDate;
		return Maths.Pow(1 - decrementProbability, firstDate.DifferenceOfTimeElapsedFromStartOfYear(lastDate));
	}
	/// <summary>
	/// Determine the death probability between two dates, where the survival probability is use a constant force of mortality.
	/// </summary>
	/// <param name="firstDate">First Date</param>
	/// <param name="secondDate">Second Date</param>
	/// <param name="decrementProbability">The probability of death</param>
	/// <returns>Probability of death</returns>
	static public decimal ConstantForceDecrement(DateOnly firstDate, DateOnly secondDate, decimal decrementProbability) => 1 - ConstantForceSurvival(firstDate, secondDate, decrementProbability);
	static public MultipleDecrementProbability ConstantForceDecrement(DateOnly firstDate, DateOnly secondDate, decimal? disabilityDecrementRate, decimal? lapseDecrementRate, decimal? mortalityDecrementRate)
	{
		if (secondDate < firstDate) throw new System.ArgumentException($"{nameof(firstDate)} must be before {nameof(secondDate)} when using {nameof(IDecrement.ConstantForceDecrement)} method");
		DateOnly endOfYearDate = new(firstDate.Year + 1, 1, 1);
		DateOnly lastDate = endOfYearDate < secondDate ? endOfYearDate : secondDate;

		decimal differenceOfTimeElapsedFromStartOfYear = firstDate.DifferenceOfTimeElapsedFromStartOfYear(lastDate);
		decimal survivalProbabilityForCompleteYear = (1 - disabilityDecrementRate ?? 0m) * (1 - lapseDecrementRate ?? 0m) * (1 - mortalityDecrementRate ?? 0m);
		decimal decrementProbabilityForCompleteYear = 1 - survivalProbabilityForCompleteYear;
		decimal survivalProbabilityForElapsedTime = Maths.Pow(survivalProbabilityForCompleteYear, differenceOfTimeElapsedFromStartOfYear);

		//Shortcut when there is at least one independent decrement that has 100% probability of decrement
		if (survivalProbabilityForCompleteYear == 1m)
		{
			decimal totalDecrement = (disabilityDecrementRate ?? 0m) + (lapseDecrementRate ?? 0m) + (mortalityDecrementRate ?? 0m);
			return new(survivalProbabilityForElapsedTime, 
				disabilityDecrementRate.HasValue ? disabilityDecrementRate.Value / totalDecrement : disabilityDecrementRate, 
				lapseDecrementRate.HasValue ? lapseDecrementRate.Value / totalDecrement : lapseDecrementRate, 
				mortalityDecrementRate.HasValue ? mortalityDecrementRate.Value / totalDecrement : mortalityDecrementRate);
		}

		decimal? dependentDisabilityProbability = ConstantForceDecrementOneOfThreeDrecrementsProbability(survivalProbabilityForCompleteYear, decrementProbabilityForCompleteYear, survivalProbabilityForElapsedTime, disabilityDecrementRate);
		decimal? dependentLapseProbability = ConstantForceDecrementOneOfThreeDrecrementsProbability(survivalProbabilityForCompleteYear, decrementProbabilityForCompleteYear, survivalProbabilityForElapsedTime, lapseDecrementRate);
		decimal? dependentMortalityProbability = ConstantForceDecrementOneOfThreeDrecrementsProbability(survivalProbabilityForCompleteYear, decrementProbabilityForCompleteYear, survivalProbabilityForElapsedTime, mortalityDecrementRate);

		return new(survivalProbabilityForElapsedTime, dependentDisabilityProbability, dependentLapseProbability, dependentMortalityProbability);
	}
	static public MultipleDecrementProbability ConstantForceDecrementRate(decimal? disabilityDecrementRate, decimal? lapseDecrementRate, decimal? mortalityDecrementRate)
	{
		decimal survivalProbabilityForCompleteYear = (1 - disabilityDecrementRate ?? 0m) * (1 - lapseDecrementRate ?? 0m) * (1 - mortalityDecrementRate ?? 0m);
		decimal decrementProbabilityForCompleteYear = 1 - survivalProbabilityForCompleteYear;

		//Shortcut when there is at least one independent decrement that has 100% probability of decrement
		if (survivalProbabilityForCompleteYear == 0m)
		{
			decimal totalDecrement = (disabilityDecrementRate ?? 0m) + (lapseDecrementRate ?? 0m) + (mortalityDecrementRate ?? 0m);
			return new(survivalProbabilityForCompleteYear,
				disabilityDecrementRate.HasValue ? disabilityDecrementRate.Value / totalDecrement : disabilityDecrementRate,
				lapseDecrementRate.HasValue ? lapseDecrementRate.Value / totalDecrement : lapseDecrementRate,
				mortalityDecrementRate.HasValue ? mortalityDecrementRate.Value / totalDecrement : mortalityDecrementRate);
		}
		if (survivalProbabilityForCompleteYear == 1m)
			return new(survivalProbabilityForCompleteYear, disabilityDecrementRate, lapseDecrementRate, mortalityDecrementRate);

		decimal? dependentDisabilityProbability = ConstantForceDecrementOneOfThreeDrecrementsRate(survivalProbabilityForCompleteYear, decrementProbabilityForCompleteYear, disabilityDecrementRate);
		decimal? dependentLapseProbability = ConstantForceDecrementOneOfThreeDrecrementsRate(survivalProbabilityForCompleteYear, decrementProbabilityForCompleteYear, lapseDecrementRate);
		decimal? dependentMortalityProbability = ConstantForceDecrementOneOfThreeDrecrementsRate(survivalProbabilityForCompleteYear, decrementProbabilityForCompleteYear, mortalityDecrementRate);

		return new(survivalProbabilityForCompleteYear, dependentDisabilityProbability, dependentLapseProbability, dependentMortalityProbability);
	}



	#region Uniform Decrement Distribution
	static private decimal? UniformDecrementDistributionThreeDecrements(in decimal secondPartOfYear,
		in decimal survivalProbability,
		in decimal? neededProbabilityDecrementRate,
		in decimal? firstOtherDecrementRate,
		in decimal? secondOtherDecrementRate) =>
		neededProbabilityDecrementRate is null
		? neededProbabilityDecrementRate
		: neededProbabilityDecrementRate
		* (secondPartOfYear - secondPartOfYear * secondPartOfYear * ((firstOtherDecrementRate ?? 0m) + (secondOtherDecrementRate ?? 0m)) / 2
		+ secondPartOfYear * secondPartOfYear * secondPartOfYear * (firstOtherDecrementRate ?? 0m) * (secondOtherDecrementRate ?? 0m)/ 3) / survivalProbability;
	static private decimal? UniformDecrementDistributionThreeDecrementsCompleteYear(in decimal? neededProbabilityDecrementRate,
		in decimal? firstOtherDecrementRate,
		in decimal? secondOtherDecrementRate) =>
		neededProbabilityDecrementRate is null
		? neededProbabilityDecrementRate
		: neededProbabilityDecrementRate
		* (1 - ((firstOtherDecrementRate ?? 0m) + (secondOtherDecrementRate ?? 0m)) / 2
		+ (firstOtherDecrementRate ?? 0m) * (secondOtherDecrementRate ?? 0m) / 3);
	#endregion

	#region Constant force of Mortality
	static private decimal? ConstantForceDecrementOneOfThreeDrecrementsProbability(in decimal survivalProbabilityForCompleteYear,
		in decimal decrementProbabilityForCompleteYear,
		in decimal survivalProbabilityForElapsedTime,
		in decimal? neededProbabilityDecrementRate)
	{
		if (neededProbabilityDecrementRate.HasValue && neededProbabilityDecrementRate > 0m)
		{
			decimal dependentDecrementRate = decrementProbabilityForCompleteYear * Maths.Log(1 - neededProbabilityDecrementRate.Value) / Maths.Log(survivalProbabilityForCompleteYear);
			return dependentDecrementRate / decrementProbabilityForCompleteYear * (1 - survivalProbabilityForElapsedTime);
		}
		return neededProbabilityDecrementRate;
	}
	static private decimal? ConstantForceDecrementOneOfThreeDrecrementsRate(in decimal survivalProbabilityForCompleteYear,
		in decimal decrementProbabilityForCompleteYear,
		in decimal? neededProbabilityDecrementRate)
	{
		if (neededProbabilityDecrementRate.HasValue && neededProbabilityDecrementRate > 0m)
			return decrementProbabilityForCompleteYear * Maths.Log(1 - neededProbabilityDecrementRate.Value) / Maths.Log(survivalProbabilityForCompleteYear);
		return neededProbabilityDecrementRate;
	}
	#endregion
	#endregion
}
