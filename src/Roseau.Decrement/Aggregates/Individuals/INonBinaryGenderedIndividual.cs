namespace Roseau.Decrement.Aggregates.Individuals;

public interface INonBinaryGenderedIndividual : IIndividual
{
	NonBinaryGender Gender { get; }
}
