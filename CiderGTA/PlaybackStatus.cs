using System.Text.Json.Serialization;

namespace CiderGTA
{
    public class PlaybackStatus
    {
        [JsonPropertyName("type")] public string Type { get; set; }

        [JsonPropertyName("data")] public object Data { get; set; }
    }
}