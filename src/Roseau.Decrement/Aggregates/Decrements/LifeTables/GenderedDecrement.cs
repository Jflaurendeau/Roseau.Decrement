using Microsoft.Extensions.Caching.Memory;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public class GenderedDecrement<TGenderedIndividual> : Decrement<TGenderedIndividual>, IUnisexDecrementT<TGenderedIndividual>
	where TGenderedIndividual : IGenderedIndividual
{
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table) : base(table, default!, default!, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IImprovement<TGenderedIndividual> improvementScale) : base(table, improvementScale, default!, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IAdjustment<TGenderedIndividual> adjustment) : base(table, default!, adjustment, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IImprovement<TGenderedIndividual> improvementScale, IAdjustment<TGenderedIndividual> adjustment) : base(table, improvementScale, adjustment, default!) { }
	public GenderedDecrement(IDecrementTable<TGenderedIndividual> table, IImprovement<TGenderedIndividual> improvementScale, IAdjustment<TGenderedIndividual> adjustment, IMemoryCache memoryCache) : base(table, improvementScale, adjustment, memoryCache) { }

	protected override int GetHashCode(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, DecrementOrSurvivalProbabilityIn decrementOrSurvivalProbability) => HashCode.Combine(_Table, individual.Gender, individual.DateOfBirth, calculationDate, dates, decrementOrSurvivalProbability);

	#region IUnisexDecrement interface
	public decimal[] DecrementUnisexProbabilities(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, decimal manProportion)
	{
		throw new NotImplementedException();
	}
	public decimal[] SurvivalUnisexProbabilities(TGenderedIndividual individual, in DateOnly calculationDate, OrderedDates dates, decimal manProportion)
	{
		throw new NotImplementedException();
	}
	public decimal SurvivalUnisexProbability(TGenderedIndividual individual, in DateOnly calculationDate, in DateOnly decrementDate, decimal manProportion)
	{
		throw new NotImplementedException();
	}
	#endregion
}
