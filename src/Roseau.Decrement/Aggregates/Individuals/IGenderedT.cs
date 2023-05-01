namespace Roseau.Decrement.Aggregates.Individuals;

public interface IGendered<TSelf> where TSelf : IGendered<TSelf>
{
	TSelf AsOtherGender();
}
