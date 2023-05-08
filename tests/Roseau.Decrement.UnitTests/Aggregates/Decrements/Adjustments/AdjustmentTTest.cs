using Moq;
using Roseau.Decrement.Aggregates.Decrements.Adjustments;
using Roseau.Decrement.Aggregates.Individuals;

namespace Roseau.Decrement.UnitTests.Aggregates.Decrements.Adjustments;

[TestClass]
public class AdjustmentTTest
{
	private static Mock<IIndividual> individualMocked { get; } = new();

	[TestMethod]
	[TestCategory(nameof(Adjustment<IIndividual>.AdjustmentFactor))]
	public void AdjustmentFactor_ReturnsOne()
	{
		// Arrange
		Adjustment<IIndividual> adjustment = Adjustment<IIndividual>.Default;
		// Act
		// Assert
		Assert.AreEqual(1m, adjustment.AdjustmentFactor(individualMocked.Object));
	}
}
