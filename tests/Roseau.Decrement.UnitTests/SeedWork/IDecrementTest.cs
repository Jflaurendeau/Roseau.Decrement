using Roseau.DateHelpers;
using Roseau.Decrement.SeedWork;
using Roseau.Mathematics;

namespace Roseau.Decrement.UnitTests.SeedWork;

[TestClass]
public class IDecrementTest
{
	[TestMethod]
	[TestCategory(nameof(IDecrement.UniformDecrementDistribution))]
	public void UniformDecrementDistribution_SecondDateEarlierThanFirstDate_ThrowsArgumentException()
	{
		// Arrange
		DateOnly firstDate = new DateOnly();
		DateOnly secondDate = firstDate.AddYears(1);
		// Act
		// Assert
		Assert.ThrowsException<ArgumentException>(() => IDecrement.UniformDecrementDistribution(0.5m, secondDate, firstDate));
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement.UniformDecrementDistribution))]
	public void UniformDecrementDistribution_SecondDateInSameYearAsFirstDate_GoodResult()
	{
		// Arrange
		DateOnly firstDate = new DateOnly(2020, 11, 15);
		DateOnly firstDateStartOfYear = new DateOnly(firstDate.Year, 1, 1);
		DateOnly secondDate = firstDate.AddDays(1);
		decimal decrementProbability = 0.5m;
		// Act
		decimal deathFirstPartOfYear = (decimal)(firstDate.DayNumber - firstDateStartOfYear.DayNumber) 
			/ (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber)
			* decrementProbability;
		decimal deathSecondPartOfYear = 1m / (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber) * decrementProbability;
		decimal expected = deathSecondPartOfYear / (1 - deathFirstPartOfYear);

		// Assert
		Assert.AreEqual(expected, IDecrement.UniformDecrementDistribution(decrementProbability, firstDate, secondDate), Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement.UniformDecrementDistribution))]
	public void UniformDecrementDistribution_SecondDateInDifferentYearThanFirstDate_GoodResult()
	{
		// Arrange
		DateOnly firstDate = new DateOnly(2020, 11, 15);
		DateOnly firstDateStartOfYear = new DateOnly(firstDate.Year, 1, 1);
		DateOnly secondDate = firstDate.AddYears(1);
		decimal decrementProbability = 0.5m;
		// Act
		decimal deathFirstPartOfYear = (decimal)(firstDate.DayNumber - firstDateStartOfYear.DayNumber)
			/ (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber)
			* decrementProbability;
		decimal deathSecondPartOfYear = (decimal)(firstDate.FirstDayOfFollowingYear().DayNumber - firstDate.DayNumber) / (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber) * decrementProbability;
		decimal expected = deathSecondPartOfYear / (1 - deathFirstPartOfYear);

		// Assert
		Assert.AreEqual(expected, IDecrement.UniformDecrementDistribution(decrementProbability, firstDate, secondDate), Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement.UniformSurvivalDistribution))]
	public void UniformSurvivalDistribution_SecondDateEarlierThanFirstDate_ThrowsArgumentException()
	{
		// Arrange
		DateOnly firstDate = new DateOnly();
		DateOnly secondDate = firstDate.AddYears(1);
		// Act
		// Assert
		Assert.ThrowsException<ArgumentException>(() => IDecrement.UniformSurvivalDistribution(0.5m, secondDate, firstDate));
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement.UniformSurvivalDistribution))]
	public void UniformSurvivalDistribution_SecondDateInSameYearAsFirstDate_GoodResult()
	{
		// Arrange
		DateOnly firstDate = new DateOnly(2020, 11, 15);
		DateOnly firstDateStartOfYear = new DateOnly(firstDate.Year, 1, 1);
		DateOnly secondDate = firstDate.AddDays(1);
		decimal decrementProbability = 0.5m;
		// Act
		decimal deathFirstPartOfYear = (decimal)(firstDate.DayNumber - firstDateStartOfYear.DayNumber)
			/ (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber)
			* decrementProbability;
		decimal deathSecondPartOfYear = 1m / (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber) * decrementProbability;
		decimal expected = deathSecondPartOfYear / (1 - deathFirstPartOfYear);

		// Assert
		Assert.AreEqual(1 - expected, IDecrement.UniformSurvivalDistribution(decrementProbability, firstDate, secondDate), Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement.UniformSurvivalDistribution))]
	public void UniformSurvivalDistribution_SecondDateInDifferentYearThanFirstDate_GoodResult()
	{
		// Arrange
		DateOnly firstDate = new DateOnly(2020, 11, 15);
		DateOnly firstDateStartOfYear = new DateOnly(firstDate.Year, 1, 1);
		DateOnly secondDate = firstDate.AddYears(1);
		decimal decrementProbability = 0.5m;
		// Act
		decimal deathFirstPartOfYear = (decimal)(firstDate.DayNumber - firstDateStartOfYear.DayNumber)
			/ (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber)
			* decrementProbability;
		decimal deathSecondPartOfYear = (decimal)(firstDate.FirstDayOfFollowingYear().DayNumber - firstDate.DayNumber) / (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber) * decrementProbability;
		decimal expected = deathSecondPartOfYear / (1 - deathFirstPartOfYear);

		// Assert
		Assert.AreEqual(1 - expected, IDecrement.UniformSurvivalDistribution(decrementProbability, firstDate, secondDate), Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement.ConstantForceDecrement))]
	public void ConstantForceDecrement_SecondDateEarlierThanFirstDate_ThrowsArgumentException()
	{
		// Arrange
		DateOnly firstDate = new DateOnly();
		DateOnly secondDate = firstDate.AddYears(1);
		// Act
		// Assert
		Assert.ThrowsException<ArgumentException>(() => IDecrement.ConstantForceDecrement(0.5m, secondDate, firstDate));
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement.ConstantForceDecrement))]
	public void ConstantForceDecrement_SecondDateInSameYearAsFirstDate_GoodResult()
	{
		// Arrange
		DateOnly firstDate = new DateOnly(2020, 11, 15);
		DateOnly firstDateStartOfYear = new DateOnly(firstDate.Year, 1, 1);
		DateOnly secondDate = firstDate.AddDays(1);
		decimal decrementProbability = 0.5m;
		// Act
		decimal timeSecondPartOfYear = 1m / (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber);
		decimal expected = Maths.Pow(1 - decrementProbability, timeSecondPartOfYear);

		// Assert
		Assert.AreEqual(1 - expected, IDecrement.ConstantForceDecrement(decrementProbability, firstDate, secondDate), Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement.ConstantForceDecrement))]
	public void ConstantForceDecrement_SecondDateInDifferentYearThanFirstDate_GoodResult()
	{
		// Arrange
		DateOnly firstDate = new DateOnly(2020, 11, 15);
		DateOnly firstDateStartOfYear = new DateOnly(firstDate.Year, 1, 1);
		DateOnly secondDate = firstDate.AddYears(1);
		decimal decrementProbability = 0.5m;
		// Act
		decimal timeSecondPartOfYear = (decimal)(firstDate.FirstDayOfFollowingYear().DayNumber - firstDate.DayNumber) / (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber);
		decimal expected = Maths.Pow(1 - decrementProbability, timeSecondPartOfYear);

		// Assert
		Assert.AreEqual(1 - expected, IDecrement.ConstantForceDecrement(decrementProbability, firstDate, secondDate), Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement.ConstantForceSurvival))]
	public void ConstantForceSurvival_SecondDateEarlierThanFirstDate_ThrowsArgumentException()
	{
		// Arrange
		DateOnly firstDate = new DateOnly();
		DateOnly secondDate = firstDate.AddYears(1);
		// Act
		// Assert
		Assert.ThrowsException<ArgumentException>(() => IDecrement.ConstantForceSurvival(0.5m, secondDate, firstDate));
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement.ConstantForceSurvival))]
	public void ConstantForceSurvival_SecondDateInSameYearAsFirstDate_GoodResult()
	{
		// Arrange
		DateOnly firstDate = new DateOnly(2020, 11, 15);
		DateOnly firstDateStartOfYear = new DateOnly(firstDate.Year, 1, 1);
		DateOnly secondDate = firstDate.AddDays(1);
		decimal decrementProbability = 0.5m;
		// Act
		decimal timeSecondPartOfYear = 1m / (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber);
		decimal expected = Maths.Pow(1 - decrementProbability, timeSecondPartOfYear);

		// Assert
		Assert.AreEqual(expected, IDecrement.ConstantForceSurvival(decrementProbability, firstDate, secondDate), Maths.Epsilon);
	}
	[TestMethod]
	[TestCategory(nameof(IDecrement.ConstantForceSurvival))]
	public void ConstantForceSurvival_SecondDateInDifferentYearThanFirstDate_GoodResult()
	{
		// Arrange
		DateOnly firstDate = new DateOnly(2020, 11, 15);
		DateOnly firstDateStartOfYear = new DateOnly(firstDate.Year, 1, 1);
		DateOnly secondDate = firstDate.AddYears(1);
		decimal decrementProbability = 0.5m;
		// Act
		decimal timeSecondPartOfYear = (decimal)(firstDate.FirstDayOfFollowingYear().DayNumber - firstDate.DayNumber) / (firstDateStartOfYear.AddYears(1).DayNumber - firstDateStartOfYear.DayNumber);
		decimal expected = Maths.Pow(1 - decrementProbability, timeSecondPartOfYear);

		// Assert
		Assert.AreEqual(expected, IDecrement.ConstantForceSurvival(decrementProbability, firstDate, secondDate), Maths.Epsilon);
	}
}
