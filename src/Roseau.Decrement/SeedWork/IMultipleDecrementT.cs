using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Decrements.LifeTables;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;

namespace Roseau.Decrement.SeedWork;

public interface IMultipleDecrement<in TIndividual, TDecrementBetweenIntegralAge, TDecrement> : IDecrement<TIndividual>
	where TIndividual : IIndividual
	where TDecrementBetweenIntegralAge : IDecrementBetweenIntegralAgeStrategy, new()
	where TDecrement : IDecrement<TIndividual>, IDecrementBetweenIntegralAge<TDecrementBetweenIntegralAge>
{
	#region Associated Single Decrement
	TDecrement? Disability { get; }
	TDecrement? Lapse { get; }
	TDecrement? Mortality { get; }
	#endregion

	decimal IDecrement<TIndividual>.SurvivalProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate)
	{
		decimal probability = IndependentDisabilitySurvivalProbability(individual, calculationDate, decrementDate) ?? 1m;
		probability *= IndependentLapseSurvivalProbability(individual, calculationDate, decrementDate) ?? 1m;
		probability *= IndependentMortalitySurvivalProbability(individual, calculationDate, decrementDate) ?? 1m;
		return probability;
	}

	#region Independent probability
	public MultipleDecrementProbability IndependentProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate)
	{
		decimal? disabilityProbability = IndependentDisabilityDecrementProbability(individual, calculationDate, decrementDate);
		decimal? lapseProbability = IndependentLapseDecrementProbability(individual, calculationDate, decrementDate);
		decimal? mortalityProbability = IndependentMortalityDecrementProbability(individual, calculationDate, decrementDate);
		decimal survivalProbability = (1 - disabilityProbability ?? 0m) * (1 - lapseProbability ?? 0m) * (1 - mortalityProbability ?? 0m);
		return new MultipleDecrementProbability(survivalProbability, disabilityProbability, lapseProbability, mortalityProbability);
	}
	public decimal? IndependentDisabilitySurvivalProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => Disability?.SurvivalProbability(individual, calculationDate, decrementDate);
	public decimal[]? IndependentDisabilitySurvivalProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => Disability?.SurvivalProbabilities(individual, calculationDate, dates);
	public decimal? IndependentDisabilityDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => Disability?.DecrementProbability(individual, calculationDate, decrementDate);
	public decimal[]? IndependentDisabilityDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => Disability?.DecrementProbabilities(individual, calculationDate, dates);
	public decimal? IndependentLapseSurvivalProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => Lapse?.SurvivalProbability(individual, calculationDate, decrementDate);
	public decimal[]? IndependentLapseSurvivalProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => Lapse?.SurvivalProbabilities(individual, calculationDate, dates);
	public decimal? IndependentLapseDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => Lapse?.DecrementProbability(individual, calculationDate, decrementDate);
	public decimal[]? IndependentLapseDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => Lapse?.DecrementProbabilities(individual, calculationDate, dates);
	public decimal? IndependentMortalitySurvivalProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => Mortality?.SurvivalProbability(individual, calculationDate, decrementDate);
	public decimal[]? IndependentMortalitySurvivalProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => Mortality?.SurvivalProbabilities(individual, calculationDate, dates);
	public decimal? IndependentMortalityDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => Mortality?.DecrementProbability(individual, calculationDate, decrementDate);
	public decimal[]? IndependentMortalityDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => Mortality?.DecrementProbabilities(individual, calculationDate, dates);
	#endregion

	#region Dependent probability
	public MultipleDecrementProbability DependentProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);
	public decimal? DependentDisabilityDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);
	public decimal[]? DependentDisabilityDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates);
	public decimal? DependentLapseDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);
	public decimal[]? DependentLapseDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates);
	public decimal? DependentMortalityDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);
	public decimal[]? DependentMortalityDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates);
	#endregion

}
