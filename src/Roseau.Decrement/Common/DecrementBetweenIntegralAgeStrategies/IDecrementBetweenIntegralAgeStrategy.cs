using Roseau.Decrement.Aggregates.Decrements.LifeTables;

namespace Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;

public interface IDecrementBetweenIntegralAgeStrategy
{
	decimal DecrementProbability(DateOnly firstDate, DateOnly secondDate, decimal decrementProbability) => 1 - SurvivalProbability(firstDate, secondDate, decrementProbability);
	MultipleDecrementProbability ThreeDecrementsProbability(DateOnly firstDate, DateOnly secondDate, decimal? disabilityDecrementRate, decimal? lapseDecrementRate, decimal? mortalityDecrementRate);
	MultipleDecrementProbability ThreeDecrementsRate(decimal? disabilityDecrementRate, decimal? lapseDecrementRate, decimal? mortalityDecrementRate);
	decimal SurvivalProbability(DateOnly firstDate, DateOnly secondDate, decimal decrementProbability);
}
