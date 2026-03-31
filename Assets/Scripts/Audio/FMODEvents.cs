using System;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using UI.Narration;
using Unity.Mathematics;
using UnityEngine;

namespace Audio
{
    public class FMODEvents : MonoBehaviour
    {
        [Header("Player SFX")]
        public EventReference _playerDeath;
        public EventReference _playerHit;
        public EventReference _playerMovement;
        public EventReference _playerDash;
        public EventReference _playerFootstep;
        
        public EventReference _playerAttack;
        public EventReference _playerSwitchIn;

        [Header("Hel SFX")]
        public EventReference _helProjectileTravel;
        public EventReference _helProjectileHit;
    
        [Header("Enemy SFX")]
        public EventReference _draugrDeath;
        public EventReference _draugrHit;
        public EventReference _draugrSwing;
        
        [Header("Misc")]
        public EventReference _potionConsume;
        public EventReference _runestoneInteract;
    
        [Header("UI SFX")]
        public EventReference _uiButtonClick;
        public EventReference _uiButtonHover;
        public EventReference _gameStart;
    
        [Header("Music")]
        public EventReference _playlistMusic;
        public EventReference _menuMusic;

        [Header("Ambience Sounds")]
        public EventReference _forest;
        
        private bool _isPlayerHel;
        private bool _isPlayerLowHealth;

        private Vector3 _nextPosition;
        private readonly List<EventInstance> _eventInstances = new();
        
        public static FMODEvents INSTANCE;
        
        private Bus _masterBus;
        private Bus _musicBus;
        private Bus _brageBus;
        private Bus _sfxBus;

        private void Awake()
        {
            if (INSTANCE != null)
            {
                Destroy(INSTANCE.gameObject);
            }
            else INSTANCE = this;
            
            _masterBus = RuntimeManager.GetBus("bus:/");
            _musicBus = RuntimeManager.GetBus("bus:/Musik");
            _brageBus = RuntimeManager.GetBus("bus:/Brage");
            _sfxBus = RuntimeManager.GetBus("bus:/SFX");
            
            var settings = SettingsMenu.INSTANCE;
            
            settings.MasterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            settings.SfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
            settings.MusicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            
            settings.MasterVolumeSlider.value = PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume"): 1f;
            settings.SfxVolumeSlider.value = PlayerPrefs.HasKey("SfxVolume") ? PlayerPrefs.GetFloat("SfxVolume") : 1f;
            settings.MusicVolumeSlider.value = PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : 1f;
        }

        private void Start()
        {
            RuntimeManager.StudioSystem.setParameterByName("Menu", 0);
        }

        private void OnDisable()
        {
            var settings = SettingsMenu.INSTANCE;
            
            float masterVolume = settings.MasterVolumeSlider.value;
            float sfxVolume = settings.SfxVolumeSlider.value;
            float musicVolume = settings.MusicVolumeSlider.value;
            
            PlayerPrefs.SetFloat("MasterVolume",masterVolume );
            PlayerPrefs.SetFloat("SfxVolume", sfxVolume );
            PlayerPrefs.SetFloat("MusicVolume", musicVolume );
        }

        private void OnDestroy()
        {
            foreach (var instance in _eventInstances)
            {
                instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }

        private void Update()
        {
            for (int i = _eventInstances.Count - 1; i >= 0; i--)
            {
                var instance = _eventInstances[i];
                if (!instance.isValid())
                {
                    _eventInstances.RemoveAt(i);
                    continue;
                }
                
                instance.setParameterByName("Player_Form", _isPlayerHel ? 1 : 0);
                
                if (DialogueManager.PLAYING_VOICED_DIALOGUE)
                    instance.setParameterByName(DialogueManager.VOICE_PARAMETER_NAME, DialogueManager.DIALOGUE_PAGE);
            }
            
            RuntimeManager.StudioSystem.setParameterByName("Player_LowHealth", _isPlayerLowHealth ? 1 : 0);
        }

        public static void SetCharacter(bool isPlayerHel) => INSTANCE._isPlayerHel = isPlayerHel;
        public static void SetLowHealth(bool isLowHealth) => INSTANCE._isPlayerLowHealth = isLowHealth;
        
        public void SetNextPosition(Transform reference) => _nextPosition = reference.position;
        
        public void PlayEvent(string eventName)
        {
            var instance = RuntimeManager.CreateInstance(eventName);
            _eventInstances.Add(instance);
            
            instance.start();
        }

        public void PlayEvent(EventReference eventReference, Vector3 position)
        {
            var instance = RuntimeManager.CreateInstance(eventReference);
            
            _eventInstances.Add(instance);
            
            instance.start();
        }

        public void CreateEvent(EventReference eventReference, out EventInstance instance)
        {
            instance = RuntimeManager.CreateInstance(eventReference);
            
            _eventInstances.Add(instance);
        }

        public void StopEvent(string eventName)
        {
            foreach (var e in _eventInstances.Where(e => e.isValid()))
            {
                e.getDescription(out var description);
                description.getPath(out var path);
                
                if (path == eventName && e.isValid())
                    e.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }

        public void SetMasterVolume(System.Single value) => SetBusVolume(value,_masterBus);
        public void SetSfxVolume(System.Single value) => SetBusVolume(value,_sfxBus);
        public void SetMusicVolume(System.Single value)
        {
            SetBusVolume(value,_musicBus);
            SetBusVolume(value,_brageBus);
        }
        private void SetBusVolume(float volume, Bus bus)
        {
            bus.setVolume(math.clamp(volume, 0f, 1f));
            bus.getPath(out var path);
            bus.getVolume(out var vol);
        }

        public void SetCombat(bool inCombat)
        {
            RuntimeManager.StudioSystem.setParameterByName("Combat", inCombat ? 1 : 0);
        }
    }
}
