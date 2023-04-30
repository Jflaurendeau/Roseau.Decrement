using Roseau.Decrement.Aggregates.Decrements.ImprovementScales;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.UnitTests.SeedWork;

[TestClass]
public class IImprovementTTest
{
	[TestMethod]
	[TestCategory(nameof(IImprovement<IIndividual>.Default))]
	public void Default_ReturnASingleton_IsTrue()
	{
		// Arrange
		var defaultFromInterface = IImprovement<IIndividual>.Default;
		var defaultFromInstance = Improvement<IIndividual>.Default;
		// Act
		// Assert
		Assert.AreEqual(defaultFromInstance, defaultFromInterface);
	}
}
