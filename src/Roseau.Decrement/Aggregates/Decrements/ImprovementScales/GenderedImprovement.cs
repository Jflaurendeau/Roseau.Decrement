using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;
using Roseau.Mathematics;

namespace Roseau.Decrement.Aggregates.Decrements.ImprovementScales;

public class GenderedImprovement : Improvement<IGenderedIndividual>
{
	#region Fields
	#endregion

	#region Constructors
	public GenderedImprovement(IImprovementTable<IGenderedIndividual> table) : base(table, default!) { }
	public GenderedImprovement(IImprovementTable<IGenderedIndividual> table, IAdjustment<IGenderedIndividual> adjustmentFactor) : base(table, adjustmentFactor) { }
	#endregion

	#region Interface and Overrided Methods
	public override int GetHashCode(IGenderedIndividual individual, int tableBaseYear, in DateOnly decrementDate)
		=> HashCode.Combine(_Table, _AdjustmentFactor, individual.Gender, _Table.AgeLimitedByScale(individual, decrementDate), decrementDate.Year);
	#endregion
}
