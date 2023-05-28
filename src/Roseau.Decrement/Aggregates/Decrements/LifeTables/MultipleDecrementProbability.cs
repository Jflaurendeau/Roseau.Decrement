namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public readonly struct MultipleDecrementProbability
{
	internal MultipleDecrementProbability(decimal survivalProbability, decimal? disabilityProbability, decimal? lapseProbability, decimal? mortalityProbability) 
	{ 
		SurvivalProbability = survivalProbability;
		DisabilityProbability = disabilityProbability;
		LapseProbability = lapseProbability;
		MortalityProbability = mortalityProbability;
	}

	public decimal SurvivalProbability { get; init; }
	public decimal? DisabilityProbability { get; init; }
	public decimal? LapseProbability { get; init; }
	public decimal? MortalityProbability { get; init; }
}
