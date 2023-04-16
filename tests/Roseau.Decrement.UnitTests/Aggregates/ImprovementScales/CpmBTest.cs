using Moq;
using Roseau.Decrement.Aggregates.Decrements.ImprovementScales;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.SeedWork;

namespace Roseau.Decrement.UnitTests.Aggregates.ImprovementScales;

[TestClass]
public class CpmBTest
{
	private readonly Mock<IAdjustment<IGenderedIndividual>> adjustmentFactorMocked = new();
	private readonly Mock<IGenderedIndividual> personMocked = new();

	[TestMethod]
	[DataRow(2014, 1, 1)]
	[DataRow(2014, 2, 2)]
	[DataRow(2014, 5, 5)]
	[DataRow(2014, 9, 9)]
	[DataRow(2014, 12, 1)]
	[DataRow(2014, 12, 31)]
	public void MortalityImprovementFactor_CalculationDateIn2014_ReturnOne(int year, int month, int day)
	{
		// Arrange
		DateOnly calculationDate = new(year, month, day);
		CpmB cPMB = new();
		DateOnly dateOfBirth = new(1991, 1, 1);
		personMocked.Setup(x => x.Gender)
			.Returns(Gender.Man);
		personMocked.Setup(x => x.DateOfBirth)
			.Returns(dateOfBirth);

		// Act
		decimal improvementScale = cPMB.ImprovementFactor(personMocked.Object, 2014, calculationDate);

		// Assert
		Assert.AreEqual(Decimal.One, improvementScale);
	}
	[TestMethod]
	[DataRow(2013, 1, 1, 17)]
	[DataRow(2013, 3, 3, 18)]
	[DataRow(2013, 3, 3, 60)]
	[DataRow(2013, 3, 3, 114)]
	[DataRow(2013, 3, 3, 115)]
	[DataRow(2013, 3, 3, 116)]
	[DataRow(2013, 12, 31, 200)]
	public void MortalityImprovementFactor_MaleGenderCalculationDateIn2013_ReturnRightFactor(int year, int month, int day, int age)
	{
		// Arrange
		DateOnly calculationDate = new(year, month, day);
		DateOnly dateOfBirth = calculationDate.AddYears(-age);
		personMocked.Setup(x => x.Gender)
			.Returns(Gender.Man);
		personMocked.Setup(x => x.DateOfBirth)
			.Returns(dateOfBirth);
		CpmB cPMB = new();
		decimal expectedFactor = 0m;
		decimal[][] listFactor = {
				new decimal[]
				{
					17m,
					18m,
					30m,
					55m,
					60m,
					65m,
					80m,
					100m,
					114m,
					115m,
					116m,
					200m,
				},
				new decimal[]
				{
					0.02316m,
					0.02316m,
					0.02526m,
					0.01867m,
					0.02344m,
					0.02821m,
					0.02653m,
					0.00047m,
					0.00022m,
					0m,
					0m,
					0m,
				}
			};

		// Act
		for (int i = 0; i < listFactor[0].Length; i++)
		{
			if (listFactor[0][i].Equals(age))
				expectedFactor = 1 / (1 - listFactor[1][i]);
		}
		decimal improvementScale = cPMB.ImprovementFactor(personMocked.Object, 2014, calculationDate);

		// Assert
		Assert.AreEqual(expectedFactor, improvementScale);
	}
	[TestMethod]
	[DataRow(2013, 1, 1, 17)]
	[DataRow(2013, 3, 3, 18)]
	[DataRow(2013, 3, 3, 60)]
	[DataRow(2013, 3, 3, 114)]
	[DataRow(2013, 3, 3, 115)]
	[DataRow(2013, 3, 3, 116)]
	[DataRow(2013, 12, 31, 200)]
	public void MortalityImprovementFactor_FemaleGenderCalculationDateIn2013_ReturnRightFactor(int year, int month, int day, int age)
	{
		// Arrange
		DateOnly calculationDate = new(year, month, day);
		DateOnly dateOfBirth = calculationDate.AddYears(-age);
		personMocked.Setup(x => x.Gender)
			.Returns(Gender.Woman);
		personMocked.Setup(x => x.DateOfBirth)
			.Returns(dateOfBirth);
		CpmB cPMB = new();
		decimal expectedFactor = 0m;
		decimal[][] listFactor = {
				new decimal[]
				{
					17m,
					18m,
					30m,
					55m,
					60m,
					65m,
					80m,
					100m,
					114m,
					115m,
					116m,
					200m,
				},
				new decimal[]
				{
					0.01432m,
					0.01432m,
					0.01095m,
					0.01297m,
					0.01499m,
					0.01701m,
					0.01701m,
					0.00047m,
					0.00022m,
					0m,
					0m,
					0m,
				}
			};

		// Act
		for (int i = 0; i < listFactor[0].Length; i++)
		{
			if (listFactor[0][i].Equals(age))
				expectedFactor = 1 / (1 - listFactor[1][i]);
		}
		decimal improvementScale = cPMB.ImprovementFactor(personMocked.Object, 2014, calculationDate);

		// Assert
		Assert.AreEqual(expectedFactor, improvementScale);
	}
	[TestMethod]
	[DataRow(2000, 1, 1, 17)]
	[DataRow(2000, 3, 3, 18)]
	[DataRow(2000, 3, 3, 60)]
	[DataRow(2000, 3, 3, 114)]
	[DataRow(2000, 3, 3, 115)]
	[DataRow(2000, 3, 3, 116)]
	[DataRow(2000, 12, 31, 200)]
	public void MortalityImprovementFactor_MaleGenderCalculationDateIn2000_ReturnRightFactor(int year, int month, int day, int age)
	{
		// Arrange
		DateOnly calculationDate = new(year, month, day);
		DateOnly dateOfBirth = calculationDate.AddYears(-age);
		personMocked.Setup(x => x.Gender)
			.Returns(Gender.Man);
		personMocked.Setup(x => x.DateOfBirth)
			.Returns(dateOfBirth);
		CpmB cPMB = new();
		decimal expectedFactor = 1m;

		decimal[] list18 = new decimal[] {
				0.026m,
				0.026m,
				0.026m,
				0.026m,
				0.026m,
				0.026m,
				0.026m,
				0.026m,
				0.026m,
				0.026m,
				0.026m,
				0.02505m,
				0.02411m,
				0.02316m,
			};
		decimal[] list60 = new decimal[] {
				0.02633m,
				0.02633m,
				0.02633m,
				0.02633m,
				0.02633m,
				0.02633m,
				0.02633m,
				0.02633m,
				0.02633m,
				0.02633m,
				0.02633m,
				0.02537m,
				0.0244m,
				0.02344m,
			};
		decimal[] list114 = new decimal[] {
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0.00007m,
				0.00015m,
				0.00022m,
			};
		decimal[] list115 = new decimal[] {
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
			};

		// Act
		if (age <= 18)
			foreach (decimal decim in list18)
				expectedFactor /= (1 - decim);
		if (age == 60)
			foreach (decimal decim in list60)
				expectedFactor /= (1 - decim);
		if (age == 114)
			foreach (decimal decim in list114)
				expectedFactor /= (1 - decim);
		if (age == 115)
			foreach (decimal decim in list115)
				expectedFactor /= (1 - decim);
		if (age == 116)
			foreach (decimal decim in list115)
				expectedFactor /= (1 - decim);
		if (age == 200)
			foreach (decimal decim in list115)
				expectedFactor /= (1 - decim);

		decimal improvementScale = cPMB.ImprovementFactor(personMocked.Object, 2014, calculationDate);

		// Assert
		Assert.AreEqual(expectedFactor, improvementScale);
	}
	[TestMethod]
	[DataRow(1998, 1, 1, 17)]
	[DataRow(1998, 3, 3, 18)]
	[DataRow(1998, 3, 3, 60)]
	[DataRow(1998, 3, 3, 114)]
	[DataRow(1998, 3, 3, 115)]
	[DataRow(1998, 3, 3, 116)]
	[DataRow(1998, 12, 31, 200)]
	public void MortalityImprovementFactor_MaleGenderCalculationDateIn1998_ThrowArgumentOutOfRangeException(int year, int month, int day, int age)
	{
		// Arrange
		DateOnly calculationDate = new(year, month, day);
		DateOnly dateOfBirth = calculationDate.AddYears(-age);
		personMocked.Setup(x => x.Gender)
			.Returns(Gender.Man);
		personMocked.Setup(x => x.DateOfBirth)
			.Returns(dateOfBirth);
		CpmB cPMB = new();


		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => cPMB.ImprovementFactor(personMocked.Object, 2014, calculationDate));

	}
	[TestMethod]
	[DataRow(2000, 1, 1, 17)]
	[DataRow(2000, 3, 3, 18)]
	[DataRow(2000, 3, 3, 60)]
	[DataRow(2000, 3, 3, 114)]
	[DataRow(2000, 3, 3, 115)]
	[DataRow(2000, 3, 3, 116)]
	[DataRow(2000, 12, 31, 200)]
	public void MortalityImprovementFactor_FemaleGenderCalculationDateIn2000_ReturnRightFactor(int year, int month, int day, int age)
	{
		// Arrange
		DateOnly calculationDate = new(year, month, day);
		DateOnly dateOfBirth = calculationDate.AddYears(-age);
		personMocked.Setup(x => x.Gender)
			.Returns(Gender.Woman);
		personMocked.Setup(x => x.DateOfBirth)
			.Returns(dateOfBirth);
		CpmB cPMB = new();
		decimal expectedFactor = 1m;

		decimal[] list18 = new decimal[] {
				0.0155m,
				0.0155m,
				0.0155m,
				0.0155m,
				0.0155m,
				0.0155m,
				0.0155m,
				0.0155m,
				0.0155m,
				0.0155m,
				0.0155m,
				0.01511m,
				0.01471m,
				0.01432m,
			};
		decimal[] list60 = new decimal[] {
				0.0163m,
				0.0163m,
				0.0163m,
				0.0163m,
				0.0163m,
				0.0163m,
				0.0163m,
				0.0163m,
				0.0163m,
				0.0163m,
				0.0163m,
				0.01586m,
				0.01543m,
				0.01499m,
			};
		decimal[] list114 = new decimal[] {
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0.00007m,
				0.00015m,
				0.00022m,
			};
		decimal[] list115 = new decimal[] {
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
				0m,
			};

		// Act
		if (age <= 18)
			foreach (decimal decim in list18)
				expectedFactor /= (1 - decim);
		if (age == 60)
			foreach (decimal decim in list60)
				expectedFactor /= (1 - decim);
		if (age == 114)
			foreach (decimal decim in list114)
				expectedFactor /= (1 - decim);
		if (age == 115)
			foreach (decimal decim in list115)
				expectedFactor /= (1 - decim);
		if (age == 116)
			foreach (decimal decim in list115)
				expectedFactor /= (1 - decim);
		if (age == 200)
			foreach (decimal decim in list115)
				expectedFactor /= (1 - decim);

		decimal improvementScale = cPMB.ImprovementFactor(personMocked.Object, 2014, calculationDate);

		// Assert
		Assert.AreEqual(expectedFactor, improvementScale);
	}
	[TestMethod]
	[DataRow(1998, 1, 1, 17)]
	[DataRow(1998, 3, 3, 18)]
	[DataRow(1998, 3, 3, 60)]
	[DataRow(1998, 3, 3, 114)]
	[DataRow(1998, 3, 3, 115)]
	[DataRow(1998, 3, 3, 116)]
	[DataRow(1998, 12, 31, 200)]
	public void MortalityImprovementFactor_FemaleGenderCalculationDateIn1998_ThrowArgumentOutOfRangeException(int year, int month, int day, int age)
	{
		// Arrange
		DateOnly calculationDate = new(year, month, day);
		DateOnly dateOfBirth = calculationDate.AddYears(-age);
		personMocked.Setup(x => x.Gender)
			.Returns(Gender.Woman);
		personMocked.Setup(x => x.DateOfBirth)
			.Returns(dateOfBirth);
		CpmB cPMB = new();


		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => cPMB.ImprovementFactor(personMocked.Object, 2014, calculationDate));

	}
	[TestMethod]
	[DataRow(1998, 1, 1, 17)]
	[DataRow(1998, 3, 3, 18)]
	[DataRow(1998, 3, 3, 60)]
	[DataRow(1998, 3, 3, 114)]
	[DataRow(1998, 3, 3, 115)]
	[DataRow(1998, 3, 3, 116)]
	[DataRow(1998, 12, 31, 200)]
	public void MortalityImprovementFactor_MaleGenderCalculationDateIn1998WithAdjustmentFactor_ThrowArgumentOutOfRangeException(int year, int month, int day, int age)
	{
		// Arrange
		DateOnly calculationDate = new(year, month, day);
		DateOnly dateOfBirth = calculationDate.AddYears(-age);
		personMocked.Setup(x => x.Gender)
			.Returns(Gender.Man);
		personMocked.Setup(x => x.DateOfBirth)
			.Returns(dateOfBirth);
		adjustmentFactorMocked.Setup(x => x.AdjustmentFactor(personMocked.Object))
			.Returns(0.3m);
		CpmB cPMB = new(adjustmentFactorMocked.Object);


		// Assert
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => cPMB.ImprovementFactor(personMocked.Object, 2014, calculationDate));

	}
}
