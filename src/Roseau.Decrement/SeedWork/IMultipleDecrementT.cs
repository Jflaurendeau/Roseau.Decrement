using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Decrements.LifeTables;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;

namespace Roseau.Decrement.SeedWork;

public interface IMultipleDecrement<in TIndividual, TDecrementBetweenIntegralAge, TDecrement> : IDecrement<TIndividual>
	where TIndividual : IIndividual
	where TDecrementBetweenIntegralAge : IDecrementBetweenIntegralAgeStrategy
	where TDecrement : IDecrementBetweenIntegralAge<TIndividual, TDecrementBetweenIntegralAge>
{
	#region Associated Single Decrement
	TDecrement? Disability { get; }
	TDecrement? Lapse { get; }
	TDecrement? Mortality { get; }
	#endregion

	#region IDecrement implementation
	decimal IDecrement<TIndividual>.SurvivalProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => IndependentProbability(individual, calculationDate, decrementDate).SurvivalProbability;
	decimal[] IDecrement<TIndividual>.DecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates)
	{
		int numberOfProbabilities = dates.Count;
		decimal[] decrementProbabilities = new decimal[numberOfProbabilities];
		decimal[] survivalProbabilities = DependentProbabilities(individual, calculationDate, dates).SurvivalProbabilities;
		for (int i = 0; i < numberOfProbabilities; i++)
			decrementProbabilities[i] = 1 - survivalProbabilities[i];
		return decrementProbabilities;
	}
	decimal[] IDecrement<TIndividual>.SurvivalProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => DependentProbabilities(individual, calculationDate, dates).SurvivalProbabilities;
	#endregion

	#region Independent probability
	public MultipleDecrementProbability IndependentProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate)
	{
		decimal? disabilityProbability = Disability?.DecrementProbability(individual, calculationDate, decrementDate);
		decimal? lapseProbability = Lapse?.DecrementProbability(individual, calculationDate, decrementDate);
		decimal? mortalityProbability = Mortality?.DecrementProbability(individual, calculationDate, decrementDate);
		decimal survivalProbability = (1 - disabilityProbability ?? 0m) * (1 - lapseProbability ?? 0m) * (1 - mortalityProbability ?? 0m);
		return new (survivalProbability, disabilityProbability, lapseProbability, mortalityProbability);
	}
	#endregion

	#region Dependent probability
	public MultipleDecrementProbability DependentProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);
	public MultipleDecrementProbabilities DependentProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates);
	#endregion

}
