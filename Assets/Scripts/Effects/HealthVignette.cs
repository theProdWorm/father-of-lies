using Entities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Animation
{
    [RequireComponent(typeof(Volume))]
    public class HealthVignette : MonoBehaviour
    {
        [SerializeField] private ParticleSystem.MinMaxCurve _healthIntensityCurve;
        [SerializeField] private float _minIntensity;
        [SerializeField] private float _maxIntensity;
        [SerializeField] private float _pulseSpeed;
        [SerializeField] private float _lowHealthPulseSpeed;
        
        private Volume _volume;
        private Vignette _vignette;

        private float _playerHealthPercent = 1f;

        private void Awake()
        {
            _volume = GetComponent<Volume>();
            _volume.profile.TryGet(out _vignette);
        }
        
        private void Start()
        {
            var player = FindFirstObjectByType<PlayerEntity>();
            if(player){ player.OnHealthChanged.AddListener((current, max) => _playerHealthPercent = current / (float) max);}
            else Debug.LogWarning("No player found");
        }
        
        private void Update()
        {
            float healthIntensity = _healthIntensityCurve.Evaluate(_playerHealthPercent);

            if (healthIntensity <= 0)
            {
                _vignette.intensity.value = 0;
                return;
            }
            
            float pulseT = (Mathf.Sin(_pulseSpeed * Time.realtimeSinceStartup * Mathf.PI * 2) + 1) / 2;
            float intensity = Mathf.Lerp(_minIntensity, _maxIntensity, healthIntensity * pulseT);

            _vignette.intensity.value = intensity;
        }
    }
}