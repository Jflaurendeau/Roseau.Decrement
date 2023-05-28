using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public class AssociateSingleDecrementUniformDeathDistribution<TIndividual, TDecrementBetweenIntegralAge, TDecrement> : MultipleDecrement<TIndividual, TDecrementBetweenIntegralAge, TDecrement>
	where TIndividual : IIndividual
	where TDecrementBetweenIntegralAge : IDecrementBetweenIntegralAgeStrategy, new()
	where TDecrement : IDecrement<TIndividual>, IDecrementBetweenIntegralAge<TDecrementBetweenIntegralAge>
{
	public AssociateSingleDecrementUniformDeathDistribution(TDecrement? disabilityDecrement, TDecrement? lapseDecrement, TDecrement? mortalityDecrement, IMemoryCache memoryCache) : base(disabilityDecrement, lapseDecrement, mortalityDecrement, memoryCache) { }

	protected override void CalculateDependentProbabilities(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, ref decimal survivalProbability, ref decimal disabilityProbability, ref decimal lapseProbability, ref decimal mortalityProbability)
	{
		decimal temporaryDisabilityProbability = disabilityProbability;
		decimal temporaryLapseProbability = lapseProbability;
		decimal temporaryMortalityProbability = mortalityProbability;
		disabilityProbability = temporaryDisabilityProbability * (1 - (temporaryLapseProbability + temporaryMortalityProbability) / 2 + temporaryLapseProbability * temporaryMortalityProbability / 3);
		lapseProbability = temporaryLapseProbability * (1 - (temporaryDisabilityProbability + temporaryMortalityProbability) / 2 + temporaryDisabilityProbability * temporaryMortalityProbability / 3);
		mortalityProbability = temporaryMortalityProbability * (1 - (temporaryLapseProbability + temporaryDisabilityProbability) / 2 + temporaryLapseProbability * temporaryDisabilityProbability / 3);
	}
	public override decimal? DependentDisabilityDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => DependentAssociatedSingleDecrement(individual, calculationDate, decrementDate, Disability, Lapse, Mortality);
	public override decimal[]? DependentDisabilityDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => GetProbabilities(individual, calculationDate, dates).DisabilityProbabilities;
	public override decimal? DependentLapseDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => DependentAssociatedSingleDecrement(individual, calculationDate, decrementDate, Lapse, Disability, Mortality);
	public override decimal[]? DependentLapseDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => GetProbabilities(individual, calculationDate, dates).LapseProbabilities;
	public override decimal? DependentMortalityDecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => DependentAssociatedSingleDecrement(individual, calculationDate, decrementDate, Mortality, Disability, Lapse);
	public override decimal[]? DependentMortalityDecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates) => GetProbabilities(individual, calculationDate, dates).MortalityProbabilities;

	private static decimal? DependentAssociatedSingleDecrement(TIndividual individual, DateOnly calculationDate, DateOnly decrementDate, IDecrement<TIndividual>? firstDecrement, IDecrement<TIndividual>? secondDecrement, IDecrement<TIndividual>? thirdDecrement)
	{
		if (firstDecrement is null) return null;
		decimal firstSingleDecrement = firstDecrement.DecrementProbability(individual, calculationDate, decrementDate);
		decimal secondSingleDecrement = secondDecrement?.DecrementProbability(individual, calculationDate, decrementDate) ?? 0m;
		decimal thirdSingleDecrement = thirdDecrement?.DecrementProbability(individual, calculationDate, decrementDate) ?? 0m;
		return firstSingleDecrement * (1 - (secondSingleDecrement + thirdSingleDecrement) / 2 + secondSingleDecrement * thirdSingleDecrement / 3);
	}

}
