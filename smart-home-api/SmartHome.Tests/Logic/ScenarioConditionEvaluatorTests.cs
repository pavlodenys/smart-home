using NUnit.Framework;
using SmartHome.Core.Enums;
using SmartHome.Logic;

namespace SmartHome.Tests.Logic
{
    [TestFixture]
    public class ScenarioConditionEvaluatorTests
    {
        [TestCase(ComparisonOperator.GreaterThan, 31, 30, true)]
        [TestCase(ComparisonOperator.GreaterThan, 30, 30, false)]
        [TestCase(ComparisonOperator.LessThan, 29, 30, true)]
        [TestCase(ComparisonOperator.LessThan, 30, 30, false)]
        [TestCase(ComparisonOperator.Equal, 30, 30, true)]
        [TestCase(ComparisonOperator.NotEqual, 29, 30, true)]
        [TestCase(ComparisonOperator.GreaterThanOrEqual, 30, 30, true)]
        [TestCase(ComparisonOperator.LessThanOrEqual, 30, 30, true)]
        public void IsMatch_EvaluatesComparison(
            ComparisonOperator comparison,
            double sensorValue,
            double threshold,
            bool expected)
        {
            Assert.That(
                ScenarioConditionEvaluator.IsMatch(comparison, sensorValue, threshold),
                Is.EqualTo(expected));
        }

        [TestCase(29, false)]
        [TestCase(30, false)]
        [TestCase(31.9, false)]
        [TestCase(32, true)]
        public void ShouldRearm_LessThan_UsesRecoveryMargin(double sensorValue, bool expected)
        {
            Assert.That(
                ScenarioConditionEvaluator.ShouldRearm(
                    ComparisonOperator.LessThan,
                    sensorValue,
                    threshold: 30,
                    hysteresis: 2),
                Is.EqualTo(expected));
        }

        [Test]
        public void ShouldRearm_NegativeHysteresis_IsTreatedAsZero()
        {
            Assert.That(
                ScenarioConditionEvaluator.ShouldRearm(
                    ComparisonOperator.LessThan,
                    sensorValue: 30,
                    threshold: 30,
                    hysteresis: -1),
                Is.True);
        }
    }
}
