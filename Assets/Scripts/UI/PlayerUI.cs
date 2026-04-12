using System.Collections;
using System.ComponentModel;
using Entities;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerUI : MonoBehaviour
    {
        private static PlayerEntity _player;

        [Header("Health")]
        [SerializeField] private Slider _healthBar;
        [SerializeField] private Slider _healthLagBar;

        [SerializeField] private float _healthFadeDelay;
        [SerializeField] private float _healthFadeDuration;
        
        [Header("Potion")]
        [SerializeField] private Slider _potionBar1;
        [SerializeField] private Slider _potionBar2;

        [SerializeField] private Image _potion1Border;
        [SerializeField] private Image _potion2Border;
        [SerializeField] private Color _unfilledBorderColor;
        [SerializeField] private Color _filledBorderColor;

        [SerializeField] private Material _potion1Material;
        [SerializeField] private Material _potion2Material;

        [SerializeField] private float _filledPotionBrightness;
        
        [Header("Cooldowns")]
        [SerializeField] private Slider _switchBar;
        
        [SerializeField] private Image _switchBorder;
        [SerializeField] private Color _switchNotReadyBorderColor;
        [SerializeField] private Color _switchReadyBorderColor;
        
        private int _brightnessProperty = Shader.PropertyToID("_Brightness");
        
        private Coroutine _healthLagRoutine;
        
        private void OnEnable()
        {
            _player = PlayerEntity.INSTANCE;
            
            _player.OnHealthChanged.AddListener(UpdateHealthUI);
            _player.OnPotionChargesChanged.AddListener(UpdatePotionCharge);
        }

        private void OnDisable()
        {
            _player.OnHealthChanged.RemoveListener(UpdateHealthUI);
            _player.OnPotionChargesChanged.RemoveListener(UpdatePotionCharge);
        }

        private void Update()
        {
            float switchCooldownPercent = Mathf.Clamp01(1 - _player.GetSwitchCooldownPercent());
            _switchBar.value = switchCooldownPercent;

            bool switchReady = Mathf.Approximately(switchCooldownPercent, 1);
            _switchBorder.color = switchReady ? _switchReadyBorderColor : _switchNotReadyBorderColor;
        }

        private void UpdateHealthUI(int currentHealth, int maxHealth)
        {
            float deltaHealth = _healthBar.value;
            
            _healthBar.maxValue = maxHealth;
            _healthLagBar.maxValue = maxHealth;
            
            _healthBar.value = currentHealth;

            if (_healthLagRoutine != null)
                StopCoroutine(_healthLagRoutine);
            
            if (currentHealth < deltaHealth)
                _healthLagRoutine = StartCoroutine(HealthLagRoutine());
            else
                _healthLagBar.value = currentHealth;
        }

        private IEnumerator HealthLagRoutine()
        {
            float startValue = _healthLagBar.value;

            yield return new WaitForSeconds(_healthFadeDelay);
            
            float elapsedTime = 0;
            while (elapsedTime < _healthFadeDuration)
            {
                float t = Mathf.Clamp01(elapsedTime / _healthFadeDuration);
                _healthLagBar.value = Mathf.Lerp(startValue, _healthBar.value, t);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            _healthLagBar.value = _healthBar.value;

            _healthLagRoutine = null;
        }

        private void UpdatePotionCharge(int currentCharges, int maxCharges)
        {
            float halfMaxCharges = maxCharges * 0.5f;
            
            _potionBar1.maxValue = halfMaxCharges;
            _potionBar1.value = Mathf.Clamp(currentCharges, 0, halfMaxCharges);
            
            _potionBar2.maxValue = halfMaxCharges;
            _potionBar2.value = Mathf.Clamp(currentCharges - halfMaxCharges, 0, halfMaxCharges);

            bool potion1Charged = Mathf.Approximately(_potionBar1.value, halfMaxCharges);
            _potion1Border.color = potion1Charged ? _filledBorderColor : _unfilledBorderColor;
            
            _potion1Material.SetFloat(_brightnessProperty, potion1Charged ? _filledPotionBrightness : 1);
            
            if (!potion1Charged)
                return;
            
            bool potion2Charged = Mathf.Approximately(_potionBar2.value, halfMaxCharges);
            _potion2Border.color = potion2Charged ? _filledBorderColor : _unfilledBorderColor;
            
            _potion2Material.SetFloat(_brightnessProperty, potion2Charged ? _filledPotionBrightness : 1);
        }

        private void OnDestroy()
        {
            _potion1Material.SetFloat(_brightnessProperty, 1);
            _potion2Material.SetFloat(_brightnessProperty, 1);
        }
    }
}
