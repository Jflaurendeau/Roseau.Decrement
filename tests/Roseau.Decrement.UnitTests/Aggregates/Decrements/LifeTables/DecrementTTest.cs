using Moq;
using Roseau.DateHelpers;
using Roseau.DateHelpers.DateArrayStrategies;
using Roseau.Decrement.Aggregates.Decrements.LifeTables;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;
using Roseau.Mathematics;

namespace Roseau.Decrement.UnitTests.Aggregates.Decrements.LifeTables;

[TestClass]
public class DecrementTTest
{
	private const int NUMBEROFYEARS = 115-17;
	private static Mock<IDecrementTable<IGenderedIndividual>> Cpm2014TableMocked { get; } = new();
	private static Mock<IImprovement<IGenderedIndividual>> ImprovementMocked { get; } = new();
	private static Mock<IAdjustment<IGenderedIndividual>> AdjustmentMocked { get; } = new();
	private static Mock<IGenderedIndividual> ManIndividualMocked { get; } = new();
	private static Mock<IGenderedIndividual> WomanIndividualMocked { get; } = new();
	private static DateOnly CalculationDate { get; } = new(2018, 1, 1);
	private static decimal[][] SurvivalProbabilities { get; } = new decimal[2][];
	private static decimal[][] DeathProbabilities { get; } = new decimal[2][];
	private static Decrement<IGenderedIndividual> Cpm2014 { get; }  = new Decrement<IGenderedIndividual>(Cpm2014TableMocked.Object, ImprovementMocked.Object, AdjustmentMocked.Object);
	private static DateOnly[] SurvivalDates { get; } = new DateOnly[NUMBEROFYEARS];

