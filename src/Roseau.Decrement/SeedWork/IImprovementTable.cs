using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using System.Reflection.Metadata.Ecma335;

namespace Roseau.Decrement.SeedWork;

public interface IImprovementTable<in TIndividual> 
	where TIndividual : IIndividual
{
	public ReadOnlyMemory<decimal> GetImprovementRatesAsMemory(TIndividual individual, int decrementTableBaseYear, in DateOnly decrementDate);
	public ReadOnlySpan<decimal> GetImprovementRatesAsSpan(TIndividual individual, int decrementTableBaseYear, in DateOnly decrementDate);
	public decimal GetImprovementRate(TIndividual individual, int improvementAge, int improvementYear);

	public int AgeLimitedByScale(TIndividual individual, DateOnly decrementDate)
	{
		int age = individual.DateOfBirth.AgeNearestBirthday(new DateOnly(decrementDate.Year, 1, 1));

		return Math.Min(Math.Max(FirstAge, age), LastAge);
	}
	public int YearLimitedByScale(int improvementYear)
	{
		return Math.Min(Math.Max(FirstYear, improvementYear), LastYear);
	}

	#region Table specifications
	int FirstAge { get; }
	int LastAge { get; }
	int FirstYear { get; }
	int LastYear { get; }
	#endregion
}
