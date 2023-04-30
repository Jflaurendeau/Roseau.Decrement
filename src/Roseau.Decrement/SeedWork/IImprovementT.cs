using Roseau.Decrement.Aggregates.Decrements.ImprovementScales;
using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.SeedWork;

public interface IImprovement<in TIndividual> where TIndividual : IIndividual
{
	static IImprovement<TIndividual> Default => Improvement<TIndividual>.Default;
	int FirstYear { get; }
	decimal ImprovementRate(TIndividual individual, int improvementAge, int improvementYear);
	decimal ImprovementFactor(TIndividual individual, int tableBaseYear, DateOnly decrementDate);
	int GetHashCode(TIndividual individual, int tableBaseYear, DateOnly decrementDate);

}
