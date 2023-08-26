using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public class AssociateSingleDecrementUniformDeathDistribution<TIndividual, TDecrement> : MultipleDecrement<TIndividual, UniformDeathDistributionStrategy, TDecrement>
	where TIndividual : IIndividual
	where TDecrement : IDecrementBetweenIntegralAge<TIndividual, UniformDeathDistributionStrategy>
{
	public AssociateSingleDecrementUniformDeathDistribution(TDecrement? disabilityDecrement, TDecrement? lapseDecrement, TDecrement? mortalityDecrement, IMemoryCache? memoryCache) : base(new UniformDeathDistributionStrategy(), disabilityDecrement, lapseDecrement, mortalityDecrement, memoryCache) { }
}
