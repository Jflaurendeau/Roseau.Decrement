using Moq;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.UnitTests.SeedWork;

[TestClass]
public class IDecrementTableTest
{
	private static Mock<IDecrementTable<IIndividual>> DecrementTableMocked { get; } = new();
	private static Mock<IIndividual> IndividualMocked { get; } = new();
	private static DateOnly CalculationDate { get; } = new(2022,3,17);

	[ClassInitialize]
	public static void Initialize(TestContext _)
	{
		DecrementTableMocked.CallBase = true;
		IndividualMocked.Setup(x => x.DateOfBirth)
			.Returns(new DateOnly(2000, 2, 15));
		DecrementTableMocked.Setup(x => x.FirstAge)
			.Returns(18);
		DecrementTableMocked.Setup(x => x.LastAge)
			.Returns(115);
	}
	[TestMethod]
	[TestCategory(nameof(IDecrementTable<IIndividual>.IsOlderThanLastAgeOfTheTable))]
	public void IsOlderThanLastAgeOfTheTable_IsNot_ReturnFalse()
	{
		// Arrange
		// Act
		// Assert
		Assert.IsFalse(DecrementTableMocked.Object.IsOlderThanLastAgeOfTheTable(IndividualMocked.Object, CalculationDate));
	}
	[TestMethod]
	[TestCategory(nameof(IDecrementTable<IIndividual>.IsOlderThanLastAgeOfTheTable))]
	public void IsOlderThanLastAgeOfTheTable_Is_ReturnTrue()
	{
		// Arrange
		// Act
		// Assert
		Assert.IsTrue(DecrementTableMocked.Object.IsOlderThanLastAgeOfTheTable(IndividualMocked.Object, CalculationDate.AddYears(200)));
	}
	[TestMethod]
	[TestCategory(nameof(IDecrementTable<IIndividual>.AgeLimitedByLifeTable))]
	public void AgeLimitedByLifeTable_AgedBelowFirstAge_ReturnFirstAge()
	{
		// Arrange
		// Act
		// Assert
		Assert.AreEqual(DecrementTableMocked.Object.FirstAge, DecrementTableMocked.Object.AgeLimitedByLifeTable(IndividualMocked.Object, CalculationDate.AddYears(-10)));
	}
	[TestMethod]
	[TestCategory(nameof(IDecrementTable<IIndividual>.AgeLimitedByLifeTable))]
	public void AgeLimitedByLifeTable_AgedOverLastAge_ReturnLastAge()
	{
		// Arrange
		// Act
		// Assert
		Assert.AreEqual(DecrementTableMocked.Object.LastAge, DecrementTableMocked.Object.AgeLimitedByLifeTable(IndividualMocked.Object, CalculationDate.AddYears(210)));
	}
	[TestMethod]
	[TestCategory(nameof(IDecrementTable<IIndividual>.AgeLimitedByLifeTable))]
	public void AgeLimitedByLifeTable_AgedBetweenFirstAndLastAge_ReturnRealAge()
	{
		// Arrange
		// Act
		var age = IndividualMocked.Object.DateOfBirth.AgeNearestBirthday(CalculationDate);
		// Assert
		Assert.AreEqual(age, DecrementTableMocked.Object.AgeLimitedByLifeTable(IndividualMocked.Object, CalculationDate));
	}
	[TestMethod]
	[TestCategory(nameof(IDecrementTable<IIndividual>.LastPossibleDecrementDate))]
	public void LastPossibleDecrementDate_ReturnTheOnlyPossibleDecrementDate()
	{
		// Arrange
		int ageFirstOfFollowingYear = IndividualMocked.Object.DateOfBirth.AgeNearestBirthday(IndividualMocked.Object.DateOfBirth.FirstDayOfFollowingYear());
		// Act
		// Assert
		Assert.AreEqual(IndividualMocked.Object.DateOfBirth.FirstDayOfFollowingYear().AddYears(DecrementTableMocked.Object.LastAge-1+ageFirstOfFollowingYear), DecrementTableMocked.Object.LastPossibleDecrementDate(IndividualMocked.Object));
	}
}
