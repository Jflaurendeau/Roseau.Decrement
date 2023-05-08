namespace Roseau.Decrement.Aggregates.Individuals;

public interface IGendered<out TSelf> where TSelf : IGendered<TSelf>
{
	TSelf AsOtherGender();
}
