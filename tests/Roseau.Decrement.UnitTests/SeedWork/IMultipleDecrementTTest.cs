using Moq;
using Roseau.DateHelpers;
using Roseau.Decrement.Aggregates.Decrements.LifeTables;
using Roseau.Decrement.Aggregates.Individuals;
using Roseau.Decrement.Common.DecrementBetweenIntegralAgeStrategies;
using Roseau.Decrement.SeedWork;
using Roseau.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roseau.Decrement.UnitTests.SeedWork;

[TestClass]
public class IMultipleDecrementTTest
{
	private const int NUMBEROFYEARS = 116;
	private static Mock<IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>> decrement1Mocked { get; } = new();
	private static Mock<IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>> decrement2Mocked { get; } = new();
	private static Mock<IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>> decrement3Mocked { get; } = new();
	private static Mock<IMultipleDecrement<IIndividual, IDecrementBetweenIntegralAgeStrategy, IDecrementBetweenIntegralAge<IIndividual, IDecrementBetweenIntegralAgeStrategy>>> multipleDecrement { get; } = new();
	private static IDecrement<IIndividual> decrement =  new MultipleDecrement<IIndividual, IDecrementBetweenIntegralAgeStrategy, IDecrementBetweenIntegralAge<IIndividual, IDecrementBetweenIntegralAgeStrategy>>(new UniformDeathDistributionStrategy(), decrement1Mocked.Object, decrement2Mocked.Object, decrement3Mocked.Object, null);
	private static Mock<IIndividual> individualMocked { get; } = new();
	private static DateOnly calculationDate { get; } = new(2018, 1, 1);
	private static decimal[] survival1Probabilities { get; } = new decimal[NUMBEROFYEARS];
	private static decimal[] survival2Probabilities { get; } = new decimal[NUMBEROFYEARS];
	private static decimal[] survival3Probabilities { get; } = new decimal[NUMBEROFYEARS];
	private static decimal[] survivalProbabilities { get; } = new decimal[NUMBEROFYEARS];
	private static decimal[] decrementRate1 { get; } = new decimal[NUMBEROFYEARS];
	private static decimal[] decrementRate2 { get; } = new decimal[NUMBEROFYEARS];
	private static decimal[] decrementRate3 { get; } = new decimal[NUMBEROFYEARS];
	private static DateOnly[] survivalDates { get; } = new DateOnly[NUMBEROFYEARS];
	private static OrderedDates dates { get; } = new OrderedDates(survivalDates);
	private static MultipleDecrementProbability multipleDecrementProbability { get; } = new();
	private static MultipleDecrementProbabilities multipleDecrementProbabilities { get; } = new(survivalProbabilities, survival1Probabilities, survival2Probabilities, survival3Probabilities);

