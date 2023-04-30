using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public class GenderedDecrement : Decrement<IGenderedIndividual>
{
	public GenderedDecrement(IDecrementTable<IGenderedIndividual> table) : base(table, default!, default!, default!) { }
	public GenderedDecrement(IDecrementTable<IGenderedIndividual> table, IImprovement<IGenderedIndividual> improvementScale) : base(table, improvementScale, default!, default!) { }
	public GenderedDecrement(IDecrementTable<IGenderedIndividual> table, IAdjustment<IGenderedIndividual> adjustment) : base(table, default!, adjustment, default!) { }
	public GenderedDecrement(IDecrementTable<IGenderedIndividual> table, IImprovement<IGenderedIndividual> improvementScale, IAdjustment<IGenderedIndividual> adjustment) : base(table, improvementScale, adjustment, default!) { }
	public GenderedDecrement(IDecrementTable<IGenderedIndividual> table, IImprovement<IGenderedIndividual> improvementScale, IAdjustment<IGenderedIndividual> adjustment, IMemoryCache memoryCache) : base(table, improvementScale, adjustment, memoryCache) { }

	protected override int GetHashCode(IGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalProbabilityIn decrementOrSurvivalProbability) => HashCode.Combine(_Table, individual.Gender, individual.DateOfBirth, calculationDate, dates, decrementOrSurvivalProbability);
}
