using Roseau.Decrement.Aggregates.Decrements.LifeTables;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;

public class ConstantForceOfMortalityStrategy : IDecrementBetweenIntegralAgeStrategy
{
	public decimal SurvivalProbability(DateOnly firstDate, DateOnly secondDate, decimal decrementProbability) => IDecrement.ConstantForceSurvival(firstDate, secondDate, decrementProbability);
	public MultipleDecrementProbability ThreeDecrementsProbability(DateOnly firstDate, DateOnly secondDate, decimal? disabilityDecrementRate, decimal? lapseDecrementRate, decimal? mortalityDecrementRate) => IDecrement.ConstantForceDecrement(firstDate, secondDate, disabilityDecrementRate, lapseDecrementRate, mortalityDecrementRate);
	public MultipleDecrementProbability ThreeDecrementsRate(decimal? disabilityDecrementRate, decimal? lapseDecrementRate, decimal? mortalityDecrementRate) => IDecrement.ConstantForceDecrementRate(disabilityDecrementRate, lapseDecrementRate, mortalityDecrementRate);
}
