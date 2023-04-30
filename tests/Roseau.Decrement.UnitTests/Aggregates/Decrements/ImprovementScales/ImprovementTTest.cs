using Moq;
using Roseau.Decrement.Aggregates.Decrements.ImprovementScales;
using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.UnitTests.Aggregates.Decrements.ImprovementScales;

[TestClass]
public class ImprovementTTest
{
	private static Mock<IIndividual> individualMocked { get; } = new();

	[TestMethod]
	[TestCategory(nameof(Improvement<IIndividual>.ImprovementFactor))]
	public void ImprovementFactor_ReturnsOne()
	{
		// Arrange
		Improvement<IIndividual> adjustment = Improvement<IIndividual>.Default;
		// Act
		// Assert
		Assert.AreEqual(1m, adjustment.ImprovementFactor(individualMocked.Object, 0, new()));
	}
}
