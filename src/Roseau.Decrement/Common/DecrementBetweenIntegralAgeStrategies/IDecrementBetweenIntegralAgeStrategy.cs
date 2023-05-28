namespace Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;

public interface IDecrementBetweenIntegralAgeStrategy
{
	decimal DecrementProbability(decimal decrementProbability, DateOnly firstDate, DateOnly secondDate) => 1 - SurvivalProbability(decrementProbability, firstDate, secondDate);
	decimal SurvivalProbability(decimal decrementProbability, DateOnly firstDate, DateOnly secondDate);
}
