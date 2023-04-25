using Roseau.Decrement.Aggregates.Decrements.ImprovementScales;
using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.SeedWork;

public interface IImprovement<in TIndividual> where TIndividual : IIndividual
{
	static IImprovement<TIndividual> Default => new Improvement<TIndividual>();
	decimal ImprovementFactor(TIndividual individual, int tableBaseYear, DateOnly decrementDate);
}
