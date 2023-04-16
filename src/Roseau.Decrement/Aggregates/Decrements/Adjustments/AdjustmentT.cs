using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.Adjustments;

public class Adjustment<TIndividual> : IAdjustment<TIndividual>
	where TIndividual : IIndividual
{
	public decimal AdjustmentFactor(TIndividual individual) => 1m;
}
