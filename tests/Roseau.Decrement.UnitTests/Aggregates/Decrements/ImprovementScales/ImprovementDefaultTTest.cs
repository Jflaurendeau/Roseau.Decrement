using Moq;
using Roseau.Decrement.Aggregates.Decrements.ImprovementScales;
using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.UnitTests.Aggregates.Decrements.ImprovementScales;

[TestClass]
public class ImprovementDefaultTTest
{
	private static Mock<IIndividual> individualMocked { get; } = new();

	[TestMethod]
	[TestCategory(nameof(ImprovementDefault<IIndividual>.ImprovementFactor))]
	public void ImprovementFactor_ReturnsOne()
	{
		// Arrange
		ImprovementDefault<IIndividual> adjustment = ImprovementDefault<IIndividual>.Default;
		// Act
		// Assert
		Assert.AreEqual(1m, adjustment.ImprovementFactor(individualMocked.Object, 0, new()));
	}
	[TestMethod]
	[TestCategory(nameof(ImprovementDefault<IIndividual>.ImprovementRate))]
	public void ImprovementRate_ReturnsZero()
	{
		// Arrange
		ImprovementDefault<IIndividual> adjustment = ImprovementDefault<IIndividual>.Default;
		// Act
		// Assert
		Assert.AreEqual(0m, adjustment.ImprovementRate(individualMocked.Object, 0, 2014));
	}
	[TestMethod]
	[TestCategory(nameof(ImprovementDefault<IIndividual>.FirstYear))]
	public void FirstYear_ReturnsZero()
	{
		// Arrange
		ImprovementDefault<IIndividual> adjustment = ImprovementDefault<IIndividual>.Default;
		// Act
		// Assert
		Assert.AreEqual(0, adjustment.FirstYear);
	}
	[TestMethod]
	[TestCategory(nameof(ImprovementDefault<IIndividual>.GetHashCode))]
	public void GetHashCode_ReturnsOne()
	{
		// Arrange
		ImprovementDefault<IIndividual> adjustment = ImprovementDefault<IIndividual>.Default;
		// Act
		// Assert
		Assert.AreEqual(1, adjustment.GetHashCode(individualMocked.Object, 2014, new()));
	}
}
