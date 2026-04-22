using Audio;
using GameManager;
using Gameplay.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities.Player
{
    public class PlayerController : MonoBehaviour
    {
        public static bool IS_FENRIR;
        public static bool HAS_CONTROL;

        public static bool USE_SWITCH_ABILITY;
        
        private static PlayerController INSTANCE;
        
        [SerializeField] private PlayerInput _playerInput;

        [SerializeField] private Fenrir _fenrir;
        [SerializeField] private Hel _hel;

        [SerializeField] private float _switchInputBufferMargin = 0.1f;
        
#if UNITY_EDITOR
        protected static bool HEL_UNLOCKED = true;
#else
        protected static bool HEL_UNLOCKED = false;
#endif

        private bool CanSwitch => (IS_FENRIR && _fenrir.IsSwitchReady) || (!IS_FENRIR && _hel.IsSwitchReady);

        public static float SwitchCooldownPercent => IS_FENRIR ? INSTANCE._fenrir.SwitchCooldownPercent : INSTANCE._hel.SwitchCooldownPercent;
        
        private void Awake()
        {
            _playerInput.SwitchCurrentActionMap("Dialogue");
            _playerInput.SwitchCurrentActionMap("UI");
            _playerInput.SwitchCurrentActionMap("Player");

            INSTANCE = this;
        }
        
        private void Start()
        {
            SceneManager.OnSceneExit.AddListener(() =>
            {
                StatsPersistence.IsFenrir = IS_FENRIR;
            });
        }

        private void Update()
        {
            InputBuffer.Update();
            if (HAS_CONTROL)
                InputBuffer.NextInput();
        }

        public static void SetActiveCharacter(bool isFenrir)
        {
            if (isFenrir == IS_FENRIR)
                return;
            
            INSTANCE.Switch();
        }
        public void UnlockHel() => HEL_UNLOCKED = true;
        
        public static void LoseControl() => HAS_CONTROL = false;
        public static void GainControl() => HAS_CONTROL = true;

        public static void TriggerDashAnimation()
        {
            if (IS_FENRIR)
                INSTANCE._fenrir.TriggerDashAnimation();
            else
                INSTANCE._hel.TriggerDashAnimation();
        }

        public static void SetAnimationSpeed(float speed)
        {
            if (IS_FENRIR)
                INSTANCE._fenrir.SetAnimationSpeed(speed);
            else
                INSTANCE._hel.SetAnimationSpeed(speed);
        }
        
        private bool Switch()
        {
            if (!CanSwitch)
                return false;
            
            IS_FENRIR = !IS_FENRIR;
            StatsPersistence.IsFenrir = IS_FENRIR;
            FMODEvents.SetCharacter(!IS_FENRIR);

            return true;
        }

        public void OnSwitchInput(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            InputBuffer.Add(Switch, _switchInputBufferMargin);
        }
    }
}