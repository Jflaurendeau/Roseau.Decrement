using Moq;
using Roseau.Decrement.Aggregates.Decrements.ImprovementScales;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.UnitTests.Aggregates.Decrements.ImprovementScales;

[TestClass]
public class ImprovementCachedTTest
{
	#region Fields
	private const int NUMBEROFYEARS = 2030 - 1999;
	private static Mock<IImprovementTable<IGenderedIndividual>> ImprovementTableMocked { get; } = new();
	private static Mock<IAdjustment<IGenderedIndividual>> AdjustmentMocked { get; } = new();
	private static Mock<IGenderedIndividual> ManIndividualMocked { get; } = new();
	private static Mock<IGenderedIndividual> WomanIndividualMocked { get; } = new();
	private static Improvement<IGenderedIndividual> GenderedImprovement { get; } = new(ImprovementTableMocked.Object, AdjustmentMocked.Object);
	private static ImprovementCached<IGenderedIndividual> GenderedImprovementCached { get; } = new(GenderedImprovement);
	private static DateOnly CalculationDate { get; } = new(2014, 1, 1);
	private static decimal[][] ImprovementRates { get; } = new decimal[2][];
	#endregion

	[ClassInitialize]
	public static void Initialize(TestContext _)
	{
		ImprovementTableMocked.CallBase = true;
		ManIndividualMocked.Setup(x => x.Gender)
			.Returns(Gender.Man);
		WomanIndividualMocked.Setup(x => x.Gender)
			.Returns(Gender.Woman);
		ManIndividualMocked.Setup(x => x.DateOfBirth)
			.Returns(new DateOnly(1990, 1, 1));
		WomanIndividualMocked.Setup(x => x.DateOfBirth)
			.Returns(new DateOnly(1990, 1, 1));
		ImprovementTableMocked.Setup(x => x.GetImprovementRatesAsMemory(ManIndividualMocked.Object, It.IsAny<int>(), It.IsAny<DateOnly>()))
			.Returns(ImprovementRates[0]);
		ImprovementTableMocked.Setup(x => x.GetImprovementRatesAsMemory(WomanIndividualMocked.Object, It.IsAny<int>(), It.IsAny<DateOnly>()))
			.Returns(ImprovementRates[1]);
		AdjustmentMocked.Setup(x => x.AdjustmentFactor(It.IsAny<IGenderedIndividual>()))
			.Returns(1.2m);
		decimal[] manSurvivalProbabilities = new decimal[NUMBEROFYEARS];
		decimal[] womanSurvivalProbabilities = new decimal[NUMBEROFYEARS];
		ImprovementRates[0] = manSurvivalProbabilities;
		ImprovementRates[1] = womanSurvivalProbabilities;
		for (int i = 0; i < NUMBEROFYEARS; i++)
		{
			decimal improvement = 0.03m * (NUMBEROFYEARS - 1.0m - i) / (NUMBEROFYEARS - 1) + i * 0.008m / (NUMBEROFYEARS - 1);
			manSurvivalProbabilities[i] = improvement;
			womanSurvivalProbabilities[i] = improvement * 1.01m;
			ImprovementTableMocked.Setup(x => x.GetImprovementRate(ManIndividualMocked.Object, It.IsAny<int>(), 2000 + i))
				.Returns(manSurvivalProbabilities[i]);
			ImprovementTableMocked.Setup(x => x.GetImprovementRate(WomanIndividualMocked.Object, It.IsAny<int>(), 2000 + i))
				.Returns(womanSurvivalProbabilities[i]);
		}
		ImprovementTableMocked.Setup(x => x.FirstAge)
			.Returns(18);
		ImprovementTableMocked.Setup(x => x.LastAge)
			.Returns(115);
		ImprovementTableMocked.Setup(x => x.FirstYear)
			.Returns(2000);
		ImprovementTableMocked.Setup(x => x.LastYear)
			.Returns(2030);
		for (int i = 0; i < 100; i++)
		{
			ImprovementTableMocked.Setup(x => x.GetImprovementRate(ManIndividualMocked.Object, It.IsAny<int>(), 1999 - i))
					.Returns(manSurvivalProbabilities[0]);
			ImprovementTableMocked.Setup(x => x.GetImprovementRate(ManIndividualMocked.Object, It.IsAny<int>(), 2031 + i))
					.Returns(manSurvivalProbabilities[^1]);
		}
	}

