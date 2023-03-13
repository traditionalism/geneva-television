using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace geneva_television.Client
{
    public class ClientMain : BaseScript
    {
        private readonly List<int> _televisionModels = new()
        {
            GetHashKey("prop_tv_flat_02"),
            GetHashKey("prop_tv_03"),
            GetHashKey("prop_tv_flat_01"),
            GetHashKey("des_tvsmash_start")
        };
        
        private readonly Dictionary<int, string> _channels = new()
        {
            { 1, "PL_STD_CNT" },
            { 2, "PL_STD_WZL" },
            { 3, "PL_LO_CNT" },
            { 4, "PL_LO_WZL" },
            { 6, "PL_SP_INV" },
            { 7, "PL_SP_INV_EXP" },
            { 8, "PL_LO_RS" },
            { 9, "PL_LO_RS_CUTSCENE" },
            { 10, "PL_SP_PLSH1_INTRO" },
            { 11, "PL_LES1_FAME_OR_SHAME" },
            { 12, "PL_STD_WZL_FOS_EP2" },
            { 13, "PL_MP_WEAZEL" },
            { 14, "PL_MP_CCTV" },
            { 15, "PL_CINEMA_ACTION" },
            { 16, "PL_CINEMA_ARTHOUSE" },
            { 17, "PL_CINEMA_MULTIPLAYER" },
            { 18, "PL_WEB_HOWITZER" },
            { 19, "PL_WEB_RANGERS" }
        };
        
        private Prop _closestTv;
        private bool _tvOn;
        private bool _focusingTv;
        private int _renderTargetId;
        private int _cam;
        private int _currentChannel = 1;
        private const float CurrentVolume = -4f;
        private bool _isChangingVolume;
        private const float VolumeChangeSpeed = 0.5f;
        private const float VolumeChangeDelay = 0.2f;
        private float _volumeChangeTimer;

        private async Task DrawTvTick()
        {
            SetTextRenderId(_renderTargetId);
            SetScriptGfxDrawOrder(4);
            SetScriptGfxDrawBehindPausemenu(true);
            DrawTvChannel(0.5f, 0.5f, 1f, 1f, 0f, 255, 255, 255, 255);
            SetTextRenderId(1);

            await Task.FromResult(0);
        }

        private async Task HandleTvOnStuffTick()
        {
            Screen.DisplayHelpTextThisFrame("Use ~INPUT_MOVE_LEFT_ONLY~ ~INPUT_MOVE_RIGHT_ONLY~ to change the channel.~n~Use ~INPUT_MOVE_UP_ONLY~ ~INPUT_MOVE_DOWN_ONLY~ to change the volume.~n~Press ~INPUT_ENTER~ to turn off the TV.~n~Press ~INPUT_CONTEXT~ to stop watching.");
            
            HideHudAndRadarThisFrame();
            
            if (Game.IsControlJustReleased(0, Control.Context))
            {
                SetCamActive(_cam, false);
                RenderScriptCams(false, false, 3000, true, false);
                SetPlayerControl(Game.Player.Handle, true, 0);
                Game.PlayerPed.IsPositionFrozen = false;
                Game.PlayerPed.IsVisible = true;
                Game.PlayerPed.IsInvincible = false;
                _focusingTv = false;
                Tick -= HandleTvOnStuffTick;
            }

            if (Game.IsControlJustReleased(0, Control.Enter))
            {
                int soundId = GetSoundId();
                PlaySoundFrontend(soundId, "MICHAEL_SOFA_TV_ON_MASTER", null, false);
                ReleaseSoundId(soundId);
                
                Game.PlayerPed.IsPositionFrozen = false;
                Game.PlayerPed.IsVisible = true;
                Game.PlayerPed.IsInvincible = false;
                SetCamActive(_cam, false);
                RenderScriptCams(false, false, 3000, true, false);
                SetPlayerControl(Game.Player.Handle, true, 0);
                
                SetTvChannel(-1);
                EnableMovieSubtitles(false);
                ReleaseAmbientAudioBank();
                _tvOn = false;
                _focusingTv = false;
                Tick -= DrawTvTick;
                Tick -= HandleTvOnStuffTick;
                Tick -= TrackInteriorTick;
            }

            if (Game.IsControlPressed(0, Control.MoveUpOnly))
            {
                if (!_isChangingVolume)
                {
                    _isChangingVolume = true;
                    
                    int soundId = GetSoundId();
                    PlaySoundFrontend(soundId, "MICHAEL_SOFA_TV_CHANGE_CHANNEL_MASTER", null, false);
                    ReleaseSoundId(soundId);
                    
                    float currentVolume = GetTvVolume();
                    float newVolume = currentVolume + VolumeChangeSpeed;
                    if (newVolume > 0f) newVolume = 0f;
                    SetTvVolume(newVolume);
                }
                else
                {
                    _volumeChangeTimer += Game.LastFrameTime;
                    if (_volumeChangeTimer >= VolumeChangeDelay)
                    {
                        int soundId = GetSoundId();
                        PlaySoundFrontend(soundId, "MICHAEL_SOFA_TV_CHANGE_CHANNEL_MASTER", null, false);
                        ReleaseSoundId(soundId);
                        
                        float currentVolume = GetTvVolume();
                        float newVolume = currentVolume + VolumeChangeSpeed;
                        if (newVolume > 0f) newVolume = 0f; 
                        SetTvVolume(newVolume);
                        _volumeChangeTimer = 0f;
                    }
                }
            }

            if (Game.IsControlPressed(0, Control.MoveDownOnly))
            {
                if (!_isChangingVolume)
                {
                    _isChangingVolume = true;
                    
                    int soundId = GetSoundId();
                    PlaySoundFrontend(soundId, "MICHAEL_SOFA_TV_CHANGE_CHANNEL_MASTER", null, false);
                    ReleaseSoundId(soundId);
                    
                    float currentVolume = GetTvVolume();
                    float newVolume = currentVolume - VolumeChangeSpeed;
                    if (newVolume < -36f) newVolume = -36f;
                    SetTvVolume(newVolume);
                }
                else
                {
                    _volumeChangeTimer += Game.LastFrameTime;
                    if (_volumeChangeTimer >= VolumeChangeDelay)
                    {
                        int soundId = GetSoundId();
                        PlaySoundFrontend(soundId, "MICHAEL_SOFA_TV_CHANGE_CHANNEL_MASTER", null, false);
                        ReleaseSoundId(soundId);
                        
                        float currentVolume = GetTvVolume();
                        float newVolume = currentVolume - VolumeChangeSpeed;
                        if (newVolume < -36f) newVolume = -36f;
                        SetTvVolume(newVolume);
                        _volumeChangeTimer = 0f;
                    }
                }
            }

            if (!Game.IsControlPressed(0, Control.MoveUpOnly) && !Game.IsControlPressed(0, Control.MoveDownOnly))
            {
                _isChangingVolume = false;
                _volumeChangeTimer = 0f;
            }

            if (Game.IsControlJustReleased(0, Control.MoveLeftOnly))
            {
                _currentChannel -= 1;

                if (_currentChannel < 1)
                {
                    _currentChannel = _channels.Count;
                }
                else if (_currentChannel > _channels.Count)
                {
                    _currentChannel = 1;
                }
                else if (_currentChannel == 5)
                {
                    _currentChannel = 4;
                }
                
                int soundId = GetSoundId();
                PlaySoundFrontend(soundId, "MICHAEL_SOFA_TV_CHANGE_CHANNEL_MASTER", null, false);
                ReleaseSoundId(soundId);

                SetTvChannelPlaylist(_currentChannel, _channels[_currentChannel], false);
                SetTvChannel(_currentChannel);
            }
            
            if (Game.IsControlJustReleased(0, Control.MoveRightOnly))
            {
                _currentChannel += 1;
                
                if (_currentChannel < 1)
                {
                    _currentChannel = _channels.Count;
                }
                else if (_currentChannel > _channels.Count)
                {
                    _currentChannel = 1;
                }
                else if (_currentChannel == 5)
                {
                    _currentChannel = 6;
                }
                
                int soundId = GetSoundId();
                PlaySoundFrontend(soundId, "MICHAEL_SOFA_TV_CHANGE_CHANNEL_MASTER", null, false);
                ReleaseSoundId(soundId);

                SetTvChannelPlaylist(_currentChannel, _channels[_currentChannel], false);
                SetTvChannel(_currentChannel);
            }

            await Task.FromResult(0);
        }

        private async Task TrackInteriorTick()
        {
            if (GetInteriorFromEntity(Game.PlayerPed.Handle) != GetInteriorFromEntity(_closestTv.Handle))
            {
                SetTvChannel(-1);
                EnableMovieSubtitles(false);
                ReleaseAmbientAudioBank();
                _tvOn = false;
                Tick -= DrawTvTick;
                Tick -= HandleTvOnStuffTick;
                Tick -= TrackInteriorTick;
            }

            await Delay(2000);
        }

        [Command("reset")]
        private void Reset()
        {
            SetPlayerControl(Game.Player.Handle, true, 0);
            Game.PlayerPed.IsPositionFrozen = false;
            Game.PlayerPed.IsVisible = true;
            Game.PlayerPed.IsInvincible = false;
        }

        private async Task WatchTv()
        {
            if (_tvOn && !_focusingTv)
            {
                SetCamActive(_cam, true);
                RenderScriptCams(true, false, 3000, true, false);
                Game.PlayerPed.Task.LookAt(_closestTv, -1);
                Game.PlayerPed.IsPositionFrozen = true;
                Game.PlayerPed.IsVisible = false;
                Game.PlayerPed.IsInvincible = true;
                Game.PlayerPed.Task.ClearAll();
                SetPlayerControl(Game.Player.Handle, false, 0);
                
                _focusingTv = true;
                Tick += HandleTvOnStuffTick;
                Tick += TrackInteriorTick;
            }

            if (!_tvOn)
            {
                while (!RequestAmbientAudioBank("SAFEHOUSE_MICHAEL_SIT_SOFA", false)) await Delay(0);

                RegisterNamedRendertarget("tvscreen", false);
                LinkNamedRendertarget(_closestTv.Model);
                _renderTargetId = GetNamedRendertargetRenderId("tvscreen");
                
                SetTvChannel(-1);
                SetTvChannelPlaylist(_currentChannel, _channels[_currentChannel], false);
                SetTvVolume(CurrentVolume);
                SetTvChannel(_currentChannel);
                EnableMovieSubtitles(true);
                SetTvAudioFrontend(false);
                AttachTvAudioToEntity(_closestTv.Handle);
                
                Tick += DrawTvTick;
                
                int soundId = GetSoundId();
                PlaySoundFrontend(soundId, "MICHAEL_SOFA_TV_ON_MASTER", null, false);
                ReleaseSoundId(soundId);
                
                _tvOn = true;
                _focusingTv = true;

                _cam = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", 2.5724f, 527.9989f, 176.1619f, 0f, 0f, -29.9488f, 50f, false, 2);
                SetCamFarClip(_cam, 100f);
                SetCamActive(_cam, true);
                RenderScriptCams(true, false, 3000, true, false);
                Game.PlayerPed.IsPositionFrozen = true;
                Game.PlayerPed.IsVisible = false;
                Game.PlayerPed.IsInvincible = true;
                Game.PlayerPed.Task.ClearAll();
                SetPlayerControl(Game.Player.Handle, false, 0);
                Game.PlayerPed.Task.LookAt(_closestTv, -1);
                
                Tick += HandleTvOnStuffTick;
                Tick += TrackInteriorTick;
            }
        }

        [Tick]
        private async Task FindTvTick()
        {
            Vector3 plyPos = Game.PlayerPed.Position;
            Prop prop = World.GetAllProps()
                .Where(p => _televisionModels.Contains(p.Model))
                .OrderBy(p => Vector3.DistanceSquared(p.Position, plyPos))
                .FirstOrDefault();
            
            if (prop is null || HasObjectBeenBroken(prop.Handle) || GetRoomKeyFromEntity(Game.PlayerPed.Handle) != GetRoomKeyFromEntity(prop.Handle))
            {
                await Delay(3500);
                return;
            }

            if (!_focusingTv)
            {
                Screen.DisplayHelpTextThisFrame(_tvOn ? "Press ~INPUT_CONTEXT~ to watch TV." : "Press ~INPUT_CONTEXT~ to turn on the TV.");
                
                if (Game.IsControlJustReleased(0, Control.Context))
                {
                    _closestTv = prop;
                    await WatchTv();
                }
            }
        }
    }
}