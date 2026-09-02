using NUnit.Framework;
using SmartHome.Connector.Services;

namespace SmartHome.Tests.Logic
{
    public class SignalRRetryPolicyTests
    {
        [TestCase(0, 0)]
        [TestCase(1, 2)]
        [TestCase(2, 5)]
        [TestCase(3, 10)]
        [TestCase(100, 10)]
        public void GetRetryDelay_keeps_retrying_with_a_bounded_delay(
            long previousRetryCount,
            int expectedSeconds)
        {
            var delay = SignalRRetryPolicy.GetRetryDelay(previousRetryCount);

            Assert.That(delay, Is.EqualTo(TimeSpan.FromSeconds(expectedSeconds)));
        }
    }
}
