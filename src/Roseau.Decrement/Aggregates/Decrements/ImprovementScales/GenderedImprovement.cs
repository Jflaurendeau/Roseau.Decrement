using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;
using Roseau.Mathematics;

namespace Roseau.Decrement.Aggregates.Decrements.ImprovementScales;

public class GenderedImprovement : IImprovement<IGenderedIndividual>
{
	#region Fields
	private readonly IImprovementTable<IGenderedIndividual> _Table;
	private readonly IAdjustment<IGenderedIndividual> _AdjustmentFactor;
	#endregion

	#region Constructors
	public GenderedImprovement(IImprovementTable<IGenderedIndividual> table) : this(table, default!) { }
	public GenderedImprovement(IImprovementTable<IGenderedIndividual> table, IAdjustment<IGenderedIndividual> adjustmentFactor)
	{
		if (table is null)
			throw new ArgumentNullException(nameof(table));
		_Table = table;
		_AdjustmentFactor = adjustmentFactor ?? IAdjustment<IGenderedIndividual>.Default;
	}
	#endregion

	#region Interface and Overrided Methods
	public decimal ImprovementRate(IGenderedIndividual individual, int improvementAge, int improvementYear)
	{
		if (improvementYear < _Table.FirstYear - 1)
			throw new ArgumentOutOfRangeException(nameof(improvementYear), $"The improvement year ({improvementYear}) can not be more than a year before the improvement scale's first year ({_Table.FirstYear}).");
		return _AdjustmentFactor.AdjustmentFactor(individual) * _Table.GetImprovementRate(individual, improvementAge, improvementYear);
	}
	public decimal ImprovementFactor(IGenderedIndividual individual, int tableBaseYear, DateOnly decrementDate)
	{
		if (individual.DateOfBirth > decrementDate)
			throw new ArgumentOutOfRangeException(nameof(decrementDate), $"The decrement date ({decrementDate}) can not be before the date of birth.");
		if (decrementDate.Year < _Table.FirstYear - 1)
			throw new ArgumentOutOfRangeException(nameof(decrementDate), $"The date of calculation ({decrementDate}) can not be before 1999-01-01.");
		if (tableBaseYear < _Table.FirstYear - 1)
			throw new ArgumentOutOfRangeException(nameof(tableBaseYear), $"The base year of the underlying mortality base table ({tableBaseYear}) can not be before 1999.");
		if (decrementDate.Year == tableBaseYear) return 1m;

		decimal singleImprovementFactor = 1m;
		decimal improvementFactor = 1m;
		ReadOnlySpan<decimal> improvementRates = _Table.GetImprovementRatesAsMemory(individual, tableBaseYear, decrementDate).Span;
		decimal adjustementFactor = _AdjustmentFactor.AdjustmentFactor(individual);
		int i = -1;
		while(++i < improvementRates.Length)
		{
			singleImprovementFactor = 1 - adjustementFactor * improvementRates[i];
			improvementFactor *= singleImprovementFactor;
		}
		int numberOfImprovementYears = Math.Abs(decrementDate.Year - tableBaseYear);
		while (i < numberOfImprovementYears)
		{
			improvementFactor *= singleImprovementFactor;
			i++;
		}
		if (decrementDate.Year < tableBaseYear)
			return 1 / improvementFactor;
		return improvementFactor;
	}
	#endregion
}
