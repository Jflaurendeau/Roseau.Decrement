using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.ImprovementScales;

public class Improvement<TIndividual> : IImprovement<TIndividual>
	where TIndividual : IIndividual
{
	#region Fields
	protected readonly IImprovementTable<TIndividual> _Table;
	protected readonly IAdjustment<TIndividual> _AdjustmentFactor;
	#endregion

	#region Constructors
	public Improvement(IImprovementTable<TIndividual> table) : this(table, default!) { }
	public Improvement(IImprovementTable<TIndividual> table, IAdjustment<TIndividual> adjustmentFactor)
	{
		if (table is null)
			throw new ArgumentNullException(nameof(table));
		_Table = table;
		_AdjustmentFactor = adjustmentFactor ?? IAdjustment<TIndividual>.Default;
	}
	#endregion

	#region Interface and Overrided Methods
	public decimal ImprovementRate(TIndividual individual, int improvementAge, int improvementYear)
	{
		if (improvementYear < _Table.FirstYear - 1)
			throw new ArgumentOutOfRangeException(nameof(improvementYear), $"The improvement year ({improvementYear}) can not be more than a year before the improvement scale's first year ({_Table.FirstYear}).");
		return _AdjustmentFactor.AdjustmentFactor(individual) * _Table.GetImprovementRate(individual, improvementAge, improvementYear);
	}
	public decimal ImprovementFactor(TIndividual individual, int tableBaseYear, DateOnly decrementDate)
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
		while (++i < improvementRates.Length)
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
	public virtual int GetHashCode(TIndividual individual, int tableBaseYear, in DateOnly decrementDate)
		=> HashCode.Combine(_Table, _AdjustmentFactor, individual, _Table.AgeLimitedByScale(individual, decrementDate), decrementDate.Year);
	public int FirstYear => _Table.FirstYear;

	#endregion
}
