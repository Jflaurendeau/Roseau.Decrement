using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.SeedWork;

public interface IAdjustment<in TIndividual> where TIndividual : IIndividual
{
	decimal AdjustmentFactor(TIndividual individual);
}
