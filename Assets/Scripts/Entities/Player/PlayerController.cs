using GameManager;
using Gameplay.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities.Player
{
    public class PlayerController : MonoBehaviour
    {
        public static bool HAS_CONTROL;
        
        [SerializeField] private PlayerInput _playerInput;

        [SerializeField] private Fenrir _fenrir;
        [SerializeField] private Hel _hel;

        [SerializeField] private float _switchInputBufferMargin;
        
#if UNITY_EDITOR
        protected static bool HEL_UNLOCKED = true;
#else
        protected static bool HEL_UNLOCKED = false;
#endif

        private bool _isFenrir;
        private bool CanSwitch => (_isFenrir && _fenrir.IsSwitchReady) || (!_isFenrir && _hel.IsSwitchReady);

        private void Awake()
        {
            _playerInput.SwitchCurrentActionMap("Dialogue");
            _playerInput.SwitchCurrentActionMap("UI");
            _playerInput.SwitchCurrentActionMap("Player");
        }
        
        private void Start()
        {
            SceneManager.OnSceneExit.AddListener(() =>
            {
                StatsPersistence.IsFenrir = _isFenrir;
            });
        }

        private void Update()
        {
            InputBuffer.Update();
            if (HAS_CONTROL)
                InputBuffer.NextInput();
        }
        
        public static void LoseControl() => HAS_CONTROL = false;
        public static void GainControl() => HAS_CONTROL = true;

        private bool Switch()
        {
            if (!CanSwitch)
                return false;
            
            _isFenrir = !_isFenrir;
            StatsPersistence.IsFenrir = _isFenrir;

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