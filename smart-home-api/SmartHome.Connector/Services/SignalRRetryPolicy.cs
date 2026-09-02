using Microsoft.AspNetCore.SignalR.Client;

namespace SmartHome.Connector.Services
{
    public sealed class SignalRRetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            return GetRetryDelay(retryContext.PreviousRetryCount);
        }

        public static TimeSpan GetRetryDelay(long previousRetryCount)
        {
            return previousRetryCount switch
            {
                0 => TimeSpan.Zero,
                1 => TimeSpan.FromSeconds(2),
                2 => TimeSpan.FromSeconds(5),
                _ => TimeSpan.FromSeconds(10)
            };
        }
    }
}
