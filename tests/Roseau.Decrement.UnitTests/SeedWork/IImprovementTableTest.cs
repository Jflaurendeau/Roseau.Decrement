using Moq;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.UnitTests.SeedWork;

[TestClass]
public class IImprovementTableTest
{
	private static Mock<IImprovementTable<IIndividual>> ImprovementTableMocked { get; } = new();
	private static Mock<IIndividual> IndividualMocked { get; } = new();
	private static DateOnly CalculationDate { get; } = new(2022,3,17);

	[ClassInitialize]
	public static void Initialize(TestContext _)
	{
		IndividualMocked.Setup(x => x.DateOfBirth)
			.Returns(new DateOnly(2000, 2, 15));
		ImprovementTableMocked.CallBase = true;
		ImprovementTableMocked.Setup(x => x.FirstAge)
			.Returns(18);
		ImprovementTableMocked.Setup(x => x.LastAge)
			.Returns(115);
		ImprovementTableMocked.Setup(x => x.FirstYear)
			.Returns(2000);
		ImprovementTableMocked.Setup(x => x.LastYear)
			.Returns(2030);
	}
	[TestMethod]
	[TestCategory(nameof(IImprovementTable<IIndividual>.AgeLimitedByScale))]
	public void AgeLimitedByScale_AgedBelowFirstAge_ReturnFirstAge()
	{
		// Arrange
		// Act
		// Assert
		Assert.AreEqual(ImprovementTableMocked.Object.FirstAge, ImprovementTableMocked.Object.AgeLimitedByScale(IndividualMocked.Object, CalculationDate.AddYears(-10)));
	}
	[TestMethod]
	[TestCategory(nameof(IImprovementTable<IIndividual>.AgeLimitedByScale))]
	public void AgeLimitedByLifeTable_AgedOverLastAge_ReturnLastAge()
	{
		// Arrange
		// Act
		// Assert
		Assert.AreEqual(ImprovementTableMocked.Object.LastAge, ImprovementTableMocked.Object.AgeLimitedByScale(IndividualMocked.Object, CalculationDate.AddYears(210)));
	}
	[TestMethod]
	[TestCategory(nameof(IImprovementTable<IIndividual>.AgeLimitedByScale))]
	public void AgeLimitedByLifeTable_AgedBetweenFirstAndLastAge_ReturnRealAge()
	{
		// Arrange
		// Act
		var age = IndividualMocked.Object.DateOfBirth.AgeNearestBirthday(CalculationDate);
		// Assert
		Assert.AreEqual(age, ImprovementTableMocked.Object.AgeLimitedByScale(IndividualMocked.Object, CalculationDate));
	}
	[TestMethod]
	[TestCategory(nameof(IImprovementTable<IIndividual>.YearLimitedByScale))]
	public void YearLimitedByScale_YearBelowFirstYear_ReturnFirstYear()
	{
		// Arrange
		// Act
		// Assert
		Assert.AreEqual(ImprovementTableMocked.Object.FirstYear, ImprovementTableMocked.Object.YearLimitedByScale(CalculationDate.AddYears(-30).Year));
	}
	[TestMethod]
	[TestCategory(nameof(IImprovementTable<IIndividual>.YearLimitedByScale))]
	public void YearLimitedByScale_YearOverLastYear_ReturnLastYear()
	{
		// Arrange
		// Act
		// Assert
		Assert.AreEqual(ImprovementTableMocked.Object.LastYear, ImprovementTableMocked.Object.YearLimitedByScale(CalculationDate.AddYears(30).Year));
	}
	[TestMethod]
	[TestCategory(nameof(IImprovementTable<IIndividual>.YearLimitedByScale))]
	public void YearLimitedByScale_YearBetweenFirstAndLastYear_ReturnRealYear()
	{
		// Arrange
		// Act
		// Assert
		Assert.AreEqual(CalculationDate.Year, ImprovementTableMocked.Object.YearLimitedByScale(CalculationDate.Year));
	}
}
