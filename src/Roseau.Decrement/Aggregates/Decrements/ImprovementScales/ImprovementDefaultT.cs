using Roseau.Decrement.Aggregates.Decrements.Adjustments;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.ImprovementScales;

public sealed class ImprovementDefault<TIndividual> : IImprovement<TIndividual>
	where TIndividual : IIndividual
{
	private ImprovementDefault() { }
	public static ImprovementDefault<TIndividual> Default { get; } = new();
	public int FirstYear => 0;
	public decimal ImprovementRate(TIndividual individual, int improvementAge, int improvementYear) => 0m;
	public decimal ImprovementFactor(TIndividual individual, int tableBaseYear, DateOnly decrementDate) => 1m;
	public int GetHashCode(TIndividual individual, int tableBaseYear, in DateOnly decrementDate) => 1;
}
