namespace Roseau.Decrement.Aggregates.Individuals;

public interface IGenderedIndividual : IIndividual
{
	public Gender BinaryGender { get; }
}
