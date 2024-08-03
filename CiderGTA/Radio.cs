using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.UI;
using LemonUI;
using LemonUI.Menus;

namespace CiderGTA
{
    public static class Logger
    {
        public static void Log(object message)
        {
            try
            {
                File.AppendAllText("scripts/CiderGTA.log", DateTime.Now + " : " + message + Environment.NewLine);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    // ReSharper disable once UnusedType.Global
    public class CiderRadio : Script
    {
        private bool _isEngineOn;
        private bool _isCiderRadio;
        private int _volume;
        private readonly CiderController _cider;

        private NativeMenu _mainMenu;
        private ObjectPool _modObjectPool;
        private NativeItem _playPausePlayback;
        private NativeItem _prevTrack;
        private NativeItem _skipTrack;
        private NativeItem _volumeLevel;
        private NativeItem _shuffleButton;
        private NativeItem _displayTrackName;
        private NativeItem _attemptReconnection;

        private readonly Scaleform _dashboardScaleform;

        private const string RadioName = "RADIO_47_SPOTIFY_RADIO";

        private readonly Keys _menuKey;

        public CiderRadio()
        {
            _cider = new CiderController();
            _isEngineOn = false;
            _isCiderRadio = false;
            _isEngineOn = false;

            File.Create("scripts/CiderGTA.log").Close();
            var config = ScriptSettings.Load("scripts/CiderGTA.ini");
            _menuKey = config.GetValue("Options", "MenuKey", Keys.F10);
            _volume = config.GetValue("Options", "InitialVolume", 90);
            _cider.HttpClient.DefaultRequestHeaders.Add("apptoken", config.GetValue("Options", "Token", "PLACEHOLDER"));
            if (_volume < 0 || _volume > 100)
            {
                _volume = 100;
            }

            if (_cider.ObtainedCiderClient)
            {
                _cider.InitialRequests();
                Function.Call(Hash.SET_RADIO_STATION_MUSIC_ONLY, RadioName, true);
                SetupMenu();
                _dashboardScaleform = new Scaleform("dashboard");
                KeyDown += OnKeyDown;
                Tick += OnTick;
            }
            else
            {
                Logger.Log("ERROR: Did not found running Cider Client");
            }
        }

        private void SetupMenu()
        {
            _modObjectPool = new ObjectPool();
            _mainMenu = new NativeMenu("Cider Radio", "Waiting for Playback...");

            _playPausePlayback = new NativeItem("Play/Pause Track");
            _prevTrack = new NativeItem("Previous Track");
            _skipTrack = new NativeItem("Skip Track");
            _volumeLevel = new NativeItem("Set Volume");
            _shuffleButton = new NativeItem("Toggle Shuffle");
            _displayTrackName = new NativeItem("Get Current Track");
            _attemptReconnection = new NativeItem("Attempt to Reconnect");

            _mainMenu.Add(_playPausePlayback);
            _mainMenu.Add(_prevTrack);
            _mainMenu.Add(_skipTrack);
            _mainMenu.Add(_volumeLevel);
            _mainMenu.Add(_shuffleButton);
            _mainMenu.Add(_displayTrackName);
            _mainMenu.Add(_attemptReconnection);

            _mainMenu.ItemActivated += (sender, item) => OnMainMenuItemSelect(item);

            _modObjectPool.Add(_mainMenu);
        }

        private void OnMainMenuItemSelect(ItemActivatedArgs sender)
        {
            var item = sender.Item;
            if (item == _playPausePlayback)
            {
                _cider.PlayPausePlayback();
            }
            else if (item == _skipTrack)
            {
                _cider.SkipNext();
            }
            else if (item == _prevTrack)
            {
                _cider.SkipPrevious();
            }
            else if (item == _volumeLevel)
            {
                try
                {
                    var vol = int.Parse(Game.GetUserInput(WindowTitle.CustomTeamName, _volume.ToString(), 3));
                    if (vol < 0 || vol > 100)
                    {
                        throw new FormatException("Number out of range.");
                    }

                    _volume = vol;
                    _cider.SetVolume(_volume);
                    if (_isCiderRadio)
                    {
                        Unmute();
                    }
                }
                catch (FormatException x)
                {
                    Notification.Show("Volume must be number between 0 and 100.");
                    Logger.Log("Invalid input: " + x.Message);
                }
            }
            else if (item == _shuffleButton)
            {
                _cider.ToggleShuffle();
            }
            else if (item == _displayTrackName)
            {
                var currentlyPlaying = _cider.GetCurrentTrack();
                if (currentlyPlaying != null)
                {
                    Notification.Show(currentlyPlaying["title"] + " by " + currentlyPlaying["artist"]);
                }
            }
            else if (item == _attemptReconnection)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _cider.AttemptReconnection();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == _menuKey)
            {
                _mainMenu.Visible = !_mainMenu.Visible;
            }
        }

        private string GetCurrentStationName() => Function.Call<string>(Hash.GET_PLAYER_RADIO_STATION_NAME);

        private void SetEngine(bool status)
        {
            if (_cider.ObtainedCiderClient)
            {
                _isEngineOn = status;
            }
        }

        private void Mute() => _cider.SetVolume(0);

        private void Unmute() => _cider.SetVolume(_volume);

        // Only play if Cider Radio is selected
        private void SetRadioStation(bool status)
        {
            if (!_cider.ObtainedCiderClient) return;
            _isCiderRadio = status;

            if (status)
            {
                Unmute();
            }
            else
            {
                Mute();
            }
        }

        private void UpdateInGameRadio(Dictionary<string, string> currentlyPlaying)
        {
            try
            {
                SetMenuName(currentlyPlaying);
                if (currentlyPlaying["radio"] == null)
                {
                    currentlyPlaying["radio"] = "Cider Radio";
                }

                // Call function
                _dashboardScaleform.CallFunction("SET_RADIO",
                    "", currentlyPlaying["radio"],
                    currentlyPlaying["artist"], currentlyPlaying["title"]);
            }
            catch (Exception exception)
            {
                Logger.Log("Failed to display track name on Radio dashboard: " + exception);
            }
        }

        private void SetMenuName(Dictionary<string, string> currentlyPlaying)
        {
            const int textLimit = 23;

            if (currentlyPlaying["radio"] != null)
            {
                // Radio is playing
                _mainMenu.Name = currentlyPlaying["radio"];
                return;
            }

            if ((currentlyPlaying["title"] + " by " + currentlyPlaying["artist"]).Length < textLimit)
            {
                // Title and artist fits
                _mainMenu.Name = currentlyPlaying["title"] + " by " + currentlyPlaying["artist"];
                return;
            }

            if (currentlyPlaying["title"].Length < textLimit)
            {
                // Title fits
                _mainMenu.Name = currentlyPlaying["title"];
                return;
            }

            // Title doesn't fit, so we add ...
            _mainMenu.Name = currentlyPlaying["title"].Substring(0, textLimit - 3) + "...";
        }

        private bool GetEngineStatus() => _isEngineOn;

        private void OnTick(object sender, EventArgs e)
        {
            _modObjectPool.Process();
            if (Game.WasCheatStringJustEntered("cider"))
            {
                _mainMenu.Visible = true;
            }

            // Handle Radio Channel
            if (Game.Player.Character.CurrentVehicle != null && Game.Player.Character.CurrentVehicle.IsEngineRunning &&
                !GetEngineStatus() && GetCurrentStationName().Equals(RadioName) && !_isCiderRadio)
            {
                SetEngine(true);
                SetRadioStation(true);
            }
            else if (Game.Player.Character.CurrentVehicle == null && GetEngineStatus() &&
                     !GetCurrentStationName().Equals(RadioName) && _isCiderRadio)
            {
                SetEngine(false);
                SetRadioStation(false);
            }
            else if (Game.Player.Character.CurrentVehicle == null && GetEngineStatus())
            {
                SetEngine(false);
                SetRadioStation(false);
            }
            else if (Game.Player.Character.CurrentVehicle != null &&
                     Game.Player.Character.CurrentVehicle.IsEngineRunning && !GetEngineStatus())
            {
                SetEngine(true);
            }
            else if (Game.Player.Character.CurrentVehicle != null &&
                     !Game.Player.Character.CurrentVehicle.IsEngineRunning && GetEngineStatus() && _isCiderRadio)
            {
                SetEngine(false);
                SetRadioStation(false);
            }
            else if (Game.Player.Character.CurrentVehicle != null &&
                     !Game.Player.Character.CurrentVehicle.IsEngineRunning && GetEngineStatus())
            {
                SetEngine(false);
            }
            else if (GetEngineStatus() && GetCurrentStationName().Equals(RadioName) && !_isCiderRadio)
            {
                SetRadioStation(true);
            }
            else if (GetEngineStatus() && !GetCurrentStationName().Equals(RadioName) && _isCiderRadio)
            {
                SetRadioStation(false);
            }

            if (!_isCiderRadio) return;
            var currentlyPlaying = _cider.GetCurrentTrack();
            if (currentlyPlaying != null)
            {
                UpdateInGameRadio(currentlyPlaying);
            }
        }
    }
}