using Moq;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;
using Roseau.Mathematics;

namespace Roseau.Decrement.UnitTests.SeedWork;

[TestClass]
public class IUnisexDecrementTTest
{
	private const int NUMBEROFYEARS = 116;
	private static Mock<IUnisexDecrement<IGenderedIndividual>> decrementMocked { get; } = new();
	private static Mock<IGenderedIndividual> individualMocked { get; } = new();
	private static DateOnly calculationDate { get; } = new();
	private static decimal[] survivalProbabilities { get; } = new decimal[NUMBEROFYEARS];
	private static DateOnly[] survivalDates { get; } = new DateOnly[NUMBEROFYEARS];
	private const decimal MANPROPORTION = 0.5m;

	[ClassInitialize]
	public static void Initialize(TestContext _)
	{
		decrementMocked.CallBase = true;
		for (int i = 0; i < NUMBEROFYEARS; i++)
		{
			survivalProbabilities[i] = (survivalProbabilities.Length - 1.0m - i) / (survivalProbabilities.Length - 1);
			survivalDates[i] = calculationDate.AddYears(i);
			decrementMocked.Setup(x => x.SurvivalUnisexProbability(individualMocked.Object, calculationDate, survivalDates[i], MANPROPORTION))
						   .Returns(survivalProbabilities[i]);
		}
	}
	[TestMethod]
	[TestCategory(nameof(IUnisexDecrement<IGenderedIndividual>.DecrementUnisexProbability))]
	public void DecrementProbability_IsTheComplementOfSurvivalProbability_AreEquals()
	{
		// Arrange
		// Act
		var expected = 1 - decrementMocked.Object.SurvivalUnisexProbability(individualMocked.Object, calculationDate, survivalDates[10], MANPROPORTION);
		var actual = decrementMocked.Object.DecrementUnisexProbability(individualMocked.Object, calculationDate, survivalDates[10], MANPROPORTION);
		// Assert
		Assert.AreEqual(expected, actual);
	}
	[TestMethod]
	[TestCategory(nameof(IUnisexDecrement<IGenderedIndividual>.KurtateSurvivalUnisexExpectancy))]
	public void KurtateSurvivalExpectancy_ArithmeticMeanOfNumberOfYearBeforeDeathWeithedByProbability_AreEqual()
	{
		// Arrange

		// Act
		var expected = -1m;
		for (int i = 1; i < NUMBEROFYEARS; i++)
		{
			var survivalAtI = decrementMocked.Object.SurvivalUnisexProbability(individualMocked.Object, calculationDate, survivalDates[i], MANPROPORTION);
			var survivalAtIMinusOne = decrementMocked.Object.SurvivalUnisexProbability(individualMocked.Object, calculationDate, survivalDates[i - 1], MANPROPORTION);

			expected += i * (survivalAtIMinusOne - survivalAtI);
		}
		var actual = decrementMocked.Object.KurtateSurvivalUnisexExpectancy(individualMocked.Object, calculationDate, MANPROPORTION);

		// Assert
		Assert.AreEqual(expected, actual, 20 * Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(IUnisexDecrement<IGenderedIndividual>.SurvivalUnisexExpectancy))]
	public void SurvivalExpectancy_ArithmeticMeanOfNumberOfYearBeforeDeathWeithedByProbability_AreEqual()
	{
		// Arrange

		// Act
		var expected = -0.5m;
		for (int i = 1; i < NUMBEROFYEARS; i++)
		{
			var survivalAtI = decrementMocked.Object.SurvivalUnisexProbability(individualMocked.Object, calculationDate, survivalDates[i], MANPROPORTION);
			var survivalAtIMinusOne = decrementMocked.Object.SurvivalUnisexProbability(individualMocked.Object, calculationDate, survivalDates[i - 1], MANPROPORTION);

			expected += i * (survivalAtIMinusOne - survivalAtI);
		}
		var actual = decrementMocked.Object.SurvivalUnisexExpectancy(individualMocked.Object, calculationDate, MANPROPORTION);

		// Assert
		Assert.AreEqual(expected, actual, 20 * Maths.Epsilon);
	}
}
