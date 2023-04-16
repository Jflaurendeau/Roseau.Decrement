using Roseau.Decrement.Aggregates.Decrements.Adjustments;
using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.SeedWork;

public interface IAdjustment<in TIndividual> where TIndividual : IIndividual
{
	static IAdjustment<TIndividual> Default => new Adjustment<TIndividual>();
	decimal AdjustmentFactor(TIndividual individual);
}
