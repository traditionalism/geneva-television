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
        private readonly Dictionary<int, int> _televisionRooms = new()
        {
            { 1411597561, GetHashKey("prop_tv_flat_01") },
            { 1961042103, GetHashKey("des_tvsmash_start") },
            { 1541752721, GetHashKey("prop_tv_flat_02") },
            { -1073987335, GetHashKey("prop_tv_03") },
            { -1913322317, GetHashKey("prop_trev_tv_01") }
        };

        private readonly Dictionary<int, Vector3> _televisionPositions = new()
        {
            { 1411597561, new Vector3(2.5724f, 527.9989f, 176.1619f) },
            { 1961042103, new Vector3(-808.3051f, 171.2623f, 77.2822f) },
            { 1541752721, new Vector3(-1160.5024f, -1520.7598f, 10.7393f) },
            { -1073987335, new Vector3(-9.8135f, -1440.9128f, 31.3654f) },
            { -1913322317, new Vector3(1978.2303f, 3819.6504f, 34.2724f) }
        };

        private readonly Dictionary<int, Vector3> _televisionRotations = new()
        {
            { 1411597561, new Vector3(0f, 0f, -29.9488f) },
            { 1961042103, new Vector3(1.8886f, 0f, 110.9232f) },
            { 1541752721, new Vector3(0f, 0f, 60.061f) },
            { -1073987335, new Vector3(0f, 0f, -134.3211f) },
            { -1913322317, new Vector3(0f, 0f, -105.15f) }
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
        
        private Entity _closestTv;
        private bool _tvOn;
        private bool _focusingTv;
        private int _renderTargetId;
        private int _cam;
        private int _currentChannel = 1;
        private const float CurrentVolume = 0f;
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
                DestroyAllCams(false);
                
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

        private async Task WatchTv()
        {
            if (_tvOn && !_focusingTv)
            {
                Vector3 televisionPosition = _televisionPositions[GetRoomKeyFromEntity(_closestTv.Handle)];
                Vector3 televisionRotation = _televisionRotations[GetRoomKeyFromEntity(_closestTv.Handle)];
                _cam = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", televisionPosition.X, televisionPosition.Y, televisionPosition.Z, televisionRotation.X, televisionRotation.Y, televisionRotation.Z, 50f, false, 2);
                SetCamFarClip(_cam, 100f);
                SetCamActive(_cam, true);
                RenderScriptCams(true, false, 3000, true, false);
                Game.PlayerPed.IsPositionFrozen = true;
                Game.PlayerPed.Task.ClearAll();
                SetPlayerControl(Game.Player.Handle, false, 0);
                Game.PlayerPed.Task.LookAt(_closestTv, -1);
                Game.PlayerPed.IsVisible = false;
                Game.PlayerPed.IsInvincible = true;
                
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
                
                Vector3 televisionPosition = _televisionPositions[GetRoomKeyFromEntity(_closestTv.Handle)];
                Vector3 televisionRotation = _televisionRotations[GetRoomKeyFromEntity(_closestTv.Handle)];
                _cam = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", televisionPosition.X, televisionPosition.Y, televisionPosition.Z, televisionRotation.X, televisionRotation.Y, televisionRotation.Z, 50f, false, 2);
                SetCamFarClip(_cam, 100f);
                SetCamActive(_cam, true);
                RenderScriptCams(true, false, 3000, true, false);
                Game.PlayerPed.IsPositionFrozen = true;
                Game.PlayerPed.Task.ClearAll();
                SetPlayerControl(Game.Player.Handle, false, 0);
                Game.PlayerPed.Task.LookAt(_closestTv, -1);
                Game.PlayerPed.IsVisible = false;
                Game.PlayerPed.IsInvincible = true;
                
                Tick += HandleTvOnStuffTick;
                Tick += TrackInteriorTick;
            }
        }

        [Tick]
        private async Task FindTvTick()
        {
            Vector3 plyPos = Game.PlayerPed.Position;
            Entity entity = World.GetAllProps()
                .Where(i => _televisionRooms.ContainsKey(GetRoomKeyFromEntity(i.Handle)) && i.Model.Hash == _televisionRooms[GetRoomKeyFromEntity(i.Handle)])
                .OrderBy(i => Vector3.DistanceSquared(i.Position, plyPos))
                .FirstOrDefault();

            if (entity is null || HasObjectBeenBroken(entity.Handle))
            {
                await Delay(3000);
                return;
            }
            
            _closestTv = entity;
            
            if (Vector3.DistanceSquared(_closestTv.Position, plyPos) > 3f)
            {
                await Delay(1500);
                return;
            }
                
            if (!_focusingTv)
            {
                Screen.DisplayHelpTextThisFrame(_tvOn ? "Press ~INPUT_CONTEXT~ to watch TV." : "Press ~INPUT_CONTEXT~ to turn on the TV.");
                
                if (Game.IsControlJustReleased(0, Control.Context))
                {
                    await WatchTv();
                }
            }
        }

        [Tick]
        private async Task BlockWeaponsInsideTick()
        {
            if (_closestTv is null)
            {
                await Delay(1500);
                return;
            }
            
            int currentInterior = GetInteriorFromEntity(Game.PlayerPed.Handle);
            int tvInterior = GetInteriorFromEntity(_closestTv.Handle);
            
            if (currentInterior == tvInterior && Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Unarmed)
            {
                Screen.DisplayHelpTextThisFrame("A weapon cannot be equipped when in a safehouse.");
                Game.PlayerPed.Weapons.Select(WeaponHash.Unarmed);
                Game.PlayerPed.SetConfigFlag(48, true);
            }
            else if (Game.PlayerPed.GetConfigFlag(48) && currentInterior != tvInterior)
            {
                Game.PlayerPed.SetConfigFlag(48, false);
            }

            await Delay(2000);
        }
    }    
}