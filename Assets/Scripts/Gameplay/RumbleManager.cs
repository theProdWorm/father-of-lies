using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class RumbleManager : MonoBehaviour
    {
        public static RumbleManager INSTANCE;
        private float RumbleStrengthMultiplier = 0;
        public static bool PLAYER_MOVING_IN_WATER;
        

        [SerializeField] private float _waterLowFrequency;
        [SerializeField] private float _waterHighFrequency;
        
        private Coroutine _rumbleCoroutine;
        
        private void Awake()
        {
            if (INSTANCE)
                Destroy(gameObject);
            else
                INSTANCE = this;
        }

        private void Start()
        {
            SettingsMenu.INSTANCE.RumbleSlider.onValueChanged.AddListener(SetRumbleMultiplier);
            SettingsMenu.INSTANCE.RumbleSlider.value = PlayerPrefs.HasKey("RumbleMultiplier") ? PlayerPrefs.GetFloat("RumbleMultiplier") : 1f;
        }

        private void OnDisable()
        {
            PlayerPrefs.SetFloat("RumbleMultiplier", RumbleStrengthMultiplier);
        }

        private void Update()
        {
            if (Gamepad.current == null || _rumbleCoroutine != null) 
                return;
            
            if (PLAYER_MOVING_IN_WATER)
                Gamepad.current?.SetMotorSpeeds(_waterLowFrequency * RumbleStrengthMultiplier, _waterHighFrequency * RumbleStrengthMultiplier);
            else
                Gamepad.current?.SetMotorSpeeds(0, 0);
        }
        
        public void StopRumble()
        {
            Gamepad.current?.SetMotorSpeeds(0, 0);

            if (_rumbleCoroutine == null)
                return;
            
            StopCoroutine(_rumbleCoroutine);
            _rumbleCoroutine = null;
        }

        public void Rumble(RumbleEvent rumbleEvent)
        {
            if (Gamepad.current == null)
                return;
            
            if (_rumbleCoroutine != null)
                StopCoroutine(_rumbleCoroutine);
            
            _rumbleCoroutine = StartCoroutine(RumbleCoroutine(rumbleEvent));
        }

        private IEnumerator RumbleCoroutine(RumbleEvent rumbleEvent)
        {
            float elapsedTime = 0;

            while (elapsedTime < rumbleEvent.Duration)
            {
                float t = Mathf.Clamp01(elapsedTime / rumbleEvent.Duration);
                
                float low = rumbleEvent.LowFrequency.Evaluate(t);
                float high = rumbleEvent.HighFrequency.Evaluate(t);
                
                low *= RumbleStrengthMultiplier;
                high *= RumbleStrengthMultiplier;
                
                Gamepad.current.SetMotorSpeeds(low, high);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            StopRumble();
        }

        private void SetRumbleMultiplier(System.Single value)
        {
            RumbleStrengthMultiplier = math.clamp(value,0,1);
        }

        private void OnDestroy()
        {
            StopRumble();
        }
    }
}
