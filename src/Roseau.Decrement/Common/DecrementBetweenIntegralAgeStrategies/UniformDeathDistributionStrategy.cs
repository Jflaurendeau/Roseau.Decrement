using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;

public class UniformDeathDistributionStrategy : IDecrementBetweenIntegralAgeStrategy
{
	public decimal SurvivalProbability(decimal decrementProbability, DateOnly firstDate, DateOnly secondDate) => IDecrement.UniformSurvivalDistribution(decrementProbability, firstDate, secondDate);
}
