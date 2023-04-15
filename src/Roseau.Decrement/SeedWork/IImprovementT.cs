using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.SeedWork;

public interface IImprovement<in TIndividual> where TIndividual : IIndividual
{
	decimal ImprovementFactor(TIndividual individual, int tableBaseYear, DateOnly decrementDate);
}
