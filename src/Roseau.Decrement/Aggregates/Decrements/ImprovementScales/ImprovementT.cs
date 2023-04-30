﻿using Roseau.Decrement.Aggregates.Decrements.Adjustments;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.Aggregates.Decrements.ImprovementScales;

public sealed class Improvement<TIndividual> : IImprovement<TIndividual>
	where TIndividual : IIndividual
{
	private Improvement() { }
	public static Improvement<TIndividual> Default { get; } = new();
	public decimal ImprovementRate(TIndividual individual, int improvementAge, int improvementYear) => 1m;
	public decimal ImprovementFactor(TIndividual individual, int tableBaseYear, DateOnly decrementDate) => 1m;
}
