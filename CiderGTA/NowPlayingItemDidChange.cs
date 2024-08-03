using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace CiderGTA
{
    public class NowPlayingItemDidChange
    {
        [JsonPropertyName("type")] public string Type { get; set; }

        [JsonPropertyName("data")] public PlaybackData Data { get; set; }

        public class PlaybackData
        {
            [JsonPropertyName("albumName")] public string AlbumName { get; set; }

            [JsonPropertyName("durationInMillis")] public int DurationInMillis { get; set; }

            [JsonPropertyName("name")] public string Name { get; set; }

            [JsonPropertyName("artistName")] public string ArtistName { get; set; }

            [JsonPropertyName("editorialNotes")] public EditorialNotes EditorialNotes { get; set; }

            [JsonPropertyName("playParams")] public PlayParams PlayParams { get; set; }
        }

        public class EditorialNotes
        {
            [JsonPropertyName("name")] public string Name { get; set; }

            [JsonPropertyName("tagline")] public string Tagline { get; set; }

            [JsonPropertyName("short")] public string Short { get; set; }
        }

        public class PlayParams
        {
            [JsonPropertyName("kind")] public string Kind { get; set; }
        }
    }
}