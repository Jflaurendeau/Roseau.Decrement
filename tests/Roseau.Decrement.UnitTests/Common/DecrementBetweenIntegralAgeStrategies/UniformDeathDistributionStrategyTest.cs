using Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.UnitTests.Common.DecrementBetweenIntegralAgeStrategies;

[TestClass]
public class UniformDeathDistributionStrategyTest
{
	[TestMethod]
	[TestCategory(nameof(UniformDeathDistributionStrategy.SurvivalProbability))]
	public void SurvivalProbability()
	{
		// Arrange
		var firstDate = new DateOnly(2020, 12, 5);
		var secondDate = new DateOnly(2021, 12, 1);
		var decrementProbability = 0.05m;
		var deathStrategy = new UniformDeathDistributionStrategy();
		// Act
		// Assert
		Assert.AreEqual(IDecrement.UniformSurvivalDistribution(firstDate, secondDate, decrementProbability), deathStrategy.SurvivalProbability(firstDate, secondDate, decrementProbability));

	}
	[TestMethod]
	[TestCategory(nameof(UniformDeathDistributionStrategy.ThreeDecrementsProbability))]
	public void ThreeDecrementsProbability()
	{
		// Arrange
		var firstDate = new DateOnly(2020, 12, 5);
		var secondDate = new DateOnly(2021, 12, 1);
		var firstDecrementProbability = 0.05m;
		var secondDecrementProbability = 0.06m;
		var thirdDecrementProbability = 0.06m;
		var deathStrategy = new UniformDeathDistributionStrategy();
		// Act
		// Assert
		Assert.AreEqual(IDecrement.UniformDecrementDistribution(firstDate, secondDate, firstDecrementProbability, secondDecrementProbability, thirdDecrementProbability), deathStrategy.ThreeDecrementsProbability(firstDate, secondDate, firstDecrementProbability, secondDecrementProbability, thirdDecrementProbability));
	}
	[TestMethod]
	[TestCategory(nameof(UniformDeathDistributionStrategy.ThreeDecrementsRate))]
	public void ThreeDecrementsRate()
	{
		// Arrange
		var firstDecrementProbability = 0.05m;
		var secondDecrementProbability = 0.06m;
		var thirdDecrementProbability = 0.07m;
		var deathStrategy = new UniformDeathDistributionStrategy();
		// Act
		// Assert
		Assert.AreEqual(IDecrement.UniformDecrementDistributionDecrementRates(firstDecrementProbability, secondDecrementProbability, thirdDecrementProbability), deathStrategy.ThreeDecrementsRate(firstDecrementProbability, secondDecrementProbability, thirdDecrementProbability));
	}
}
