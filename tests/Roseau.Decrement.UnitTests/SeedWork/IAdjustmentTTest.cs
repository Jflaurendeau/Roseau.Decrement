using Roseau.Decrement.Aggregates.Decrements.Adjustments;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.UnitTests.SeedWork;

[TestClass]
public class IAdjustmentTTest
{
	[TestMethod]
	[TestCategory(nameof(IAdjustment<IIndividual>.Default))]
	public void Default_ReturnASingleton_IsTrue()
	{
		// Arrange
		var defaultFromInterface = IAdjustment<IIndividual>.Default;
		var defaultFromInstance = Adjustment<IIndividual>.Default;
		// Act
		// Assert
		Assert.AreEqual(defaultFromInstance, defaultFromInterface);
	}
}
