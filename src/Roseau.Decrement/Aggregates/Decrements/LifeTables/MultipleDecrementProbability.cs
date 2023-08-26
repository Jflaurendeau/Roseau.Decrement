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

	public static MultipleDecrementProbability operator *(MultipleDecrementProbability left, MultipleDecrementProbability right)
	{
		decimal survival = left.SurvivalProbability * right.SurvivalProbability;
		decimal? disability =  left.DisabilityProbability is null && right.DisabilityProbability is null ? null : left.DisabilityProbability + left.SurvivalProbability * right.DisabilityProbability;
		decimal? lapse =  left.LapseProbability is null && right.LapseProbability is null ? null : left.LapseProbability + left.SurvivalProbability * right.LapseProbability;
		decimal? mortality =  left.MortalityProbability is null && right.MortalityProbability is null ? null : left.MortalityProbability + left.SurvivalProbability * right.MortalityProbability;
		return new(survival, disability, lapse, mortality);
	}
}
