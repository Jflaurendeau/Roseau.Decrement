using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.SeedWork;

public interface IDecrementTable<in TIndividual> 
	where TIndividual : IIndividual
{
	public decimal GetRate(TIndividual individual, in DateOnly decrementDate);
	public bool IsOlderThanLastAgeOfTheTable(TIndividual individual, in DateOnly calculationDate)
	{
		int age = individual.DateOfBirth.AgeNearestBirthday(calculationDate.FirstDayOfTheYear());
		return age > LastAge;
	}
	public int AgeLimitedByLifeTable(TIndividual individual, in DateOnly calculationDate)
	{
		int age = individual.DateOfBirth.AgeNearestBirthday(calculationDate.FirstDayOfTheYear());
		return Math.Min(Math.Max(FirstAge, age), LastAge);
	}
	public DateOnly LastPossibleDecrementDate(TIndividual individual)
	{
		DateOnly date = individual.DateOfBirth.FirstDayOfFollowingYear();
		date = individual.DateOfBirth.AgeNearestBirthday(date) == 0
			? date.AddYears(LastAge + 1)
			: date.AddYears(LastAge);
		return date;
	}

	#region Table specifications
	int FirstAge { get; }
	int LastAge { get; }
	int BaseYear { get; }
	#endregion
}
