using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CiderGTA
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ValuesForFactory
    {
        [JsonPropertyName("type")] public string Type { get; set; }
    }

    public static class PlaybackStatusFactory
    {
        public static PlaybackStatus Create(string jsonString)
        {
            var status = JsonSerializer.Deserialize<List<ValuesForFactory>>(jsonString);
            var type = status[0].Type;
            object data = null;

            switch (type)
            {
                case "playbackStatus.nowPlayingItemDidChange":
                    data = JsonSerializer.Deserialize<List<NowPlayingItemDidChange>>(jsonString)[0];
                    break;
                case "playbackStatus.playbackStateDidChange":
                    data = JsonSerializer.Deserialize<List<PlaybackStateDidChange>>(jsonString)[0];
                    break;
            }

            return new PlaybackStatus
            {
                Type = type,
                Data = data
            };
        }
    }
}