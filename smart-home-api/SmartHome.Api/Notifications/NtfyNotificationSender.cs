using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace SmartHome.Api.Notifications
{
    public sealed class NtfyNotificationSender : INotificationSender
    {
        private readonly HttpClient _httpClient;
        private readonly NtfyOptions _options;

        public NtfyNotificationSender(HttpClient httpClient, IOptions<NtfyOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task SendAsync(
            string title,
            string message,
            CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                throw new InvalidOperationException("ntfy notifications are disabled.");
            }

            var baseUrl = _options.BaseUrl.TrimEnd('/');
            var topic = Uri.EscapeDataString(_options.Topic);
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/{topic}")
            {
                Content = new StringContent(message, Encoding.UTF8, "text/plain")
            };

            request.Headers.TryAddWithoutValidation("Title", title);
            request.Headers.TryAddWithoutValidation("Tags", "droplet");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
