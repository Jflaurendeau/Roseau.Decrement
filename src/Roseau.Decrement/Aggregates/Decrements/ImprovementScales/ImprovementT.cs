using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.ImprovementScales;

public sealed class Improvement<TIndividual> : IImprovement<TIndividual>
	where TIndividual : IIndividual
{
	public decimal ImprovementFactor(TIndividual individual, int tableBaseYear, DateOnly decrementDate) => 0m;
}
