using Moq;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Decrements.LifeTables;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.UnitTests.Aggregates.Decrements.LifeTables;

[TestClass]
public class MultipleDecrementTTest
{
	private const int NUMBEROFYEARS = 116;
	private static Mock<IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>> decrement1Mocked { get; } = new();
	private static Mock<IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>> decrement2Mocked { get; } = new();
	private static Mock<IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>> decrement3Mocked { get; } = new();
	private static MultipleDecrement<IIndividual, UniformDeathDistributionStrategy, IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>> multipleDecrement { get; } = 
		new AssociateSingleDecrementUniformDeathDistribution<IIndividual, IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>>(decrement1Mocked.Object, decrement2Mocked.Object, decrement3Mocked.Object, null);
	private static Mock<IIndividual> individualMocked { get; } = new();
	private static DateOnly calculationDate { get; } = new();
	private static decimal[] survival1Probabilities { get; } = new decimal[NUMBEROFYEARS];
	private static decimal[] survival2Probabilities { get; } = new decimal[NUMBEROFYEARS];
	private static decimal[] survival3Probabilities { get; } = new decimal[NUMBEROFYEARS];
	private static decimal[] survivalProbabilities { get; } = new decimal[NUMBEROFYEARS];
	private static DateOnly[] survivalDates { get; } = new DateOnly[NUMBEROFYEARS];
	private static OrderedDates dates { get; } = new OrderedDates(survivalDates);
	private static MultipleDecrementProbability multipleDecrementProbability { get; } = new();
	private static MultipleDecrementProbabilities multipleDecrementProbabilities { get; } = new(survivalProbabilities, survival1Probabilities, survival2Probabilities, survival3Probabilities);

	[ClassInitialize]
	public static void Initialize(TestContext _)
	{
		decrement1Mocked.CallBase = true;
		decrement2Mocked.CallBase = true;
		decrement3Mocked.CallBase = true;

		for (int i = 0; i < NUMBEROFYEARS; i++)
		{
			survival1Probabilities[i] = (survival1Probabilities.Length - 1.0m - i) / (survival1Probabilities.Length - 1);
			survival2Probabilities[i] = survival1Probabilities[i] == 1m ? 1m : survival1Probabilities[i] * 0.9m;
			survival3Probabilities[i] = survival2Probabilities[i] == 1m ? 1m : survival2Probabilities[i] * 0.9m;
			survivalDates[i] = calculationDate.AddYears(i);
			decrement1Mocked.Setup(x => x.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i]))
						   .Returns(survival1Probabilities[i]);
			decrement2Mocked.Setup(x => x.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i]))
						   .Returns(survival2Probabilities[i]);
			decrement3Mocked.Setup(x => x.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i]))
						   .Returns(survival3Probabilities[i]);
			survivalProbabilities[i] = survival1Probabilities[i] * survival2Probabilities[i] * survival3Probabilities[i];
		}
		decrement1Mocked.Setup(x => x.SurvivalProbabilities(individualMocked.Object, calculationDate, dates))
						.Returns(survival1Probabilities);
		decrement2Mocked.Setup(x => x.SurvivalProbabilities(individualMocked.Object, calculationDate, dates))
					   .Returns(survival2Probabilities);
		decrement3Mocked.Setup(x => x.SurvivalProbabilities(individualMocked.Object, calculationDate, dates))
					   .Returns(survival3Probabilities);
	}

	[TestMethod]
	[TestCategory(nameof(MultipleDecrement<IIndividual, UniformDeathDistributionStrategy, IDecrementBetweenIntegralAge <IIndividual, UniformDeathDistributionStrategy>>.DecrementRate))]
	[DataRow(1, 1, 1)]
	[DataRow(1, 1, 0)]
	[DataRow(1, 0, 1)]
	[DataRow(1, 0, 0)]
	[DataRow(0, 1, 1)]
	[DataRow(0, 1, 0)]
	[DataRow(0, 0, 1)]
	[DataRow(0, 0, 0)]
	public void DecrementRate(int hasDisabilityDecrement, int hasLapseDecrement, int hasMortalityDecrement)
	{
		// Arrange
		decrement1Mocked.Setup(x => x.DecrementRate(individualMocked.Object, survivalDates[10]))
						   .Returns(0.05m);
		decrement2Mocked.Setup(x => x.DecrementRate(individualMocked.Object, survivalDates[10]))
					   .Returns(0.06m);
		decrement3Mocked.Setup(x => x.DecrementRate(individualMocked.Object, survivalDates[10]))
						.Returns(0.07m);
		MultipleDecrement<IIndividual, UniformDeathDistributionStrategy, IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>> decrement =
		new AssociateSingleDecrementUniformDeathDistribution<IIndividual, IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>>(
			hasDisabilityDecrement == 1 ? decrement1Mocked.Object : null,
			hasLapseDecrement == 1 ? decrement2Mocked.Object : null,
			hasMortalityDecrement == 1 ? decrement3Mocked.Object : null, null);

	// Act
		var expected = 1 - (1 - hasDisabilityDecrement * decrement1Mocked.Object.DecrementRate(individualMocked.Object, survivalDates[10])) *
				(1 - hasLapseDecrement * decrement2Mocked.Object.DecrementRate(individualMocked.Object, survivalDates[10])) *
				(1 - hasMortalityDecrement * decrement3Mocked.Object.DecrementRate(individualMocked.Object, survivalDates[10]));
		var actual = decrement.DecrementRate(individualMocked.Object, survivalDates[10]);
		// Assert
		Assert.AreEqual(expected, actual);
	}
	[TestMethod]
	[TestCategory(nameof(MultipleDecrement<IIndividual, UniformDeathDistributionStrategy, IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>>.LastPossibleDecrementDate))]
	[DataRow(0, 1, 2)]
	[DataRow(0, 2, 1)]
	[DataRow(1, 0, 2)]
	[DataRow(1, 2, 0)]
	[DataRow(2, 0, 1)]
	[DataRow(2, 1, 0)]
	public void LastPossibleDecrementDate(int disabilityDays, int lapseDays, int mortalityDays)
	{
		// Arrange
		decrement1Mocked.Setup(x => x.LastPossibleDecrementDate(individualMocked.Object))
						.Returns(new DateOnly(2018, 12, 1 + disabilityDays));
		decrement2Mocked.Setup(x => x.LastPossibleDecrementDate(individualMocked.Object))
					   .Returns(new DateOnly(2018, 12, 1 + lapseDays));
		decrement3Mocked.Setup(x => x.LastPossibleDecrementDate(individualMocked.Object))
					   .Returns(new DateOnly(2018, 12, 1 + mortalityDays));
		// Act
		var expected = new DateOnly(2018, 12, 1 + Math.Max(Math.Max(disabilityDays, lapseDays), mortalityDays));
		var actual = multipleDecrement.LastPossibleDecrementDate(individualMocked.Object);
		// Assert
		Assert.AreEqual(expected, actual);
	}
}
