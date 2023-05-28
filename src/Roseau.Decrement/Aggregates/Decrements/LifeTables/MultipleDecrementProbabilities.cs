namespace Roseau.Decrement.Aggregates.Decrements.LifeTables;

public class MultipleDecrementProbabilities
{
	internal MultipleDecrementProbabilities(decimal[] survivalProbability, decimal[]? disabilityProbability, decimal[]? lapseProbability, decimal[]? mortalityProbability)
	{
		SurvivalProbabilities = survivalProbability;
		DisabilityProbabilities = disabilityProbability;
		LapseProbabilities = lapseProbability;
		MortalityProbabilities = mortalityProbability;
	}

	public decimal[] SurvivalProbabilities { get; init; }
	public decimal[]? DisabilityProbabilities { get; init; }
	public decimal[]? LapseProbabilities { get; init; }
	public decimal[]? MortalityProbabilities { get; init; }
}
