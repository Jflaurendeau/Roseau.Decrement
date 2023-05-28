using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;

public class ConstantForceOfMortalityStrategy : IDecrementBetweenIntegralAgeStrategy
{
	public decimal SurvivalProbability(decimal decrementProbability, DateOnly firstDate, DateOnly secondDate) => IDecrement.ConstantForceSurvival(decrementProbability, firstDate, secondDate);
}
