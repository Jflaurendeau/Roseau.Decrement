using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.SeedWork;

public interface IUnisexDecrement<in TGenderedIndividual> : IDecrement<TGenderedIndividual>
	where TGenderedIndividual : IGenderedIndividual
{
	decimal SurvivalUnisexProbability(TGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, decimal manProportion);
	decimal[] SurvivalUnisexProbabilities(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, decimal manProportion);
	decimal DecrementUnisexProbability(TGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, decimal manProportion) => 1 - SurvivalUnisexProbability(individual, calculationDate, decrementDate, manProportion);
	decimal[] DecrementUnisexProbabilities(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, decimal manProportion);
	decimal KurtateSurvivalUnisexExpectancy(TGenderedIndividual individual, in DateOnly calculationDate, decimal manProportion)
	{
		decimal lifeExpectancy = 0m;
		for (int i = 1; i < 116; i++)
		{
			lifeExpectancy += SurvivalUnisexProbability(individual, calculationDate, calculationDate.AddYears(i), manProportion);
		}
		return lifeExpectancy;
	}
	decimal SurvivalUnisexExpectancy(TGenderedIndividual individual, in DateOnly calculationDate, decimal manProportion) => KurtateSurvivalUnisexExpectancy(individual, calculationDate, manProportion) + 0.5m;
}
