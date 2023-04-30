using Roseau.DateHelpers;
using Roseau.Mathematics;

namespace Roseau.Decrement.SeedWork;

public interface IDecrement
{
	#region Fractional Age calculation method
	/// <summary>
	/// Determine the death probability between two dates, where the death probability is uniform.
	/// </summary>
	/// <param name="decrementProbability">The probability of death</param>
	/// <param name="firstDate">First Date</param>
	/// <param name="secondDate">Second Date</param>
	/// <returns>Probability of death</returns>
	static public decimal UniformDecrementDistribution(decimal decrementProbability, DateOnly firstDate, DateOnly secondDate)
	{
		if (secondDate < firstDate) throw new System.ArgumentException("firstDate must be before secondDate when using UniformDeathDistribution method");
		DateOnly endOfYearDate = new(firstDate.Year + 1, 1, 1);
		DateOnly lastDate = endOfYearDate < secondDate ? endOfYearDate : secondDate;
		decimal deathFirstPartOfYear = firstDate.TimeElapsedFromStartOfYear() * decrementProbability;
		decimal deathSecondPartOfYear = firstDate.DifferenceOfTimeElapsedFromStartOfYear(lastDate) * decrementProbability;
		return deathSecondPartOfYear / (1 - deathFirstPartOfYear);
	}
	/// <summary>
	/// Determine the survival probability between two dates, where the death probability is uniform.
	/// </summary>
	/// <param name="decrementProbability">The probability of death</param>
	/// <param name="firstDate">First Date</param>
	/// <param name="secondDate">Second Date</param>
	/// <returns>Probability of survival</returns>
	static public decimal UniformSurvivalDistribution(decimal decrementProbability, DateOnly firstDate, DateOnly secondDate) => 1 - UniformDecrementDistribution(decrementProbability, firstDate, secondDate);
	/// <summary>
	/// Determine the death probability between two dates, where the survival probability is use a constant force of mortality.
	/// </summary>
	/// <param name="decrementProbability">The probability of death</param>
	/// <param name="firstDate">First Date</param>
	/// <param name="secondDate">Second Date</param>
	/// <returns>Probability of death</returns>
	static public decimal ConstantForceDecrement(decimal decrementProbability, DateOnly firstDate, DateOnly secondDate) => 1 - ConstantForceSurvival(decrementProbability, firstDate, secondDate);
	/// <summary>
	/// Determine the Survival probability between two dates, where the survival probability is use a constant force of mortality.
	/// </summary>
	/// <param name="decrementProbability">The probability of death</param>
	/// <param name="firstDate">First Date</param>
	/// <param name="secondDate">Second Date</param>
	/// <returns>Probability of survival</returns>
	static public decimal ConstantForceSurvival(decimal decrementProbability, DateOnly firstDate, DateOnly secondDate)
	{
		if (secondDate < firstDate) throw new System.ArgumentException("firstDate must be before secondDate when using SurvivalConstantForceMortality method");
		DateOnly endOfYearDate = new(firstDate.Year + 1, 1, 1);
		DateOnly lastDate = endOfYearDate < secondDate ? endOfYearDate : secondDate;
		return Maths.Pow(1 - decrementProbability, firstDate.DifferenceOfTimeElapsedFromStartOfYear(lastDate));
	}
	#endregion
}
