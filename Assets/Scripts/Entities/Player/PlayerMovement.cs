using Abilities;
using Gameplay.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace Entities.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private static PlayerMovement INSTANCE;
        
        [Header("Speed Control")]
        [SerializeField] private float _speed;
        [SerializeField] private float _animationLockMoveSpeedFadeDuration;
        [SerializeField] private float _insideHoleSpeedMultiplier;
        
        [Header("VFX")]
        [SerializeField] private ParticleSystem _grassStepVFX;
        [SerializeField] private VisualEffect _waterStepVFX;
        
        [Header("Dash")]
        [SerializeField] private float _dashInputBufferMargin;
        [SerializeField] private Ability _dashAbility;
        [SerializeField] private Transform _dashPoint;
        
        [Range(0.02f, 0.5f)]
        [SerializeField] private float _dashDuration;
        [Range(0f, 1f), Tooltip("Fraction of dash duration to fade back to normal speed.")]
        [SerializeField] private float _dashFade;

        private LayerMask _dashingPlayerLayer;

        private AbilityTracker _dashAbilityTracker;
        
        private Vector2 _moveInput;
        
        private bool _deltaIsMoving;
        private bool _deltaAboveHole;
        private bool _isDashing;
        private float _dashSpeed;
        
        private bool _isMoving;

        private float _originalDashDistance;
        private float _originalMoveSpeed;
        private Coroutine _dashCoroutine;
        
        private float _controlLossDuration;

        private void Awake()
        {
            _dashingPlayerLayer = LayerMask.GetMask("DashingPlayer");
            
            _originalMoveSpeed = _speed;
            _originalDashDistance = Vector3.Distance(transform.position, _dashPoint.position);
            _dashSpeed = _originalDashDistance / _dashDuration;
            
            _dashAbilityTracker = new(_dashAbility, () => PerformDash(_dashPoint.position, true));
        }

        private void Start()
        {
        }
        
        public static void SetDashing(bool isDashing) => INSTANCE._isDashing = isDashing;
        public static void SetDashing() => INSTANCE._isDashing = true;

        private void Update()
        {
            if (!PlayerController.HAS_CONTROL)
                _controlLossDuration += Time.deltaTime;
            else
                _controlLossDuration = 0;
            
            if (!PlayerController.HAS_CONTROL && !_isDashing)
            {
                if (_animationLockMoveSpeedFadeDuration == 0)
                {
                    _speed = 0;
                }
                else
                {
                    float t = Mathf.Clamp01(_controlLossDuration / _animationLockMoveSpeedFadeDuration);
                    _speed = Mathf.Lerp(_originalMoveSpeed, 0, t);
                }
            }
            else if (!_isDashing)
            {
                _speed = _originalMoveSpeed;
            }
        }
        
        public void MoveInput(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
        
        public void DashInput(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            InputBuffer.Add(_dashAbilityTracker.TryUse, );
        }
    }
}