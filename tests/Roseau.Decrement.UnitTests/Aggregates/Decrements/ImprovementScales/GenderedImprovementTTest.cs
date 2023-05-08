using Moq;
using Roseau.Decrement.Aggregates.Decrements.ImprovementScales;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.UnitTests.Aggregates.Decrements.ImprovementScales;

[TestClass]
public class GenderedImprovementTTest
{
	#region Fields
	private const int NUMBEROFYEARS = 2030 - 1999;
	private static Mock<IImprovementTable<IGenderedIndividual>> ImprovementTableMocked { get; } = new();
	private static Mock<IAdjustment<IGenderedIndividual>> AdjustmentMocked { get; } = new();
	private static Mock<IGenderedIndividual> ManIndividualMocked { get; } = new();
	private static Mock<IGenderedIndividual> WomanIndividualMocked { get; } = new();
	private static GenderedImprovement<IGenderedIndividual> GenderedImprovement { get; } = new(ImprovementTableMocked.Object, AdjustmentMocked.Object);
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
	public void Constructor_WithTableAndAdjustment_CallBaseDoesNotThrow()
	{
		// Arrange
		// Act
		// Assert
		Assert.AreEqual(ImprovementRates[0][0] * 1.2m, GenderedImprovement.ImprovementRate(ManIndividualMocked.Object, 0, ImprovementTableMocked.Object.FirstYear));
	}
	[TestMethod]
	[TestCategory("Constructors")]
	public void Constructor_WithTableOnly_CallBaseDoesNotThrow()
	{
		// Arrange
		var improvement = new GenderedImprovement<IGenderedIndividual>(ImprovementTableMocked.Object);
		// Act
		// Assert
		Assert.AreEqual(ImprovementRates[0][0], improvement.ImprovementRate(ManIndividualMocked.Object, 0, ImprovementTableMocked.Object.FirstYear));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedImprovement<IGenderedIndividual>.GetHashCode))]
	public void GetHashCode_ReturnsGoodHashCode()
	{
		// Arrange
		int hashCode = HashCode.Combine(ImprovementTableMocked.Object, AdjustmentMocked.Object, ManIndividualMocked.Object.Gender, ImprovementTableMocked.Object.AgeLimitedByScale(ManIndividualMocked.Object, CalculationDate), CalculationDate.Year);
		// Act
		// Assert
		Assert.AreEqual(hashCode, GenderedImprovement.GetHashCode(ManIndividualMocked.Object, 0, CalculationDate));
	}
}
