using Moq;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;
using Roseau.Mathematics;

namespace Roseau.Decrement.UnitTests.SeedWork;

[TestClass]
public class IDecrementTTest
{
	private const int NUMBEROFYEARS = 116;
	private static Mock<IDecrement<IIndividual>> decrementMocked { get; } = new();
	private static Mock<IIndividual> individualMocked { get; } = new();
	private static DateOnly calculationDate { get; } = new();
	private	static decimal[] survivalProbabilities { get; } = new decimal[NUMBEROFYEARS];
	private static DateOnly[] survivalDates { get; } = new DateOnly[NUMBEROFYEARS];
	
	[ClassInitialize]
	public static void Initialize(TestContext _)
	{
		decrementMocked.CallBase = true;
		for (int i = 0; i < NUMBEROFYEARS; i++)
		{
			survivalProbabilities[i] = (survivalProbabilities.Length - 1.0m - i) / (survivalProbabilities.Length - 1);
			survivalDates[i] = calculationDate.AddYears(i);
			decrementMocked.Setup(x => x.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i]))
						   .Returns(survivalProbabilities[i]);
		}
	}

	[TestMethod]
	[TestCategory(nameof(IDecrement<IIndividual>.DecrementProbability))]
	public void DecrementProbability_IsTheComplementOfSurvivalProbability_AreEquals()
	{
		// Arrange
		// Act
		var expected = 1 - decrementMocked.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		var actual = decrementMocked.Object.DecrementProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		// Assert
		Assert.AreEqual(expected, actual);
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement<IIndividual>.KurtateSurvivalExpectancy))]
	public void KurtateSurvivalExpectancy_ArithmeticMeanOfNumberOfYearBeforeDeathWeithedByProbability_AreEqual()
	{
		// Arrange

		// Act
		var expected = -1m;
		for (int i = 1; i < NUMBEROFYEARS; i++)
		{
			var survivalAtI = decrementMocked.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i]);
			var survivalAtIMinusOne = decrementMocked.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i - 1]);

			expected += i * (survivalAtIMinusOne - survivalAtI);
		}
		var actual = decrementMocked.Object.KurtateSurvivalExpectancy(individualMocked.Object, calculationDate);

		// Assert
		Assert.AreEqual(expected, actual, 20 * Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement<IIndividual>.SurvivalExpectancy))]
	public void SurvivalExpectancy_ArithmeticMeanOfNumberOfYearBeforeDeathWeithedByProbability_AreEqual()
	{
		// Arrange
		
		// Act
		var expected = -0.5m;
		for (int i = 1; i < NUMBEROFYEARS; i++)
		{
			var survivalAtI = decrementMocked.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i]);
			var survivalAtIMinusOne = decrementMocked.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i - 1]);

			expected += i * (survivalAtIMinusOne - survivalAtI); 
		}
		var actual = decrementMocked.Object.SurvivalExpectancy(individualMocked.Object, calculationDate);

		// Assert
		Assert.AreEqual(expected, actual, 20*Maths.Epsilon);
	}
}
