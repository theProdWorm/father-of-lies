using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIEnemyHealth : MonoBehaviour
    {
        private static List<UIEnemyHealth> BARS = new List<UIEnemyHealth>();
        private static bool ALLENABLED = true;

        [SerializeField]
        private Slider _healthSlider;

        [SerializeField]
        private Slider _healthLagBar;

        [SerializeField] private float _healthFadeDelay;
        [SerializeField] private float _healthFadeDuration;

        private Coroutine _healthLagRoutine;

        private int _storedMax = 1;
        private int _storedCurrent = 1;
        
        private bool _enabled = true;

        public static void SlidersEnabled(bool value)
        {
            foreach (var bar in BARS)
            {
                bar.SetSlider(value);
            }
            ALLENABLED = value;
        }

        private void SetSlider(bool value)
        {
            _healthSlider.gameObject.SetActive(value);
            _enabled = value;
        }

        private void OnEnable()
        {
            BARS.Add(this);
            UpdateHealthUI(_storedCurrent, _storedMax);
            if (!ALLENABLED)
            {
                SetSlider(false);
            }
        }

        private void OnDisable()
        {
            _storedMax = (int)_healthSlider.maxValue;
            _storedCurrent = (int)_healthSlider.value;
            BARS.Remove(this);
        }

        public void UpdateHealthUI(int currentHealth, int maxHealth)
        {
            float deltaHealth = _healthSlider.value;

            //TODO: Consider adding an effect when hit
            if (_healthSlider == null)
                return;
            
            //_healthSlider = GetComponentInChildren<Slider>();

            _healthSlider.maxValue = maxHealth;
            _healthLagBar.maxValue = maxHealth;
            _healthSlider.value = currentHealth;

            //TODO: Consider adding a fade out effect instead of just deactivating the game object when health is full. This would make it look smoother and more polished.
            if (currentHealth >= maxHealth || currentHealth <= 0)
            {
                _healthSlider.gameObject.SetActive(false);
                _healthLagBar.gameObject.SetActive(false);
            }
            else if(_enabled)
            {
                _healthSlider.gameObject.SetActive(true);
                _healthLagBar.gameObject.SetActive(true);
            }

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
                _healthLagBar.value = Mathf.Lerp(startValue, _healthSlider.value, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _healthLagRoutine = null;
        }
    }
}