namespace SmartHome.Api.Notifications
{
    public sealed class NtfyOptions
    {
        public const string SectionName = "Notifications:Ntfy";

        public bool Enabled { get; set; }
        public string BaseUrl { get; set; } = "https://ntfy.sh";
        public string Topic { get; set; } = string.Empty;

        public static bool IsValid(NtfyOptions options)
        {
            if (!options.Enabled)
            {
                return true;
            }

            return Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
                && baseUri.Scheme == Uri.UriSchemeHttps
                && !string.IsNullOrWhiteSpace(options.Topic)
                && options.Topic.IndexOfAny(new[] { '/', '?', '#' }) < 0;
        }
    }
}
