namespace Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;

public interface IDecrementBetweenIntegralAge<out TDecrementBetweenIntegralAge>
	where TDecrementBetweenIntegralAge : IDecrementBetweenIntegralAgeStrategy, new()
{
	public TDecrementBetweenIntegralAge DecrementBetweenIntegralAge { get; }
}
