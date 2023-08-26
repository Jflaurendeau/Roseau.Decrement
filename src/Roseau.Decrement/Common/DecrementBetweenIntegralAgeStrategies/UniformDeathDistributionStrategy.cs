using Roseau.Decrement.Aggregates.Decrements.LifeTables;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;

public class UniformDeathDistributionStrategy : IDecrementBetweenIntegralAgeStrategy
{
	public decimal SurvivalProbability(DateOnly firstDate, DateOnly secondDate, decimal decrementProbability) => IDecrement.UniformSurvivalDistribution(firstDate, secondDate, decrementProbability);
	public MultipleDecrementProbability ThreeDecrementsProbability(DateOnly firstDate, DateOnly secondDate, decimal? disabilityDecrementRate, decimal? lapseDecrementRate, decimal? mortalityDecrementRate) => IDecrement.UniformDecrementDistribution(firstDate, secondDate, disabilityDecrementRate, lapseDecrementRate, mortalityDecrementRate);
	public MultipleDecrementProbability ThreeDecrementsRate(decimal? disabilityDecrementRate, decimal? lapseDecrementRate, decimal? mortalityDecrementRate) => IDecrement.UniformDecrementDistributionDecrementRates(disabilityDecrementRate, lapseDecrementRate, mortalityDecrementRate);
}
