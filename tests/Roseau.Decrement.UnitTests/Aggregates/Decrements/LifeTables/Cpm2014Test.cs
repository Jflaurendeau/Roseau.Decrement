using Moq;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Decrements.ImprovementScales;
using Roseau.Decrement.Aggregates.Decrements.LifeTables;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;
using Roseau.Mathematics;

namespace Roseau.Decrement.UnitTests.Aggregates.Decrements.LifeTables;

[TestClass]
public class Cpm2014Test
{
    private readonly Mock<IAdjustment<IGenderedIndividual>> adjustmentFactorMocked = new();
    private readonly Mock<IGenderedIndividual> personMocked = new();
    private readonly Mock<IImprovement<IGenderedIndividual>> scaleMocked = new();
    private const decimal AcceptableError = 20 * Maths.Epsilon;

    [TestMethod]
    [DataRow(1)]
    [DataRow(16)]
    [DataRow(17)]
    public void SurvivalProbability_Male_YoungerThanMinimumLifeTableAge_OneYearProjection_ReturnMiniumSurvivalRate(int age)
    {
        // Arrange
        decimal survivalProbability;
        DateOnly calculationDate = new(2020, 1, 1);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Man);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age));
        adjustmentFactorMocked.Setup(x => x.AdjustmentFactor(personMocked.Object))
            .Returns(0.3m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate))
            .Returns(0.7m);

        Cpm2014Combined cPM2014LiteTable = new(scaleMocked.Object, adjustmentFactorMocked.Object);

        // Act
        survivalProbability = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, calculationDate.AddYears(1));

        // Assert
        Assert.AreEqual(1 - 0.00067m * 0.3m * 0.7m, survivalProbability);
    }
    [TestMethod]
    [DataRow(1)]
    [DataRow(16)]
    [DataRow(17)]
    public void SurvivalProbability_Female_YoungerThanMinimumLifeTableAge_OneYearProjection_ReturnMiniumSurvivalRate(int age)
    {
        // Arrange
        decimal survivalProbability;
        DateOnly calculationDate = new(2020, 1, 1);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Woman);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age));
        adjustmentFactorMocked.Setup(x => x.AdjustmentFactor(personMocked.Object))
            .Returns(0.3m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate))
            .Returns(0.7m);

        Cpm2014Combined cPM2014LiteTable = new(scaleMocked.Object, adjustmentFactorMocked.Object);

        // Act
        survivalProbability = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, calculationDate.AddYears(1));

        // Assert
        Assert.AreEqual(1 - 0.00015m * 0.3m * 0.7m, survivalProbability);
    }
    [TestMethod]
    [DataRow(115)]
    [DataRow(116)]
    [DataRow(200)]
    public void SurvivalProbability_Male_OlderThanMaximumLifeTableAge_OneYearProjection_ReturnZero(int age)
    {
        // Arrange
        decimal survivalProbability;
        DateOnly calculationDate = new(2020, 1, 1);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Man);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age));
        adjustmentFactorMocked.Setup(x => x.AdjustmentFactor(personMocked.Object))
            .Returns(0.3m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate))
            .Returns(0.7m);

        Cpm2014Combined cPM2014LiteTable = new(scaleMocked.Object, adjustmentFactorMocked.Object);

        // Act
        survivalProbability = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, calculationDate.AddYears(1));

        // Assert
        Assert.AreEqual(0m, survivalProbability);
    }
    [TestMethod]
    [DataRow(115)]
    [DataRow(116)]
    [DataRow(200)]
    public void SurvivalProbability_Female_OlderThanMaximumLifeTableAge_OneYearProjection_ReturnZero(int age)
    {
        // Arrange
        decimal survivalProbability;
        DateOnly calculationDate = new(2020, 1, 1);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Woman);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age));
        adjustmentFactorMocked.Setup(x => x.AdjustmentFactor(personMocked.Object))
            .Returns(0.3m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate))
            .Returns(0.7m);

        Cpm2014Combined cPM2014LiteTable = new(scaleMocked.Object, adjustmentFactorMocked.Object);

        // Act
        survivalProbability = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, calculationDate.AddYears(1));

        // Assert
        Assert.AreEqual(0m, survivalProbability);
    }
    [TestMethod]
    [DataRow(30, 0.0012)]
    [DataRow(65, 0.00844)]
    [DataRow(114, 0.66)]
    public void SurvivalProbability_Male_OneYearProjection_ReturnGoodAgeRate(int age, double deathRate)
    {
        // Arrange
        decimal expectedSurvivalProbability = 1 - (decimal)deathRate * 0.3m * 0.7m;
        decimal survivalProbability;
        DateOnly calculationDate = new(2020, 1, 1);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Man);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age));
        adjustmentFactorMocked.Setup(x => x.AdjustmentFactor(personMocked.Object))
            .Returns(0.3m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate))
            .Returns(0.7m);

        Cpm2014Combined cPM2014LiteTable = new(scaleMocked.Object, adjustmentFactorMocked.Object);

        // Act
        survivalProbability = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, calculationDate.AddYears(1));

        // Assert
        Assert.AreEqual(expectedSurvivalProbability, survivalProbability);
    }
    [TestMethod]
    [DataRow(30, 0.0003)]
    [DataRow(65, 0.00562)]
    [DataRow(114, 0.61)]
    public void SurvivalProbability_Female_OneYearProjection_ReturnGoodAgeRate(int age, double deathRate)
    {
        // Arrange
        decimal expectedSurvivalProbability = 1 - (decimal)deathRate * 0.7m * 0.3m;
        decimal survivalProbability;
        DateOnly calculationDate = new(2020, 1, 1);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Woman);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age));
        adjustmentFactorMocked.Setup(x => x.AdjustmentFactor(personMocked.Object))
            .Returns(0.3m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate))
            .Returns(0.7m);

        Cpm2014Combined cPM2014LiteTable = new(scaleMocked.Object, adjustmentFactorMocked.Object);

        // Act
        survivalProbability = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, calculationDate.AddYears(1));

        // Assert
        Assert.AreEqual(expectedSurvivalProbability, survivalProbability);
    }
    [TestMethod]
    [DataRow(67)]
    public void SurvivalProbability_Male_ThreeCalenderYearsProjection_ReturnGoodAgeRate(int age)
    {
        // Arrange
        decimal[] survivalList = new decimal[]
        {
                0.00981m,
                0.01066m,
                0.01166m
        };
        decimal expectedSurvivalProbability = 1;
        decimal survivalProbability;
        DateOnly calculationDate = new(2020, 1, 1);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Man);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age));
        adjustmentFactorMocked.Setup(x => x.AdjustmentFactor(personMocked.Object))
            .Returns(0.3m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate))
            .Returns(0.7m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate.AddYears(1)))
            .Returns(0.7m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate.AddYears(2)))
            .Returns(0.7m);

        Cpm2014Combined cPM2014LiteTable = new(scaleMocked.Object, adjustmentFactorMocked.Object);

        // Act
        survivalProbability = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, calculationDate.AddYears(3));
        foreach (decimal survival in survivalList)
            expectedSurvivalProbability *= 1 - survival * 0.3m * 0.7m;

        // Assert
        Assert.AreEqual(expectedSurvivalProbability, survivalProbability);
    }
    [TestMethod]
    [DataRow(67)]
    public void SurvivalProbability_Female_ThreeCalenderYearsProjection_ReturnGoodAgeRate(int age)
    {
        // Arrange
        decimal[] survivalList = new decimal[]
        {
                0.00675m,
                0.00739m,
                0.00809m,
        };
        decimal expectedSurvivalProbability = 1;
        decimal survivalProbability;
        DateOnly calculationDate = new(2020, 1, 1);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Woman);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age));
        adjustmentFactorMocked.Setup(x => x.AdjustmentFactor(personMocked.Object))
            .Returns(0.3m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate))
            .Returns(0.7m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate.AddYears(1)))
            .Returns(0.7m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate.AddYears(2)))
            .Returns(0.7m);

        Cpm2014Combined cPM2014LiteTable = new(scaleMocked.Object, adjustmentFactorMocked.Object);

        // Act
        survivalProbability = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, calculationDate.AddYears(3));
        foreach (decimal survival in survivalList)
            expectedSurvivalProbability *= 1 - survival * 0.3m * 0.7m;

        // Assert
        Assert.AreEqual(expectedSurvivalProbability, survivalProbability);
    }
    [TestMethod]
    [DataRow(67)]
    public void SurvivalProbability_Male_ThreeCalenderYearsProjectionWithPartialFirstAndLastYears_ReturnGoodAgeRate(int age)
    {
        // Arrange
        decimal[] survivalList = new decimal[]
        {
                0.00981m * 1 / 366m / (1 - 0.00981m * 0.3m * 0.7m * 365 / 366m),
                0.01066m,
                0.01166m * 29 / 365m
        };
        decimal expectedSurvivalProbability = 1;
        decimal survivalProbability;
        DateOnly calculationDate = new(2020, 12, 31);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Man);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age - 1));
        adjustmentFactorMocked.Setup(x => x.AdjustmentFactor(personMocked.Object))
            .Returns(0.3m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate))
            .Returns(0.7m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate.AddDays(1)))
            .Returns(0.7m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate.AddYears(1).AddDays(1)))
            .Returns(0.7m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate.AddYears(2).AddDays(1)))
            .Returns(0.7m);

        Cpm2014Combined cPM2014LiteTable = new(scaleMocked.Object, adjustmentFactorMocked.Object);

        // Act
        survivalProbability = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, calculationDate.AddYears(1).AddDays(30));
        foreach (decimal survival in survivalList)
            expectedSurvivalProbability *= 1 - survival * 0.3m * 0.7m;

        // Assert
        Assert.IsTrue(Math.Abs(expectedSurvivalProbability - survivalProbability) <= Maths.Epsilon);
    }
    [TestMethod]
    [DataRow(67)]
    public void SurvivalProbability_Female_ThreeCalenderYearsProjectionWithPartialFirstAndLastYears_ReturnGoodAgeRate(int age)
    {
        // Arrange
        decimal[] survivalList = new decimal[]
        {
                0.00675m * 1 / 366m / (1 - 0.00675m * 0.3m * 0.7m * 365 / 366m),
                0.00739m,
                0.00809m * 29 / 365m
        };
        decimal expectedSurvivalProbability = 1;
        decimal survivalProbability;
        DateOnly calculationDate = new(2020, 12, 31);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Woman);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age - 1));
        adjustmentFactorMocked.Setup(x => x.AdjustmentFactor(personMocked.Object))
            .Returns(0.3m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate))
            .Returns(0.7m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate.AddDays(1)))
            .Returns(0.7m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate.AddYears(1).AddDays(1)))
            .Returns(0.7m);
        scaleMocked.Setup(x => x.ImprovementFactor(personMocked.Object, 2014, calculationDate.AddYears(2).AddDays(1)))
            .Returns(0.7m);

        Cpm2014Combined cPM2014LiteTable = new(scaleMocked.Object, adjustmentFactorMocked.Object);

        // Act
        survivalProbability = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, calculationDate.AddYears(1).AddDays(30));
        foreach (decimal survival in survivalList)
            expectedSurvivalProbability *= 1 - survival * 0.3m * 0.7m;

        // Assert
        Assert.IsTrue(Math.Abs(expectedSurvivalProbability - survivalProbability) <= Maths.Epsilon);
    }
    [TestMethod]
    [DataRow(67)]
    public void SurvivalProbabilities_Male_ThreeCalenderYearsProjectionWithPartialFirstAndLastYears_PaymentEveryMonth_ReturnGoodAgeRate(int age)
    {
        // Arrange
        decimal[] expectedSurvivalProbability = new decimal[]
        {
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
        };
        decimal[] survivalProbabilities;
        decimal differenceBetweenSurvivalProbabilities = decimal.Zero;
        DateOnly calculationDate = new(2020, 12, 31);
        DateOnly[] dateOnlies = new DateOnly[]
        {
                new(2021, 1, 1),
                new(2021, 2, 1),
                new(2021, 3, 1),
                new(2021, 4, 1),
                new(2021, 5, 1),
                new(2021, 6, 1),
                new(2021, 7, 1),
                new(2021, 8, 1),
                new(2021, 9, 1),
                new(2021, 10, 1),
                new(2021, 11, 1),
                new(2021, 12, 1),
                new(2022, 1, 1),
                new(2022, 2, 1),
        };
        OrderedDates dates = new(dateOnlies);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Man);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age - 1));
        Cpm2014Combined cPM2014LiteTable = new();

        // Act
        survivalProbabilities = cPM2014LiteTable.SurvivalProbabilities(personMocked.Object, calculationDate, dates);
        for (int i = 0; i < expectedSurvivalProbability.Length; i++)
        {
            expectedSurvivalProbability[i] = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, dateOnlies[i]);
            differenceBetweenSurvivalProbabilities += Math.Abs(expectedSurvivalProbability[i] - survivalProbabilities[i]);
        }


        // Assert
        Assert.IsTrue(differenceBetweenSurvivalProbabilities <= Maths.Epsilon * expectedSurvivalProbability.Length);
    }
    [TestMethod]
    [DataRow(67)]
    public void SurvivalProbabilities_Female_ThreeCalenderYearsProjectionWithPartialFirstAndLastYears_PaymentEveryMonth_ReturnGoodAgeRate(int age)
    {
        // Arrange
        decimal[] expectedSurvivalProbability = new decimal[]
        {
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
                1.0m,
        };
        decimal[] survivalProbabilities;
        decimal differenceBetweenSurvivalProbabilities = decimal.Zero;
        DateOnly calculationDate = new(2020, 12, 31);
        DateOnly[] dateOnlies = new DateOnly[]
        {
                new(2021, 1, 1),
                new(2021, 2, 1),
                new(2021, 3, 1),
                new(2021, 4, 1),
                new(2021, 5, 1),
                new(2021, 6, 1),
                new(2021, 7, 1),
                new(2021, 8, 1),
                new(2021, 9, 1),
                new(2021, 10, 1),
                new(2021, 11, 1),
                new(2021, 12, 1),
                new(2022, 1, 1),
                new(2022, 2, 1),
        };
        OrderedDates dates = new(dateOnlies);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Woman);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age - 1));
        Cpm2014Combined cPM2014LiteTable = new();

        // Act
        survivalProbabilities = cPM2014LiteTable.SurvivalProbabilities(personMocked.Object, calculationDate, dates);
        for (int i = 0; i < expectedSurvivalProbability.Length; i++)
        {
            expectedSurvivalProbability[i] = cPM2014LiteTable.SurvivalProbability(personMocked.Object, calculationDate, dateOnlies[i]);
            differenceBetweenSurvivalProbabilities += Math.Abs(expectedSurvivalProbability[i] - survivalProbabilities[i]);
        }


        // Assert
        Assert.IsTrue(differenceBetweenSurvivalProbabilities <= Maths.Epsilon * expectedSurvivalProbability.Length);
    }
    [TestMethod]
    [DataRow(2021, 65)]
    public void LifeExpectancy_Male_ReturnGoodValue(int year, int age)
    {
        // Arrange
        decimal expectedLifeExpectancy = 22.585921574891541164886932725M;
        decimal actualLifeExpectancy = 0m;
        DateOnly calculationDate = new(year, 1, 1);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Man);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age));
        IDecrement<IGenderedIndividual> cPM2014LifeTable = new Cpm2014Combined(new CpmB());

        // Act
        actualLifeExpectancy = cPM2014LifeTable.SurvivalExpectancy(personMocked.Object, calculationDate);
        var diff = Math.Abs(actualLifeExpectancy - expectedLifeExpectancy);

        // Assert
        Assert.IsTrue(diff <= AcceptableError);
    }
    [TestMethod]
    [DataRow(2021, 65)]
    public void LifeExpectancy_Female_ReturnGoodValue(int year, int age)
    {
        // Arrange
        decimal expectedLifeExpectancy = 24.801420265330239773057922196M;
        decimal actualLifeExpectancy = 0m;
        DateOnly calculationDate = new(year, 1, 1);
        personMocked.Setup(x => x.Gender)
            .Returns(Gender.Woman);
        personMocked.Setup(x => x.DateOfBirth)
            .Returns(calculationDate.AddYears(-age));
        IDecrement<IGenderedIndividual> cPM2014LifeTable = new Cpm2014Combined(new CpmB());

        // Act
        actualLifeExpectancy = cPM2014LifeTable.SurvivalExpectancy(personMocked.Object, calculationDate);

        // Assert
        Assert.AreEqual(expectedLifeExpectancy, actualLifeExpectancy);
    }
}