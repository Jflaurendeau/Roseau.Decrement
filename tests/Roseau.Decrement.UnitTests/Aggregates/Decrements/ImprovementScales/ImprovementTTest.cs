using Moq;
using Roseau.Decrement.Aggregates.Decrements.ImprovementScales;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;
using Roseau.Decrement.UnitTests.AssertExtensions;

namespace Roseau.Decrement.UnitTests.Aggregates.Decrements.ImprovementScales;

[TestClass]
public class ImprovementTTest
{
	#region Fields
	private const int NUMBEROFYEARS = 2030 - 1999;
	private static Mock<IImprovementTable<IGenderedIndividual>> ImprovementTableMocked { get; } = new();
	private static Mock<IAdjustment<IGenderedIndividual>> AdjustmentMocked { get; } = new();
	private static Mock<IGenderedIndividual> ManIndividualMocked { get; } = new();
	private static Mock<IGenderedIndividual> WomanIndividualMocked { get; } = new();
	private static Improvement<IGenderedIndividual> GenderedImprovement { get; } = new(ImprovementTableMocked.Object, AdjustmentMocked.Object);
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
		Assert.ThrowsException<ArgumentNullException>(() => new GenderedImprovement<IGenderedIndividual>(null!));
	}
	[TestMethod]
	[TestCategory("Constructors")]
	public void Constructor_WithTableOnly_AdjustmentFactorIsDefault()
	{
		// Arrange
		var improvement = new GenderedImprovement<IGenderedIndividual>(ImprovementTableMocked.Object);
		// Act
		// Assert
		Assert.AreEqual(ImprovementRates[0][0], improvement.ImprovementRate(ManIndividualMocked.Object, 0, ImprovementTableMocked.Object.FirstYear));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.GetHashCode))]
	public void GetHashCode_EqualsToSpeceficFeature()
	{
		// Arrange
		int hashCode = HashCode.Combine(ImprovementTableMocked.Object, AdjustmentMocked.Object, ManIndividualMocked.Object, ImprovementTableMocked.Object.AgeLimitedByScale(ManIndividualMocked.Object, CalculationDate), CalculationDate.Year);
		// Act
		// Assert
		Assert.AreEqual(hashCode, GenderedImprovement.GetHashCode(ManIndividualMocked.Object, 2020, CalculationDate));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementRate))]
	public void ImprovementRate_ImprovementYearIsExactlyOneYearBeforeFirstYearOfTheTable_DoesNotThrow()
	{
		// Arrange
		// Act
		// Assert
		Assert.That.DoesNotThrow(() => GenderedImprovement.ImprovementRate(ManIndividualMocked.Object, 18, 1999));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementRate))]
	public void ImprovementRate_ImprovementYearIsMoreThanOneYearBeforeFirstYearOfTheTable_ThrowArgumentOutOfRange()
	{
		// Arrange
		// Act
		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => GenderedImprovement.ImprovementRate(ManIndividualMocked.Object, 18, 1998));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementRate))]
	public void ImprovementRate_ManAndWomanHaveTheirOwnDifferentTable_ReturnsDifferentValue()
	{
		// Arrange

		// Act
		var notExpectedImprovement = GenderedImprovement.ImprovementRate(ManIndividualMocked.Object, 18, 2020);
		var actualImprovement = GenderedImprovement.ImprovementRate(WomanIndividualMocked.Object, 18, 2020);
		// Assert
		Assert.AreNotEqual(notExpectedImprovement, actualImprovement);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementRate))]
	public void ImprovementRate_ImprovementAgeAndImprovementYearAreValid_ReturnsGoodValue()
	{
		// Arrange
		int improvementYear = 2020;

		// Act
		var expectedImprovement = AdjustmentMocked.Object.AdjustmentFactor(ManIndividualMocked.Object) * ImprovementRates[(int)ManIndividualMocked.Object.Gender][improvementYear - ImprovementTableMocked.Object.FirstYear];
		var actualImprovement = GenderedImprovement.ImprovementRate(ManIndividualMocked.Object, 18, improvementYear);
		// Assert
		Assert.AreEqual(expectedImprovement, actualImprovement);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementFactor))]
	public void ImprovementFactor_DecrementExactlyOneBeforeTableFirstYear_DoesNotThrow()
	{
		// Arrange

		// Act
		// Assert
		Assert.That.DoesNotThrow(() => GenderedImprovement.ImprovementFactor(ManIndividualMocked.Object, CalculationDate.Year, CalculationDate.AddYears(-15)));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementFactor))]
	public void ImprovementFactor_DecrementYearMoreThanOneBeforeTableFirstYear_ThrowArgumentOutOfRange()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => GenderedImprovement.ImprovementFactor(ManIndividualMocked.Object, CalculationDate.Year, CalculationDate.AddYears(-16)));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementFactor))]
	public void ImprovementFactor_TableBaseExactlyOneBeforeTableFirstYear_DoesNotThrow()
	{
		// Arrange

		// Act
		// Assert
		Assert.That.DoesNotThrow(() => GenderedImprovement.ImprovementFactor(ManIndividualMocked.Object, CalculationDate.AddYears(-15).Year, CalculationDate));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementFactor))]
	public void ImprovementFactor_TableBaseYearMoreThanOneBeforeTableFirstYear_ThrowArgumentOutOfRange()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => GenderedImprovement.ImprovementFactor(ManIndividualMocked.Object, CalculationDate.AddYears(-16).Year, CalculationDate));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementFactor))]
	public void ImprovementFactor_DateOfBirthIsNotBeforeDecrementDate_ThrowArgumentOutOfRange()
	{
		// Arrange
		Mock<IGenderedIndividual> ManIndividualMocked2 = new();
		ManIndividualMocked2.Setup(x => x.DateOfBirth)
			.Returns(new DateOnly(2020, 1, 1));
		// Act
		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => GenderedImprovement.ImprovementFactor(ManIndividualMocked2.Object, CalculationDate.Year, CalculationDate.AddYears(1)));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementFactor))]
	[DataRow(1999)]
	[DataRow(2000)]
	[DataRow(2030)]
	[DataRow(2031)]
	public void ImprovementFactor_TableBaseYearIsEqualToDecrementYear_ReturnOne(int year)
	{
		// Arrange
		// Act
		var expectedImprovement = 1m;
		var actualImprovement = GenderedImprovement.ImprovementFactor(ManIndividualMocked.Object, year, new DateOnly(year, 5, 17));
		// Assert
		Assert.AreEqual(expectedImprovement, actualImprovement);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementFactor))]
	[DataRow(1999, 1)]
	[DataRow(1999, 2)]
	[DataRow(1999, 10)]
	[DataRow(1999, 30)]
	[DataRow(1999, 31)]
	[DataRow(1999, 40)]
	[DataRow(2000, 1)]
	[DataRow(2000, 30)]
	[DataRow(2000, 35)]
	[DataRow(2030, 1)]
	[DataRow(2030, 5)]
	[DataRow(2031, 1)]
	[DataRow(2031, 10)]
	public void ImprovementFactor_TableBaseYearIsBeforeDecrementYear_ReturnExpectedValue(int year, int numberOfYearsOfDifference)
	{
		// Arrange
		DateOnly decrementDate = new(year + numberOfYearsOfDifference, 2, 17);
		int startIndex = Math.Min(year + 1, ImprovementTableMocked.Object.LastYear) - ImprovementTableMocked.Object.FirstYear;
		int lastIndex = Math.Min(decrementDate.Year, ImprovementTableMocked.Object.LastYear) - ImprovementTableMocked.Object.FirstYear;
		int length = Math.Max(0, lastIndex - startIndex)+1;
		ReadOnlyMemory<decimal> readOnlyMemory = new(ImprovementRates[0], startIndex, length);
		ImprovementTableMocked.Setup(x => x.GetImprovementRatesAsMemory(ManIndividualMocked.Object, year, decrementDate))
				.Returns(readOnlyMemory);
		
		// Act
		var singleImprovement = 1m;
		var expectedImprovement = 1m;
		for (int i = 0; i < numberOfYearsOfDifference; i++)
		{
			singleImprovement = GenderedImprovement.ImprovementRate(ManIndividualMocked.Object, 2014, Math.Min(year + 1, ImprovementTableMocked.Object.LastYear) + i);
			expectedImprovement *= 1 - singleImprovement;
		}
		var actualImprovement = GenderedImprovement.ImprovementFactor(ManIndividualMocked.Object, year, decrementDate);
		// Assert
		Assert.AreEqual(expectedImprovement, actualImprovement);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement.ImprovementFactor))]
	[DataRow(1999, 1)]
	[DataRow(1999, 2)]
	[DataRow(1999, 10)]
	[DataRow(1999, 30)]
	[DataRow(1999, 31)]
	[DataRow(1999, 40)]
	[DataRow(2000, 1)]
	[DataRow(2000, 30)]
	[DataRow(2000, 35)]
	[DataRow(2030, 1)]
	[DataRow(2030, 5)]
	[DataRow(2031, 1)]
	[DataRow(2031, 10)]
	public void ImprovementFactor_TableBaseYearIsAfterDecrementYear_ReturnExpectedValue(int year, int numberOfYearsOfDifference)
	{
		// Arrange
		DateOnly decrementDate = new(year + numberOfYearsOfDifference, 2, 17);
		int startIndex = Math.Min(year + 1, ImprovementTableMocked.Object.LastYear) - ImprovementTableMocked.Object.FirstYear;
		int lastIndex = Math.Min(decrementDate.Year, ImprovementTableMocked.Object.LastYear) - ImprovementTableMocked.Object.FirstYear;
		int length = Math.Max(0, lastIndex - startIndex) + 1;
		ReadOnlyMemory<decimal> readOnlyMemory = new(ImprovementRates[0], startIndex, length);
		ImprovementTableMocked.Setup(x => x.GetImprovementRatesAsMemory(ManIndividualMocked.Object, decrementDate.Year, new DateOnly(year, 3, 29)))
				.Returns(readOnlyMemory);

		// Act
		var singleImprovement = 1m;
		var expectedImprovement = 1m;
		for (int i = 0; i < numberOfYearsOfDifference; i++)
		{
			singleImprovement = GenderedImprovement.ImprovementRate(ManIndividualMocked.Object, 2014, Math.Min(year + 1, ImprovementTableMocked.Object.LastYear) + i);
			expectedImprovement *= 1 - singleImprovement;
		}
		var actualImprovement = GenderedImprovement.ImprovementFactor(ManIndividualMocked.Object, decrementDate.Year, new DateOnly(year, 3, 29));
		// Assert
		Assert.AreEqual(1 / expectedImprovement, actualImprovement);
	}
}
