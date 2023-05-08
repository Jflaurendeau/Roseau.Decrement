using Moq;
using Roseau.DateHelpers.DateArrayStrategies;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Decrements.LifeTables;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;
using Roseau.Decrement.UnitTests.AssertExtensions;
using Roseau.Mathematics;

namespace Roseau.Decrement.UnitTests.Aggregates.Decrements.LifeTables;

[TestClass]
public class GenderedDecrementTTest
{
	private const int NUMBEROFYEARS = 115 - 17;
	private const decimal MANPROPORTION = 0.5m;
	private static Mock<IDecrementTable<IGenderedIndividual>> DecrementTableMocked { get; } = new();
	private static Mock<IImprovement<IGenderedIndividual>> ImprovementMocked { get; } = new();
	private static Mock<IAdjustment<IGenderedIndividual>> AdjustmentMocked { get; } = new();
	private static Mock<IGenderedIndividual> ManIndividualMocked { get; } = new();
	private static Mock<IGenderedIndividual> WomanIndividualMocked { get; } = new();
	private static DateOnly CalculationDate { get; } = new(2018, 1, 1);
	private static decimal[][] SurvivalProbabilities { get; } = new decimal[2][];
	private static decimal[][] DeathProbabilities { get; } = new decimal[2][];
	private static GenderedDecrement<IGenderedIndividual> Decrement { get; } = new GenderedDecrement<IGenderedIndividual>(DecrementTableMocked.Object, ImprovementMocked.Object, AdjustmentMocked.Object);
	private static DateOnly[] SurvivalDates { get; } = new DateOnly[NUMBEROFYEARS];

