using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LovEat.API.Services
{
    /// <summary>
    /// P1 hardening: real live streaming via Mux (mux.com) — a REST API
    /// purpose-built for exactly this ("create a live stream, get an RTMP
    /// ingest URL for the broadcaster and an HLS playback URL for viewers"),
    /// replacing the previous stub:// fake URL in LiveStreamService.StartAsync.
    /// </summary>
    public interface IStreamingProviderService
    {
        Task<(bool Success, string? IngestUrl, string? StreamKey, string? PlaybackUrl, string? ErrorMessage)> CreateLiveStreamAsync();
        Task<bool> EndLiveStreamAsync(string providerStreamId);
        bool IsConfigured { get; }
    }

    public class MuxStreamingService : IStreamingProviderService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<MuxStreamingService> _logger;

        public MuxStreamingService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<MuxStreamingService> logger)
        {
            _http = httpClientFactory.CreateClient();
            _config = config;
            _logger = logger;
        }

        private string? TokenId => _config["ExternalServices:Streaming:MuxTokenId"];
        private string? TokenSecret => _config["ExternalServices:Streaming:MuxTokenSecret"];

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(TokenId) && TokenId != "REPLACE_VIA_ENV_VAR" &&
            !string.IsNullOrWhiteSpace(TokenSecret) && TokenSecret != "REPLACE_VIA_ENV_VAR";

        // Creates a Mux live stream: returns the RTMP ingest URL + stream key
        // the chef's broadcasting app pushes video to, and the HLS playback
        // URL viewers watch. Mux auto-generates a playback asset once the
        // broadcast ends, so no separate "save the recording" step is needed.
        public async Task<(bool Success, string? IngestUrl, string? StreamKey, string? PlaybackUrl, string? ErrorMessage)> CreateLiveStreamAsync()
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("Live stream creation skipped — ExternalServices:Streaming:MuxTokenId/MuxTokenSecret not configured.");
                return (false, null, null, null, "Live streaming is not configured on this server.");
            }

            var authBytes = Encoding.UTF8.GetBytes($"{TokenId}:{TokenSecret}");
            var payload = JsonSerializer.Serialize(new
            {
                playback_policy = new[] { "public" },
                new_asset_settings = new { playback_policy = new[] { "public" } },
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mux.com/video/v1/live-streams")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

            try
            {
                var response = await _http.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Mux live stream creation failed ({StatusCode}): {Body}", response.StatusCode, body);
                    return (false, null, null, null, "Could not create the live stream. Please try again.");
                }

                using var doc = JsonDocument.Parse(body);
                var data = doc.RootElement.GetProperty("data");
                var streamKey = data.GetProperty("stream_key").GetString();
                var playbackId = data.GetProperty("playback_ids")[0].GetProperty("id").GetString();

                var ingestUrl = "rtmp://global-live.mux.com/app";
                var playbackUrl = $"https://stream.mux.com/{playbackId}.m3u8";
                return (true, ingestUrl, streamKey, playbackUrl, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mux live stream creation threw an exception.");
                return (false, null, null, null, "Could not reach the streaming provider. Please try again.");
            }
        }

        public async Task<bool> EndLiveStreamAsync(string providerStreamId)
        {
            if (!IsConfigured) return false;
            var authBytes = Encoding.UTF8.GetBytes($"{TokenId}:{TokenSecret}");
            using var request = new HttpRequestMessage(HttpMethod.Put, $"https://api.mux.com/video/v1/live-streams/{providerStreamId}/complete");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
            try
            {
                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mux live stream completion threw an exception for {StreamId}", providerStreamId);
                return false;
            }
        }
    }
}
