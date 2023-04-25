using Roseau.Mathematics;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.SeedWork;

public interface IDecrement<in TIndividual> : IDecrement
	where TIndividual : IIndividual
{
	decimal SurvivalProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate);
	decimal[] SurvivalProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates);
	decimal DecrementProbability(TIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate) => 1 - SurvivalProbability(individual, calculationDate, decrementDate);
	decimal[] DecrementProbabilities(TIndividual individual, in DateOnly calculationDate, OrderedDates dates);
	decimal SurvivalExpectancy(TIndividual individual, in DateOnly calculationDate)
	{
		decimal lifeExpectancy = 0m;
		for (int i = 0; i < 116; i++)
		{
			lifeExpectancy += SurvivalProbability(individual, calculationDate, calculationDate.AddYears(i));
		}
		return lifeExpectancy - 0.5m;
	}
}