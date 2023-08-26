using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;

public interface IDecrementBetweenIntegralAge<in TIndividual, out TDecrementBetweenIntegralAge> : IDecrement<TIndividual>
	where TIndividual : IIndividual
	where TDecrementBetweenIntegralAge : IDecrementBetweenIntegralAgeStrategy
{
	public TDecrementBetweenIntegralAge DecrementBetweenIntegralAge { get; }
}