	[ClassInitialize]
	public static void Initialize(TestContext _)
	{
		ImprovementMocked.CallBase = true;
		Cpm2014TableMocked.CallBase = true;
		ManIndividualMocked.Setup(x => x.Gender)
			.Returns(Gender.Man);
		WomanIndividualMocked.Setup(x => x.Gender)
			.Returns(Gender.Woman);
		ManIndividualMocked.Setup(x => x.DateOfBirth)
			.Returns(new DateOnly(2000, 1, 1));
		WomanIndividualMocked.Setup(x => x.DateOfBirth)
			.Returns(new DateOnly(2000, 1, 1));
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
			Cpm2014TableMocked.Setup(x => x.GetRate(ManIndividualMocked.Object, SurvivalDates[i]))
				.Returns(manDeathProbabilities[i]);
			Cpm2014TableMocked.Setup(x => x.GetRate(WomanIndividualMocked.Object, SurvivalDates[i]))
				.Returns(womanDeathProbabilities[i]);
		}
		Cpm2014TableMocked.Setup(x => x.FirstAge)
			.Returns(18);
		Cpm2014TableMocked.Setup(x => x.BaseYear)
			.Returns(2018);
		Cpm2014TableMocked.Setup(x => x.LastAge)
			.Returns(115);
	}

	[TestMethod]
	[TestCategory("Constructors")]
	public void Constructor_WithANullTable_ThrowsArguementNullException()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentNullException>(() => new Decrement<IGenderedIndividual>(null!));
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.SurvivalProbability))]
	public void SurvivalProbability_DecrementDateBeforeCalculationDate_ThrowsArguementException()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentException>(() => Cpm2014.SurvivalProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate));
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.SurvivalProbability))]
	public void SurvivalProbability_DateOfBirthIsNotBeforeCalculationDate_ThrowsArguementException()
	{
		// Arrange

		// Act
		// Assert
		Assert.ThrowsException<ArgumentException>(() => Cpm2014.SurvivalProbability(ManIndividualMocked.Object, ManIndividualMocked.Object.DateOfBirth.AddDays(-1), CalculationDate));
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.SurvivalProbability))]
	public void SurvivalProbability_ManAndWomanHaveTheirOwnDifferentTable_ReturnsDifferentValue()
	{
		// Arrange

		// Act
		var notExpectedSurvivalProbability = Cpm2014.SurvivalProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3));
		var actualSurvivalProbability = Cpm2014.SurvivalProbability(WomanIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3));
		// Assert
		Assert.AreNotEqual(notExpectedSurvivalProbability, actualSurvivalProbability);
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.SurvivalProbability))]
	public void SurvivalProbability_UnderFirstAge_ReturnFirstValue()
	{
		// Arrange

		// Act
		var expectedSurvivalProbability = 1 - DeathProbabilities[(int)ManIndividualMocked.Object.Gender][0];
		var actualSurvivalProbability = Cpm2014.SurvivalProbability(ManIndividualMocked.Object, CalculationDate.AddYears(-1), CalculationDate);
		// Assert
		Assert.AreEqual(expectedSurvivalProbability, actualSurvivalProbability);
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.SurvivalProbability))]
	public void SurvivalProbability_OverLastAge_ReturnLastValue()
	{
		// Arrange

		// Act
		var expectedSurvivalProbability = 1 - DeathProbabilities[(int)ManIndividualMocked.Object.Gender][^1];
		var actualSurvivalProbability = Cpm2014.SurvivalProbability(ManIndividualMocked.Object, CalculationDate.AddYears(100), CalculationDate.AddYears(101));
		// Assert
		Assert.AreEqual(expectedSurvivalProbability, actualSurvivalProbability);
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.SurvivalProbability))]
	public void SurvivalProbability_AgeIsInRangeOfTable_ReturnGoodValue()
	{
		// Arrange

		// Act
		var expectedSurvivalProbability = 1 - DeathProbabilities[(int)ManIndividualMocked.Object.Gender][2];
		var actualSurvivalProbability = Cpm2014.SurvivalProbability(ManIndividualMocked.Object, CalculationDate.AddYears(2), CalculationDate.AddYears(3));
		// Assert
		Assert.AreEqual(expectedSurvivalProbability, actualSurvivalProbability);
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.SurvivalProbability))]
	public void SurvivalProbability_AgeIsInRangeOfTable_CalculationInMiddleOfAYear_DecrementTwoYearsLater_ReturnGoodValue()
	{
		// Arrange
		var calculationDate = CalculationDate.AddYears(2).AddDays(56);
		var decrementDate = calculationDate.AddYears(2);
		var deathRateYear1 = DeathProbabilities[(int)ManIndividualMocked.Object.Gender][2];
		var deathRateYear2 = DeathProbabilities[(int)ManIndividualMocked.Object.Gender][3];
		var deathRateYear3 = DeathProbabilities[(int)ManIndividualMocked.Object.Gender][4];
		Cpm2014TableMocked.Setup(x => x.GetRate(ManIndividualMocked.Object, calculationDate))
				.Returns(deathRateYear1);
		Cpm2014TableMocked.Setup(x => x.GetRate(ManIndividualMocked.Object, decrementDate))
				.Returns(deathRateYear3);

		// Act
		var survivalProbabilityYear1 = IDecrement.UniformSurvivalDistribution(deathRateYear1, calculationDate, calculationDate.FirstDayOfFollowingYear());
		var survivalProbabilityYear2 = 1 - deathRateYear2;
		var survivalProbabilityYear3 = IDecrement.UniformSurvivalDistribution(deathRateYear3, decrementDate.FirstDayOfTheYear(), decrementDate);

		var expectedSurvivalProbability = survivalProbabilityYear1 * survivalProbabilityYear2 * survivalProbabilityYear3;
		var actualSurvivalProbability = Cpm2014.SurvivalProbability(ManIndividualMocked.Object, calculationDate, decrementDate);
		
		// Assert
		Assert.AreEqual(expectedSurvivalProbability, actualSurvivalProbability);
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.SurvivalProbabilities))]
	public void SurvivalProbabilities_FirstDecrementDateBeforeCalculationDate_ThrowsArguementException()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(100));

		// Act

		// Assert
		Assert.ThrowsException<ArgumentException>(() => Cpm2014.SurvivalProbabilities(ManIndividualMocked.Object, decrementDates[0].AddDays(1), decrementDates));
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.SurvivalProbabilities))]
	public void SurvivalProbabilities_DateOfBirthIsNotBeforeCalculationDate_ThrowsArguementException()
	{
		// Arrange
		DateOnly calculationDate = ManIndividualMocked.Object.DateOfBirth.AddDays(-1);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(100));

		// Act

		// Assert
		Assert.ThrowsException<ArgumentException>(() => Cpm2014.SurvivalProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates));
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.SurvivalProbabilities))]
	public void SurvivalProbabilities_CalculationDateInMiddleOfAYear_DecrementEachMonth_ReturnGoodValue()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(57);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(100));
		decimal[] expectedSurvivalProbabilities = new decimal[decrementDates.Count];
		for (int i = 0; i < decrementDates.Count; i++)
		{
			if (decrementDates[i].DayOfYear != 1)
			{
				var decrementProbInYear = Cpm2014TableMocked.Object.GetRate(ManIndividualMocked.Object, decrementDates[i].FirstDayOfTheYear());
				Cpm2014TableMocked.Setup(x => x.GetRate(ManIndividualMocked.Object, decrementDates[i]))
					.Returns(decrementProbInYear);
			}
		}

		// Act
		for (int i = 0; i < decrementDates.Count; i++)
			expectedSurvivalProbabilities[i] = Cpm2014.SurvivalProbability(ManIndividualMocked.Object, calculationDate, decrementDates[i]);

		var actualSurvivalProbabilities = Cpm2014.SurvivalProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates);

		// Assert
		for (int i = 0; i < decrementDates.Count; i++)
			Assert.AreEqual(expectedSurvivalProbabilities[i], actualSurvivalProbabilities[i], 20 * Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.SurvivalProbabilities))]
	public void SurvivalProbabilities_CallingForSameSurvivalProbabilities_ReturnFromCacheSameObject()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(100));


		// Act
		var	expectedSurvivalProbabilities = Cpm2014.SurvivalProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates);
		var actualSurvivalProbabilities = Cpm2014.SurvivalProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates);

		// Assert
		Assert.AreEqual(expectedSurvivalProbabilities, actualSurvivalProbabilities);
	}
	[TestMethod]
	[TestCategory(nameof(Cpm2014.DecrementProbabilities))]
	public void DecrementProbabilities_CalculationDateInMiddleOfAYear_DecrementEachMonth_ReturnGoodValue()
	{
		// Arrange
		DateOnly calculationDate = CalculationDate.AddDays(56);
		IDateArrayStrategy dateArrayStrategy = new FirstDayOfEveryMonthStrategy();
		OrderedDates decrementDates = new(dateArrayStrategy, calculationDate, calculationDate.AddYears(100));
		decimal[] expectedDecrementProbabilities = new decimal[decrementDates.Count];


		// Act
		var expectedSurvivalProbabilities = Cpm2014.SurvivalProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates);
		for (int i = 0; i < decrementDates.Count; i++)
			expectedDecrementProbabilities[i] = 1 - expectedSurvivalProbabilities[i];

		var actualDecrementProbabilities = Cpm2014.DecrementProbabilities(ManIndividualMocked.Object, calculationDate, decrementDates);

		// Assert
		CollectionAssert.AreEqual(expectedSurvivalProbabilities, actualDecrementProbabilities);
	}
}

