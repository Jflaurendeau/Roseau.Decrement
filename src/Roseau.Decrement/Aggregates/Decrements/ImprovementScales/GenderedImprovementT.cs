using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;
using Roseau.Mathematics;

namespace Roseau.Decrement.Aggregates.Decrements.ImprovementScales;

public class GenderedImprovement<TGenderedIndividual> : Improvement<TGenderedIndividual>
	where TGenderedIndividual : IGenderedIndividual
{
	#region Fields
	#endregion

	#region Constructors
	public GenderedImprovement(IImprovementTable<TGenderedIndividual> table) : base(table, default!) { }
	public GenderedImprovement(IImprovementTable<TGenderedIndividual> table, IAdjustment<TGenderedIndividual> adjustmentFactor) : base(table, adjustmentFactor) { }
	#endregion

	#region Interface and Overrided Methods
	public override int GetHashCode(TGenderedIndividual individual, int tableBaseYear, in DateOnly decrementDate)
		=> HashCode.Combine(_Table, _AdjustmentFactor, individual.Gender, _Table.AgeLimitedByScale(individual, decrementDate), decrementDate.Year);
	#endregion
}
