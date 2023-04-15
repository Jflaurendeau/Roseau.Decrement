using Roseau.Mathematics;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.SeedWork;

public interface IDecrement<in TIndividual> : IDecrement
	where TIndividual : IIndividual
{
	decimal SurvivalProbability(TIndividual individual, DateOnly calculationDate, DateOnly decrementDate);
	decimal[] SurvivalProbabilities(TIndividual individual, DateOnly calculationDate, OrderedDates dates);
	decimal DecrementProbability(TIndividual individual, DateOnly calculationDate, DateOnly decrementDate) => 1 - SurvivalProbability(individual, calculationDate, decrementDate);
	decimal[] DecrementProbabilities(TIndividual individual, DateOnly calculationDate, OrderedDates dates);
	decimal SurvivalExpectancy(TIndividual individual, DateOnly calculationDate)
	{
		decimal lifeExpectancy = 0m;
		for (int i = 0; i < 116; i++)
		{
			lifeExpectancy += SurvivalProbability(individual, calculationDate, calculationDate.AddYears(i));
		}
		return lifeExpectancy - 0.5m;
	}
}