	[ClassInitialize]
	public static void Initialize(TestContext _)
	{
		decrement1Mocked.CallBase = true;
		decrement2Mocked.CallBase = true;
		decrement3Mocked.CallBase = true;
		multipleDecrement.CallBase = true;
		multipleDecrement.Setup(x => x.Disability)
						.Returns(decrement1Mocked.Object);
		multipleDecrement.Setup(x => x.Lapse)
						.Returns(decrement2Mocked.Object);
		multipleDecrement.Setup(x => x.Mortality)
						.Returns(decrement3Mocked.Object);
		multipleDecrement.Setup(x => x.DependentProbabilities(individualMocked.Object, calculationDate, dates))
						.Returns(multipleDecrementProbabilities);
		multipleDecrement.As<IDecrement<IIndividual>>().Setup(x => x.SurvivalProbabilities(individualMocked.Object, calculationDate, dates))
						.Returns(multipleDecrement.Object.DependentProbabilities(individualMocked.Object, calculationDate, dates).SurvivalProbabilities);
		
		for (int i = 0; i < NUMBEROFYEARS; i++)
		{
			survival1Probabilities[i] = (survival1Probabilities.Length - 1.0m - i) / (survival1Probabilities.Length - 1);
			survival2Probabilities[i] = survival1Probabilities[i] == 1m ? 1m : survival1Probabilities[i] * 0.9m;
			survival3Probabilities[i] = survival2Probabilities[i] == 1m ? 1m : survival2Probabilities[i] * 0.9m;
			if (i > 0)
			{
				decrementRate1[i - 1] = (survival1Probabilities[i - 1] - survival1Probabilities[i]) / survival1Probabilities[i - 1];
				decrementRate2[i - 1] = (survival2Probabilities[i - 1] - survival2Probabilities[i]) / survival2Probabilities[i - 1];
				decrementRate3[i - 1] = (survival3Probabilities[i - 1] - survival3Probabilities[i]) / survival3Probabilities[i - 1];
				decrement1Mocked.Setup(x => x.DecrementRate(individualMocked.Object, survivalDates[i - 1]))
							.Returns(decrementRate1[i - 1]);
				decrement2Mocked.Setup(x => x.DecrementRate(individualMocked.Object, survivalDates[i - 1]))
								.Returns(decrementRate2[i - 1]);
				decrement3Mocked.Setup(x => x.DecrementRate(individualMocked.Object, survivalDates[i - 1]))
								.Returns(decrementRate3[i - 1]);
			}
			if (i == NUMBEROFYEARS - 1)
			{
				decrementRate1[i] = 0m;
				decrementRate2[i] = 0m;
				decrementRate3[i] = 0m;
				decrement1Mocked.Setup(x => x.DecrementRate(individualMocked.Object, survivalDates[i]))
							.Returns(decrementRate1[i]);
				decrement2Mocked.Setup(x => x.DecrementRate(individualMocked.Object, survivalDates[i]))
								.Returns(decrementRate2[i]);
				decrement3Mocked.Setup(x => x.DecrementRate(individualMocked.Object, survivalDates[i]))
								.Returns(decrementRate3[i]);
			}
			survivalDates[i] = calculationDate.AddYears(i);
			decrement1Mocked.Setup(x => x.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i]))
						   .Returns(survival1Probabilities[i]);
			decrement2Mocked.Setup(x => x.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i]))
						   .Returns(survival2Probabilities[i]);
			decrement3Mocked.Setup(x => x.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i]))
						   .Returns(survival3Probabilities[i]);
			
			multipleDecrement.Setup(x => x.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[i]))
							.Returns(multipleDecrement.Object.IndependentProbability(individualMocked.Object, calculationDate, survivalDates[i]).SurvivalProbability);
			survivalProbabilities[i] = survival1Probabilities[i] * survival2Probabilities[i] * survival3Probabilities[i];
		}
		decrement1Mocked.Setup(x => x.SurvivalProbabilities(individualMocked.Object, calculationDate, dates))
						.Returns(survival1Probabilities);
		decrement2Mocked.Setup(x => x.SurvivalProbabilities(individualMocked.Object, calculationDate, dates))
					   .Returns(survival2Probabilities);
		decrement3Mocked.Setup(x => x.SurvivalProbabilities(individualMocked.Object, calculationDate, dates))
					   .Returns(survival3Probabilities);
		decrement1Mocked.Setup(x => x.LastPossibleDecrementDate(individualMocked.Object))
						.Returns(new DateOnly(2138,12,31));
		decrement2Mocked.Setup(x => x.LastPossibleDecrementDate(individualMocked.Object))
						.Returns(new DateOnly(2138, 12, 31));
		decrement3Mocked.Setup(x => x.LastPossibleDecrementDate(individualMocked.Object))
						.Returns(new DateOnly(2138, 12, 31));
		multipleDecrement.As<IDecrement<IIndividual>>().Setup(x => x.DecrementProbabilities(individualMocked.Object, calculationDate, dates))
						.Returns(multipleDecrement.Object.DependentProbabilities(individualMocked.Object, calculationDate, dates).SurvivalProbabilities.Select(_ => 1 - _).ToArray());
	}

	#region IDecrement Implementation
	[TestMethod]
	[TestCategory(nameof(IMultipleDecrement<IIndividual, IDecrementBetweenIntegralAgeStrategy, IDecrementBetweenIntegralAge<IIndividual, IDecrementBetweenIntegralAgeStrategy>>.SurvivalProbability))]
	public void SurvivalProbability_ProductOfIndependentDecrement_AreEquals()
	{
		// Arrange
		var disability = decrement1Mocked.Object.DecrementProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		var lapse = decrement2Mocked.Object.DecrementProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		var mortality = decrement3Mocked.Object.DecrementProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		// Act
		var expected =	decrement1Mocked.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[10]) *
						decrement2Mocked.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[10]) *
						decrement3Mocked.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		//var actual = multipleDecrement.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		var actual = decrement.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		// Assert
		Assert.AreEqual(expected, actual);
	}
	[TestMethod]
	[TestCategory(nameof(IMultipleDecrement<IIndividual, IDecrementBetweenIntegralAgeStrategy, IDecrementBetweenIntegralAge<IIndividual, IDecrementBetweenIntegralAgeStrategy>>.SurvivalProbabilities))]
	public void SurvivalProbabilities_ProductOfIndependentDecrement_AreEquals()
	{
		// Arrange

		// Act
		var expected1 = decrement1Mocked.Object.SurvivalProbabilities(individualMocked.Object, calculationDate, dates);
		var expected2 = decrement2Mocked.Object.SurvivalProbabilities(individualMocked.Object, calculationDate, dates);
		var expected3 = decrement3Mocked.Object.SurvivalProbabilities(individualMocked.Object, calculationDate, dates);
		var expectedProbabilities = new decimal[dates.Count];
		for (int i = 0; i < dates.Count; i++)
			expectedProbabilities[i] = expected1[i] * expected2[i] * expected3[i];
		var actual = decrement.SurvivalProbabilities(individualMocked.Object, calculationDate, dates);

		// Assert
		for (int i = 0; i < NUMBEROFYEARS; i++)
		{
			Assert.AreEqual(expectedProbabilities[i], actual[i], NUMBEROFYEARS * Maths.Epsilon);
		}
	}
	[TestMethod]
	[TestCategory(nameof(IMultipleDecrement<IIndividual, IDecrementBetweenIntegralAgeStrategy, IDecrementBetweenIntegralAge<IIndividual, IDecrementBetweenIntegralAgeStrategy>>.DecrementProbabilities))]
	public void DecrementProbabilities_ComplementOfProductOfIndependentDecrement_AreEquals()
	{
		// Arrange
		IMultipleDecrement<IIndividual, UniformDeathDistributionStrategy, IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>> decrement =
		new AssociateSingleDecrementUniformDeathDistribution<IIndividual, IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>>(
			decrement1Mocked.Object,
			decrement2Mocked.Object,
			decrement3Mocked.Object, null);
		// Act
		var survivalProbabilities = decrement.SurvivalProbabilities(individualMocked.Object, calculationDate, dates);

		var expectedProbabilities = new decimal[dates.Count];
		for (int i = 0; i < dates.Count; i++)
			expectedProbabilities[i] = 1 - survivalProbabilities[i];
		var actual = decrement.DecrementProbabilities(individualMocked.Object, calculationDate, dates);

		// Assert
		CollectionAssert.AreEqual(expectedProbabilities, actual);
	}
	#endregion

	#region Independent probability
	[TestMethod]
	[DataRow(1, 1, 1)]
	[DataRow(1, 1, 0)]
	[DataRow(1, 0, 1)]
	[DataRow(1, 0, 0)]
	[DataRow(0, 1, 1)]
	[DataRow(0, 1, 0)]
	[DataRow(0, 0, 1)]
	[DataRow(0, 0, 0)]
	[TestCategory(nameof(IMultipleDecrement<IIndividual, IDecrementBetweenIntegralAgeStrategy, IDecrementBetweenIntegralAge<IIndividual, IDecrementBetweenIntegralAgeStrategy>>.IndependentProbability))]
	public void IndependentProbability_IndependentDecrementProbability_AreEquals(int hasDisabilityDecrement, int hasLapseDecrement, int hasMortalityDecrement)
	{
		// Arrange
		IMultipleDecrement<IIndividual, UniformDeathDistributionStrategy, IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>> decrement =
		new AssociateSingleDecrementUniformDeathDistribution<IIndividual, IDecrementBetweenIntegralAge<IIndividual, UniformDeathDistributionStrategy>>(
			hasDisabilityDecrement == 1 ? decrement1Mocked.Object : null,
			hasLapseDecrement == 1 ? decrement2Mocked.Object : null,
			hasMortalityDecrement == 1 ? decrement3Mocked.Object : null, null);

		// Act
		decimal? expectedDisabilityProbability = hasDisabilityDecrement == 0m ? null : 1 - decrement1Mocked.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		decimal? expectedLapseProbability = hasLapseDecrement == 0m ? null : 1 - decrement2Mocked.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		decimal? expectedMortalityProbability = hasMortalityDecrement == 0m ? null : 1 - decrement3Mocked.Object.SurvivalProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		decimal expectedSurvivalProbability = (1 - expectedDisabilityProbability ?? 0m) * (1 - expectedLapseProbability ?? 0m) * (1 - expectedMortalityProbability ?? 0m);
		var actual = decrement.IndependentProbability(individualMocked.Object, calculationDate, survivalDates[10]);
		// Assert
		Assert.AreEqual(expectedSurvivalProbability, actual.SurvivalProbability);
		Assert.AreEqual(expectedDisabilityProbability, actual.DisabilityProbability);
		Assert.AreEqual(expectedLapseProbability, actual.LapseProbability);
		Assert.AreEqual(expectedMortalityProbability, actual.MortalityProbability);
	}
	#endregion

}
