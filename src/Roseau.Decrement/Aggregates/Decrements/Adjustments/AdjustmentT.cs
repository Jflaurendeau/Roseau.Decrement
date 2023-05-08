using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.Adjustments;

public sealed class Adjustment<TIndividual> : IAdjustment<TIndividual>
	where TIndividual : IIndividual
{
	private Adjustment() { }
	public static Adjustment<TIndividual> Default { get; } = new();
	public decimal AdjustmentFactor(TIndividual individual) => 1m;
}