	[TestMethod]
	[TestCategory("Constructors")]
	public void Constructor_WithANullTable_ThrowsArguementNullException()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentNullException>(() => new ImprovementCached<IGenderedIndividual>(null!));
	}
	[TestMethod]
	[TestCategory(nameof(ImprovementCached<IGenderedIndividual>.FirstYear))]
	public void FirstYear_EqualsUnderlyingImprovement()
	{
		// Arrange

		// Act
		// Assert
		Assert.AreEqual(GenderedImprovement.FirstYear, GenderedImprovementCached.FirstYear);
	}
	[TestMethod]
	[TestCategory(nameof(ImprovementCached<IGenderedIndividual>.ImprovementRate))]
	public void ImprovementRate_EqualsUnderlyingImprovement()
	{
		// Arrange

		// Act
		// Assert
		Assert.AreEqual(GenderedImprovement.ImprovementRate(ManIndividualMocked.Object, 18, 2014), GenderedImprovementCached.ImprovementRate(ManIndividualMocked.Object, 18, 2014));
	}
	[TestMethod]
	[TestCategory(nameof(ImprovementCached<IGenderedIndividual>.ImprovementFactor))]
	public void ImprovementFactor_EqualsUnderlyingImprovement()
	{
		// Arrange

		// Act
		// Assert
		Assert.AreEqual(GenderedImprovement.ImprovementFactor(ManIndividualMocked.Object, 2020, CalculationDate), GenderedImprovementCached.ImprovementFactor(ManIndividualMocked.Object, 2020, CalculationDate));
	}
	[TestMethod]
	[TestCategory(nameof(ImprovementCached<IGenderedIndividual>.ImprovementFactor))]
	public void ImprovementFactor_DecrementYearMoreThanOneBeforeTableFirstYear_ThrowArgumentOutOfRange()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => GenderedImprovementCached.ImprovementFactor(ManIndividualMocked.Object, CalculationDate.Year, CalculationDate.AddYears(-16)));
	}
	[TestMethod]
	[TestCategory(nameof(ImprovementCached<IGenderedIndividual>.ImprovementFactor))]
	public void ImprovementFactor_TableBaseYearMoreThanOneBeforeTableFirstYear_ThrowArgumentOutOfRange()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => GenderedImprovementCached.ImprovementFactor(ManIndividualMocked.Object, CalculationDate.AddYears(-16).Year, CalculationDate));
	}
	[TestMethod]
	[TestCategory(nameof(ImprovementCached<IGenderedIndividual>.ImprovementFactor))]
	public void ImprovementFactor_DateOfBirthIsNotBeforeDecrementDate_ThrowArgumentOutOfRange()
	{
		// Arrange
		Mock<IGenderedIndividual> ManIndividualMocked2 = new();
		ManIndividualMocked2.Setup(x => x.DateOfBirth)
			.Returns(new DateOnly(2020, 1, 1));
		// Act
		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => GenderedImprovementCached.ImprovementFactor(ManIndividualMocked2.Object, CalculationDate.Year, CalculationDate.AddYears(1)));
	}
	[TestMethod]
	[TestCategory(nameof(ImprovementCached<IGenderedIndividual>.ImprovementFactor))]
	[DataRow(1999)]
	[DataRow(2000)]
	[DataRow(2030)]
	[DataRow(2031)]
	public void ImprovementFactor_TableBaseYearIsEqualToDecrementYear_ReturnOne(int year)
	{
		// Arrange
		// Act
		var expectedImprovement = 1m;
		var actualImprovement = GenderedImprovementCached.ImprovementFactor(ManIndividualMocked.Object, year, new DateOnly(year, 5, 17));
		// Assert
		Assert.AreEqual(expectedImprovement, actualImprovement);
	}
}