	[ClassInitialize]
	public static void Initialize(TestContext _)
	{
		ImprovementMocked.CallBase = true;
		DecrementTableMocked.CallBase = true;
		ManIndividualMocked.CallBase = true;
		WomanIndividualMocked.CallBase = true;
		ManIndividualMocked.Setup(x => x.Gender)
			.Returns(Gender.Man);
		WomanIndividualMocked.Setup(x => x.Gender)
			.Returns(Gender.Woman);
		ManIndividualMocked.Setup(x => x.DateOfBirth)
			.Returns(new DateOnly(2000, 1, 1));
		WomanIndividualMocked.Setup(x => x.DateOfBirth)
			.Returns(new DateOnly(2000, 1, 1));
		ManIndividualMocked.Setup(x => x.AsOtherGender())
			.Returns(WomanIndividualMocked.Object);
		WomanIndividualMocked.Setup(x => x.AsOtherGender())
			.Returns(ManIndividualMocked.Object);
		ImprovementMocked.Setup(x => x.ImprovementFactor(It.IsAny<IGenderedIndividual>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
			.Returns(1m);
		AdjustmentMocked.Setup(x => x.AdjustmentFactor(It.IsAny<IGenderedIndividual>()))
			.Returns(1m);
		decimal[] manDeathProbabilities = new decimal[NUMBEROFYEARS];
		decimal[] manSurvivalProbabilities = new decimal[NUMBEROFYEARS];
		decimal[] womanDeathProbabilities = new decimal[NUMBEROFYEARS];
		decimal[] womanSurvivalProbabilities = new decimal[NUMBEROFYEARS];
		DeathProbabilities[0] = manDeathProbabilities;
		DeathProbabilities[1] = womanDeathProbabilities;
		SurvivalProbabilities[0] = manSurvivalProbabilities;
		SurvivalProbabilities[1] = womanSurvivalProbabilities;
		for (int i = 0; i < NUMBEROFYEARS; i++)
		{
			decimal survivalProbability = (NUMBEROFYEARS - 1.0m - i) / (NUMBEROFYEARS - 1);
			manSurvivalProbabilities[i] = survivalProbability;
			womanSurvivalProbabilities[i] = Math.Min(1m, survivalProbability * (1 + i / (5m * NUMBEROFYEARS)));
			if (i == 0)
			{
				manDeathProbabilities[i] = 0;
				womanDeathProbabilities[i] = 0;
			}
			else
			{
				manDeathProbabilities[i] = 1 - manSurvivalProbabilities[i] / manSurvivalProbabilities[i - 1];
				womanDeathProbabilities[i] = 1 - womanSurvivalProbabilities[i] / womanSurvivalProbabilities[i - 1];
			}
			SurvivalDates[i] = CalculationDate.AddYears(i);
			DecrementTableMocked.Setup(x => x.GetRate(ManIndividualMocked.Object, SurvivalDates[i]))
				.Returns(manDeathProbabilities[i]);
			DecrementTableMocked.Setup(x => x.GetRate(WomanIndividualMocked.Object, SurvivalDates[i]))
				.Returns(womanDeathProbabilities[i]);
		}
		DecrementTableMocked.Setup(x => x.FirstAge)
			.Returns(18);
		DecrementTableMocked.Setup(x => x.BaseYear)
			.Returns(2018);
		DecrementTableMocked.Setup(x => x.LastAge)
			.Returns(115);
	}
	[TestMethod]
	[TestCategory("Constructors")]
	public void Constructor_WithTableOnly_DoesNotThrow()
	{
		// Arrange

		// Act
		// Assert
		Assert.That.DoesNotThrow(() => new GenderedDecrement<IGenderedIndividual>(DecrementTableMocked.Object));
	}
	[TestMethod]
	[TestCategory("Constructors")]
	public void Constructor_WithTableAndImprovementOnly_DoesNotThrow()
	{
		// Arrange

		// Act
		// Assert
		Assert.That.DoesNotThrow(() => new GenderedDecrement<IGenderedIndividual>(DecrementTableMocked.Object, ImprovementMocked.Object));
	}
	[TestMethod]
	[TestCategory("Constructors")]
	public void Constructor_WithTableAndAdjustmentOnly_DoesNotThrow()
	{
		// Arrange

		// Act
		// Assert
		Assert.That.DoesNotThrow(() => new GenderedDecrement<IGenderedIndividual>(DecrementTableMocked.Object, AdjustmentMocked.Object));
	}
	[TestMethod]
	[TestCategory("Constructors")]
	public void Constructor_WithTableAndImprovementAndAdjustmentOnly_DoesNotThrow()
	{
		// Arrange

		// Act
		// Assert
		Assert.That.DoesNotThrow(() => new GenderedDecrement<IGenderedIndividual>(DecrementTableMocked.Object, ImprovementMocked.Object, AdjustmentMocked.Object));
	}
	[TestMethod]
	[TestCategory("Constructors")]
	public void Constructor_WithAllPossibleParams_DoesNotThrow()
	{
		// Arrange

		// Act
		// Assert
		Assert.That.DoesNotThrow(() => new GenderedDecrement<IGenderedIndividual>(DecrementTableMocked.Object, ImprovementMocked.Object, AdjustmentMocked.Object, null!));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_ManProportionIsLowerThanZero_ThrowsArgumentOutOfRangeException()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate, -1));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_ManProportionIsHigherThanOne_ThrowsArgumentOutOfRangeException()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate, 1.02m));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_Man_ManProportionIsExactlyOne_EqualsManSurvivalProbability()
	{
		// Arrange

		// Act
		var expectedSurvivalUnisexProbability = Decrement.SurvivalProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3));
		var actualSurvivalUnisexProbability = Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3), 1);
		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbability, actualSurvivalUnisexProbability);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_Woman_ManProportionIsExactlyOne_EqualsManSurvivalProbability()
	{
		// Arrange

		// Act
		var expectedSurvivalUnisexProbability = Decrement.SurvivalProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3));
		var actualSurvivalUnisexProbability = Decrement.SurvivalUnisexProbability(WomanIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3), 1);
		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbability, actualSurvivalUnisexProbability);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_Man_ManProportionIsExactlyZero_EqualsWomanSurvivalProbability()
	{
		// Arrange

		// Act
		var expectedSurvivalUnisexProbability = Decrement.SurvivalProbability(WomanIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3));
		var actualSurvivalUnisexProbability = Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3), 0);
		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbability, actualSurvivalUnisexProbability);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_Woman_ManProportionIsExactlyZero_EqualsWomanSurvivalProbability()
	{
		// Arrange

		// Act
		var expectedSurvivalUnisexProbability = Decrement.SurvivalProbability(WomanIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3));
		var actualSurvivalUnisexProbability = Decrement.SurvivalUnisexProbability(WomanIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3), 0);
		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbability, actualSurvivalUnisexProbability);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_DecrementDateBeforeCalculationDate_ThrowsArguementException()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentException>(() => Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate, MANPROPORTION));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_DateOfBirthIsNotBeforeCalculationDate_ThrowsArguementException()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentException>(() => Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, ManIndividualMocked.Object.DateOfBirth.AddDays(-1), CalculationDate, MANPROPORTION));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_ManAndWomanHaveTheirOwnDifferentTable_ReturnsSameValue()
	{
		// Arrange

		// Act
		var notExpectedSurvivalUnisexProbability = Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3), MANPROPORTION);
		var actualSurvivalUnisexProbability = Decrement.SurvivalUnisexProbability(WomanIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3), MANPROPORTION);
		// Assert
		Assert.AreEqual(notExpectedSurvivalUnisexProbability, actualSurvivalUnisexProbability);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_UnderFirstAge_ReturnFirstValue()
	{
		// Arrange

		// Act
		var expectedSurvivalUnisexProbability = 1 - MANPROPORTION * (DeathProbabilities[(int)ManIndividualMocked.Object.Gender][0] + DeathProbabilities[(int)WomanIndividualMocked.Object.Gender][0]);
		var actualSurvivalUnisexProbability = Decrement.SurvivalUnisexProbability(WomanIndividualMocked.Object, CalculationDate.AddYears(-1), CalculationDate, MANPROPORTION);
		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbability, actualSurvivalUnisexProbability);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_OverLastAge_ReturnLastValue()
	{
		// Arrange

		// Act
		var expectedSurvivalUnisexProbability = 1 - MANPROPORTION * (DeathProbabilities[(int)ManIndividualMocked.Object.Gender][^1] + DeathProbabilities[(int)WomanIndividualMocked.Object.Gender][^1]);
		var actualSurvivalUnisexProbability = Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, CalculationDate.AddYears(100), CalculationDate.AddYears(101), MANPROPORTION);
		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbability, actualSurvivalUnisexProbability);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_AgeIsInRangeOfTable_ReturnGoodValue()
	{
		// Arrange

		// Act
		var expectedSurvivalUnisexProbability = 1 - MANPROPORTION * (DeathProbabilities[(int)ManIndividualMocked.Object.Gender][2] + DeathProbabilities[(int)WomanIndividualMocked.Object.Gender][2]);
		var actualSurvivalUnisexProbability = Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3), MANPROPORTION);
		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbability, actualSurvivalUnisexProbability);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbability))]
	public void SurvivalUnisexProbability_AgeIsInRangeOfTable_CalculationInMiddleOfAYear_DecrementTwoYearsLater_ReturnGoodValue()
	{
		// Arrange
		var calculationDate = CalculationDate.AddYears(2).AddDays(56);
		var decrementDate = calculationDate.AddYears(2);
		var deathRateYear1 = MANPROPORTION * (DeathProbabilities[(int)ManIndividualMocked.Object.Gender][2] + DeathProbabilities[(int)WomanIndividualMocked.Object.Gender][2]);
		var deathRateYear2 = MANPROPORTION * (DeathProbabilities[(int)ManIndividualMocked.Object.Gender][3] + DeathProbabilities[(int)WomanIndividualMocked.Object.Gender][3]);
		var deathRateYear3 = MANPROPORTION * (DeathProbabilities[(int)ManIndividualMocked.Object.Gender][4] + DeathProbabilities[(int)WomanIndividualMocked.Object.Gender][4]);
		DecrementTableMocked.Setup(x => x.GetRate(ManIndividualMocked.Object, calculationDate))
				.Returns(DeathProbabilities[(int)ManIndividualMocked.Object.Gender][2]);
		DecrementTableMocked.Setup(x => x.GetRate(ManIndividualMocked.Object, decrementDate))
				.Returns(DeathProbabilities[(int)ManIndividualMocked.Object.Gender][4]);
		DecrementTableMocked.Setup(x => x.GetRate(WomanIndividualMocked.Object, calculationDate))
				.Returns(DeathProbabilities[(int)WomanIndividualMocked.Object.Gender][2]);
		DecrementTableMocked.Setup(x => x.GetRate(WomanIndividualMocked.Object, decrementDate))
				.Returns(DeathProbabilities[(int)WomanIndividualMocked.Object.Gender][4]);

		// Act
		var SurvivalUnisexProbabilityYear1 = IDecrement.UniformSurvivalDistribution(deathRateYear1, calculationDate, calculationDate.FirstDayOfFollowingYear());
		var SurvivalUnisexProbabilityYear2 = 1 - deathRateYear2;
		var SurvivalUnisexProbabilityYear3 = IDecrement.UniformSurvivalDistribution(deathRateYear3, decrementDate.FirstDayOfTheYear(), decrementDate);

		var expectedSurvivalUnisexProbability = SurvivalUnisexProbabilityYear1 * SurvivalUnisexProbabilityYear2 * SurvivalUnisexProbabilityYear3;
		var actualSurvivalUnisexProbability = Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, calculationDate, decrementDate, MANPROPORTION);

		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbability, actualSurvivalUnisexProbability);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbabilities))]
	public void SurvivalUnisexProbabilities_ManProportionIsLowerThanZero_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var expectedSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);
		var actualSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);

		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, CalculationDate.AddYears(2), decrementDates, -1));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbabilities))]
	public void SurvivalUnisexProbabilities_ManProportionIsHigherThanOne_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var expectedSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);
		var actualSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);

		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, CalculationDate.AddYears(2), decrementDates, 1.02m));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbabilities))]
	public void SurvivalUnisexProbabilities_Man_ManProportionIsExactlyOne_ReturnsManProbabilities()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var actualSurvivalUnisexProbabilities = Decrement.SurvivalProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates);
		var expectedSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, 1m);

		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbabilities, actualSurvivalUnisexProbabilities);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbabilities))]
	public void SurvivalUnisexProbabilities_Woman_ManProportionIsExactlyOne_ReturnsManProbabilities()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var actualSurvivalUnisexProbabilities = Decrement.SurvivalProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates);
		var expectedSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(WomanIndividualMocked.Object, calculationDate, decrementDates, 1m);

		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbabilities, actualSurvivalUnisexProbabilities);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbabilities))]
	public void SurvivalUnisexProbabilities_Man_ManProportionIsExactlyZero_ReturnsWomanProbabilities()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var actualSurvivalUnisexProbabilities = Decrement.SurvivalProbabilities(WomanIndividualMocked.Object, calculationDate, decrementDates);
		var expectedSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, 0);

		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbabilities, actualSurvivalUnisexProbabilities);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbabilities))]
	public void SurvivalUnisexProbabilities_Woman_ManProportionIsExactlyZero_ReturnsWomanProbabilities()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var actualSurvivalUnisexProbabilities = Decrement.SurvivalProbabilities(WomanIndividualMocked.Object, calculationDate, decrementDates);
		var expectedSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(WomanIndividualMocked.Object, calculationDate, decrementDates, 0);

		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbabilities, actualSurvivalUnisexProbabilities);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbabilities))]
	public void SurvivalUnisexProbabilities_FirstDecrementDateBeforeCalculationDate_ThrowsArguementException()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));

		// Act

		// Assert
		Assert.ThrowsException<ArgumentException>(() => Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, decrementDates[0].AddDays(1), decrementDates, MANPROPORTION));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbabilities))]
	public void SurvivalUnisexProbabilities_DateOfBirthIsNotBeforeCalculationDate_ThrowsArguementException()
	{
		// Arrange
		DateOnly calculationDate = ManIndividualMocked.Object.DateOfBirth.AddDays(-1);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));

		// Act

		// Assert
		Assert.ThrowsException<ArgumentException>(() => Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbabilities))]
	public void SurvivalUnisexProbabilities_Man_CalculationDateInMiddleOfAYear_DecrementEachMonth_ReturnGoodValue()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(57);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryYearStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(100));
		decimal[] expectedSurvivalUnisexProbabilities = new decimal[decrementDates.Count];
		for (int i = 0; i < decrementDates.Count; i++)
		{
			if (decrementDates[i].DayOfYear != 1)
			{
				var decrementProbInYearMan = DecrementTableMocked.Object.GetRate(ManIndividualMocked.Object, decrementDates[i].FirstDayOfTheYear());
				var decrementProbInYearWoman = DecrementTableMocked.Object.GetRate(WomanIndividualMocked.Object, decrementDates[i].FirstDayOfTheYear());
				DecrementTableMocked.Setup(x => x.GetRate(ManIndividualMocked.Object, decrementDates[i]))
					.Returns(decrementProbInYearMan);
				DecrementTableMocked.Setup(x => x.GetRate(WomanIndividualMocked.Object, decrementDates[i]))
					.Returns(decrementProbInYearWoman);
			}
		}

		// Act
		for (int i = 0; i < decrementDates.Count; i++)
			expectedSurvivalUnisexProbabilities[i] = Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, calculationDate, decrementDates[i], MANPROPORTION);

		var actualSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);

		// Assert
		for (int i = 0; i < decrementDates.Count; i++)
			Assert.AreEqual(expectedSurvivalUnisexProbabilities[i], actualSurvivalUnisexProbabilities[i], 30 * Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbabilities))]
	public void SurvivalUnisexProbabilities_Woman_CalculationDateInMiddleOfAYear_DecrementEachMonth_ReturnGoodValue()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(57);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));
		decimal[] expectedSurvivalUnisexProbabilities = new decimal[decrementDates.Count];
		for (int i = 0; i < decrementDates.Count; i++)
		{
			if (decrementDates[i].DayOfYear != 1)
			{
				var decrementProbInYearMan = DecrementTableMocked.Object.GetRate(ManIndividualMocked.Object, decrementDates[i].FirstDayOfTheYear());
				var decrementProbInYearWoman = DecrementTableMocked.Object.GetRate(WomanIndividualMocked.Object, decrementDates[i].FirstDayOfTheYear());
				DecrementTableMocked.Setup(x => x.GetRate(ManIndividualMocked.Object, decrementDates[i]))
					.Returns(decrementProbInYearMan);
				DecrementTableMocked.Setup(x => x.GetRate(WomanIndividualMocked.Object, decrementDates[i]))
					.Returns(decrementProbInYearWoman);
			}
		}

		// Act
		for (int i = 0; i < decrementDates.Count; i++)
			expectedSurvivalUnisexProbabilities[i] = Decrement.SurvivalUnisexProbability(ManIndividualMocked.Object, calculationDate, decrementDates[i], MANPROPORTION);

		var actualSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(WomanIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);

		// Assert
		for (int i = 0; i < decrementDates.Count; i++)
			Assert.AreEqual(expectedSurvivalUnisexProbabilities[i], actualSurvivalUnisexProbabilities[i], 30 * Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.SurvivalUnisexProbabilities))]
	public void SurvivalUnisexProbabilities_CallingForSameSurvivalUnisexProbabilities_ReturnFromCacheSameObject()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var expectedSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);
		var actualSurvivalUnisexProbabilities = Decrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);

		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbabilities, actualSurvivalUnisexProbabilities);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.DecrementUnisexProbabilities))]
	public void DecrementUnisexProbabilities_ManProportionIsLowerThanZero_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var expectedSurvivalUnisexProbabilities = Decrement.DecrementUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);
		var actualSurvivalUnisexProbabilities = Decrement.DecrementUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);

		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => Decrement.DecrementUnisexProbabilities(ManIndividualMocked.Object, CalculationDate.AddYears(2), decrementDates, -1));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.DecrementUnisexProbabilities))]
	public void DecrementUnisexProbabilities_ManProportionIsHigherThanOne_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var expectedSurvivalUnisexProbabilities = Decrement.DecrementUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);
		var actualSurvivalUnisexProbabilities = Decrement.DecrementUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);

		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => Decrement.DecrementUnisexProbabilities(ManIndividualMocked.Object, CalculationDate.AddYears(2), decrementDates, 1.02m));
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.DecrementUnisexProbabilities))]
	public void DecrementUnisexProbabilities_Man_ManProportionIsExactlyOne_ReturnsManProbabilities()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var actualSurvivalUnisexProbabilities = Decrement.DecrementProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates);
		var expectedSurvivalUnisexProbabilities = Decrement.DecrementUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, 1m);

		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbabilities, actualSurvivalUnisexProbabilities);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.DecrementUnisexProbabilities))]
	public void DecrementUnisexProbabilities_Woman_ManProportionIsExactlyOne_ReturnsManProbabilities()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var actualSurvivalUnisexProbabilities = Decrement.DecrementProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates);
		var expectedSurvivalUnisexProbabilities = Decrement.DecrementUnisexProbabilities(WomanIndividualMocked.Object, calculationDate, decrementDates, 1m);

		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbabilities, actualSurvivalUnisexProbabilities);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.DecrementUnisexProbabilities))]
	public void DecrementUnisexProbabilities_Man_ManProportionIsExactlyZero_ReturnsWomanProbabilities()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var actualSurvivalUnisexProbabilities = Decrement.DecrementProbabilities(WomanIndividualMocked.Object, calculationDate, decrementDates);
		var expectedSurvivalUnisexProbabilities = Decrement.DecrementUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, 0);

		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbabilities, actualSurvivalUnisexProbabilities);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.DecrementUnisexProbabilities))]
	public void DecrementUnisexProbabilities_Woman_ManProportionIsExactlyZero_ReturnsWomanProbabilities()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));


		// Act
		var actualSurvivalUnisexProbabilities = Decrement.DecrementProbabilities(WomanIndividualMocked.Object, calculationDate, decrementDates);
		var expectedSurvivalUnisexProbabilities = Decrement.DecrementUnisexProbabilities(WomanIndividualMocked.Object, calculationDate, decrementDates, 0);

		// Assert
		Assert.AreEqual(expectedSurvivalUnisexProbabilities, actualSurvivalUnisexProbabilities);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.DecrementUnisexProbabilities))]
	public void DecrementUnisexProbabilities_Man_CalculationDateInMiddleOfAYear_DecrementEachMonth_ReturnGoodValue()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));
		decimal[] expectedDecrementProbabilities = new decimal[decrementDates.Count];
		var newDecrement = new GenderedDecrement<IGenderedIndividual>(DecrementTableMocked.Object, ImprovementMocked.Object, AdjustmentMocked.Object);

		// Act
		var actualDecrementProbabilities = newDecrement.DecrementUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);
		var expectedSurvivalUnisexProbabilities = newDecrement.SurvivalUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);
		for (int i = 0; i < decrementDates.Count; i++)
			expectedDecrementProbabilities[i] = 1 - expectedSurvivalUnisexProbabilities[i];


		// Assert
		CollectionAssert.AreEqual(expectedSurvivalUnisexProbabilities, actualDecrementProbabilities);
	}
	[TestMethod]
	[TestCategory(nameof(GenderedDecrement<IGenderedIndividual>.DecrementUnisexProbabilities))]
	public void DecrementUnisexProbabilities_Woman_CalculationDateInMiddleOfAYear_DecrementEachMonth_ReturnGoodValue()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(2));
		decimal[] expectedDecrementProbabilities = new decimal[decrementDates.Count];
		var newDecrement = new GenderedDecrement<IGenderedIndividual>(DecrementTableMocked.Object, ImprovementMocked.Object, AdjustmentMocked.Object);

		// Act
		var actualDecrementProbabilities = newDecrement.DecrementUnisexProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);
		var expectedSurvivalUnisexProbabilities = newDecrement.SurvivalUnisexProbabilities(WomanIndividualMocked.Object, calculationDate, decrementDates, MANPROPORTION);
		for (int i = 0; i < decrementDates.Count; i++)
			expectedDecrementProbabilities[i] = 1 - expectedSurvivalUnisexProbabilities[i];


		// Assert
		CollectionAssert.AreEqual(expectedSurvivalUnisexProbabilities, actualDecrementProbabilities);
	}
}
