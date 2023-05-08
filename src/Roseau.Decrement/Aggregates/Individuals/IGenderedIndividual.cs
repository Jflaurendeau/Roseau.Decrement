namespace Roseau.Decrement.Aggregates.Individuals;

public interface IGenderedIndividual : IIndividual, IGendered<IGenderedIndividual>
{
	public Gender Gender { get; }
}
