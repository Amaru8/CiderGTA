using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace CiderGTA
{
    internal class CiderController
    {
        public readonly HttpClient HttpClient = new HttpClient();

        private static readonly SocketIOClient.SocketIO SocketIoClient =
            new SocketIOClient.SocketIO("http://localhost:10767/");

        public readonly bool ObtainedCiderClient;

        public string CurrentTitle;

        public string CurrentArtist;

        // public string CurrentAlbum;
        public string CurrentRadio;

        public CiderController()
        {
            ObtainedCiderClient = true;
            var gtaTextInfo = new CultureInfo("en-US", false).TextInfo;

            SocketIoClient.On("API:Playback", response =>
            {
                var playbackStatus = PlaybackStatusFactory.Create(response.ToString());

                switch (playbackStatus.Data)
                {
                    case NowPlayingItemDidChange nowPlayingData:
                    {
                        if (nowPlayingData.Data.PlayParams?.Kind == "radioStation")
                        {
                            CurrentRadio = nowPlayingData.Data.EditorialNotes.Name;
                            CurrentTitle = "LIVE - Controlled by Cider";
                            CurrentArtist = nowPlayingData.Data.EditorialNotes.Short;
                            // CurrentAlbum = "Radio Station";
                        }
                        else
                        {
                            CurrentRadio = null;
                            CurrentTitle = gtaTextInfo.ToTitleCase(nowPlayingData.Data.Name);
                            CurrentArtist = gtaTextInfo.ToUpper(nowPlayingData.Data.ArtistName);
                            // CurrentAlbum = nowPlayingData.Data.AlbumName;
                        }

                        break;
                    }
                    case PlaybackStateDidChange playbackStateData:
                        if (playbackStateData.Data.Attributes.PlayParams?.Kind == "radioStation")
                        {
                            CurrentRadio = playbackStateData.Data.Attributes.EditorialNotes.Name;
                            CurrentTitle = "LIVE - Controlled by Cider";
                            CurrentArtist = playbackStateData.Data.Attributes.EditorialNotes.Short;
                            // CurrentAlbum = "Radio Station";
                        }
                        else
                        {
                            CurrentRadio = null;
                            CurrentTitle = gtaTextInfo.ToTitleCase(playbackStateData.Data.Attributes.Name);
                            CurrentArtist = gtaTextInfo.ToUpper(playbackStateData.Data.Attributes.ArtistName);
                            // CurrentAlbum = playbackStateData.Data.Attributes.AlbumName;
                        }

                        break;
                }
            });

            SocketIoClient.OnConnected += (sender, e) => { Logger.Log("Connected to Cider"); };
            SocketIoClient.ConnectAsync();
        }

        public void InitialRequests()
        {
            Logger.Log("Making initial requests to Cider.");
            try
            {
                SetVolume(0);
                ResumePlayback();
            }
            catch (Exception ex)
            {
                Logger.Log("An error occured while running initial requests: " + ex.Message);
            }
        }

        public void SkipNext() => RunGenericPostEndpoint("Next", "api/v1/playback/next");

        public void SkipPrevious() => RunGenericPostEndpoint("Previous", "api/v1/playback/previous");

        public void PlayPausePlayback() => RunGenericPostEndpoint("PlayPause", "api/v1/playback/playpause");

        public void PausePlayback() => RunGenericPostEndpoint("Pause", "api/v1/playback/pause");

        public void ResumePlayback() => RunGenericPostEndpoint("Resume", "api/v1/playback/play");

        public void SetVolume(int volumeRequest)
        {
            var dictionary = new Dictionary<string, double>
            {
                { "volume", Math.Round(volumeRequest / 100.0, 2) }
            };

            var byteContent =
                new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dictionary)));
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            RunGenericPostEndpoint("Set Volume", "api/v1/playback/volume", byteContent);
        }

        public void ToggleShuffle() => RunGenericPostEndpoint("Toggle Shuffle", "api/v1/playback/toggle-shuffle");

        private async void RunGenericPostEndpoint(string name, string endpoint, ByteArrayContent content = null)
        {
            try
            {
                var response =
                    await HttpClient.PostAsync("http://127.0.0.1:10767/" + endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.Log(name + " endpoint failed: " + response.StatusCode);
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException != null)
                {
                    Logger.Log(name + " endpoint failed: " + ex.InnerException.Message);
                }
                else
                {
                    Logger.Log(name + " endpoint failed: " + ex.Message);
                }
            }
        }

        public async Task<bool> AttemptReconnection()
        {
            try
            {
                SocketIoClient.Dispose();
                await SocketIoClient.ConnectAsync();
                return true;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException != null)
                {
                    Logger.Log("Reconnection attempt failed: " + ex.InnerException.Message);
                }
                else
                {
                    Logger.Log("Reconnection attempt failed: " + ex.Message);
                }

                return false;
            }
        }


        public Dictionary<string, string> GetCurrentTrack()
        {
            if (CurrentTitle != null)
            {
                return new Dictionary<string, string>
                {
                    { "title", CurrentTitle },
                    { "artist", CurrentArtist },
                    // { "album", CurrentAlbum },
                    { "radio", CurrentRadio }
                };
            }

            return null;
        }
    }
}