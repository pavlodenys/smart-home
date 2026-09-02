using SmartHome.Core.Enums;

namespace SmartHome.Logic
{
    public static class ScenarioConditionEvaluator
    {
        public static bool IsMatch(ComparisonOperator comparison, double sensorValue, double threshold)
        {
            return comparison switch
            {
                ComparisonOperator.GreaterThan => sensorValue > threshold,
                ComparisonOperator.LessThan => sensorValue < threshold,
                ComparisonOperator.Equal => sensorValue == threshold,
                ComparisonOperator.NotEqual => sensorValue != threshold,
                ComparisonOperator.GreaterThanOrEqual => sensorValue >= threshold,
                ComparisonOperator.LessThanOrEqual => sensorValue <= threshold,
                _ => false
            };
        }

        public static bool ShouldRearm(
            ComparisonOperator comparison,
            double sensorValue,
            double threshold,
            double hysteresis)
        {
            var margin = Math.Max(0, hysteresis);

            return comparison switch
            {
                ComparisonOperator.LessThan => sensorValue >= threshold + margin,
                ComparisonOperator.LessThanOrEqual => sensorValue > threshold + margin,
                ComparisonOperator.GreaterThan => sensorValue <= threshold - margin,
                ComparisonOperator.GreaterThanOrEqual => sensorValue < threshold - margin,
                _ => !IsMatch(comparison, sensorValue, threshold)
            };
        }
    }
}
