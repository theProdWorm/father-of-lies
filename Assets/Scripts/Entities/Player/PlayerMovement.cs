using System.Collections;
using Abilities;
using Audio;
using Gameplay;
using Gameplay.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace Entities.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public static bool IS_MOVING;
        
        private static PlayerMovement INSTANCE;
        
        [Header("Speed Control")]
        [SerializeField] private float _speed;
        [SerializeField] private float _animationLockMoveSpeedFadeDuration;
        [SerializeField] private float _insideHoleSpeedMultiplier;
        
        [Header("VFX")]
        [SerializeField] private ParticleSystem _grassStepVFX;
        [SerializeField] private VisualEffect _waterStepVFX;
        
        [Header("Dash")]
        [SerializeField] private float _dashInputBufferMargin = 0.1f;
        [SerializeField] private Ability _dashAbility;
        [SerializeField] private Transform _dashPoint;
        
        [Range(0.02f, 0.5f)]
        [SerializeField] private float _dashDuration;
        [Range(0f, 1f), Tooltip("Fraction of dash duration to fade back to normal speed.")]
        [SerializeField] private float _dashFade;

        private LayerMask _dashingPlayerLayer;
        private LayerMask _wallLayer;
        
        private Camera _camera;
        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;
        
        private PlayerEntity _playerEntity;
        
        private AbilityTracker _dashAbilityTracker;
        
        private Vector2 _moveInput;
        
        private bool _deltaIsMoving;
        private bool _deltaAboveHole;
        private bool _isDashing;
        private float _dashSpeed;
        
        private float _originalDashDistance;
        private float _originalMoveSpeed;
        private Coroutine _dashCoroutine;
        
        private float _controlLossDuration;

        private void Awake()
        {
            _dashingPlayerLayer = LayerMask.GetMask("DashingPlayer");
            _wallLayer = LayerMask.GetMask("Ground");
            
            _originalMoveSpeed = _speed;
            _originalDashDistance = Vector3.Distance(transform.position, _dashPoint.position);
            _dashSpeed = _originalDashDistance / _dashDuration;
            
            _dashAbilityTracker = new(_dashAbility, () => PerformDash(_dashPoint.position, true));
        }

        private void Start()
        {
            _camera = Camera.main!;
            _rigidbody = GetComponent<Rigidbody>();
            
            _rigidbody.maxAngularVelocity = 0;
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
            
            MoveAndRotate();
            
            PlayerController.SetAnimationSpeed(_playerEntity.AboveHole ? _insideHoleSpeedMultiplier : 1f);
        }
        
        private void MoveAndRotate()
        {
            Vector3 movement = Vector3.zero;

            if (!_isDashing && PlayerController.HAS_CONTROL)
            {
                var cameraForward = _camera.transform.forward;
                var downProjection = Vector3.Project(cameraForward, Vector3.up);

                var forwardDirection = (cameraForward - downProjection).normalized;
                var rightDirection = _camera.transform.right.normalized;

                Vector3 movementX = _moveInput.x * rightDirection;
                Vector3 movementZ = _moveInput.y * forwardDirection;

                float speed = _speed * (_playerEntity.AboveHole ? _insideHoleSpeedMultiplier : 1);
                movement = speed * (movementX + movementZ).normalized;
            }
            else if (_isDashing)
            {
                movement = _speed * transform.forward;
            }

            _rigidbody.linearVelocity = movement;

            transform.LookAt(transform.position + _rigidbody.linearVelocity);

            IS_MOVING = movement.magnitude > 0.01f;

            RumbleManager.PLAYER_MOVING_IN_WATER = _playerEntity.AboveHole && IS_MOVING;
        }
        
        private void PerformDash(Vector3 dashPoint, bool animate)
        {
            var sound = FMODEvents.INSTANCE._playerDash;
            FMODEvents.INSTANCE.PlayEvent(sound, transform.position);

            // Projected dash vector using the calculated offset from player center to front
            Vector3 dashVector = dashPoint - _rigidbody.position;
            float distance = dashVector.magnitude;

            // Distance from center of player to the front collision point
            Vector3 collisionPointOffset =
                dashVector.normalized * (0.02f + _collider.radius * 2);

            bool hitWall = Physics.Raycast(transform.position, dashVector, out var rHit, distance, _wallLayer);
            if (hitWall)
            {
                var wallDistance = rHit.distance;
                dashVector = dashVector.normalized * wallDistance;

                dashPoint = transform.position + dashVector - collisionPointOffset;
            }

            if (_dashCoroutine != null)
            {
                StopCoroutine(_dashCoroutine);
                _speed = _originalMoveSpeed;
            }

            _dashCoroutine = StartCoroutine(DashCoroutine(dashPoint, animate));
        }
        
        private IEnumerator DashCoroutine(Vector3 dashPoint, bool animate)
        {
            if (animate)
                PlayerController.TriggerDashAnimation();

            SetDashing(true);
            PlayerController.LoseControl();

            // int defaultPlayerLayer = gameObject.layer;
            //
            // int dashingPlayerLayer = _dashingPlayerLayer;
            // int dashLayer = 0;
            // while ((dashingPlayerLayer >>= 1) > 0)
            //     dashLayer++;
            //
            // gameObject.layer = dashLayer;

            float actualDashDistance = Vector3.Distance(transform.position, dashPoint);
            float dashDistanceFraction = Mathf.Clamp01(actualDashDistance / _originalDashDistance);

            float dashDuration = _dashDuration * dashDistanceFraction;

            _speed = _dashSpeed;

            float elapsedTime = 0;
            while (elapsedTime < dashDuration)
            {
                Vector3 velocityVector = _rigidbody.linearVelocity * Time.fixedDeltaTime;
                float moveDistance = velocityVector.magnitude;
                float distanceToDashPoint = Vector3.Distance(_rigidbody.position, dashPoint);

                if (moveDistance > distanceToDashPoint)
                {
                    float fraction = distanceToDashPoint / moveDistance;
                    _speed *= fraction;
                    yield return new WaitForFixedUpdate();
                    break;
                }

                elapsedTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            SetDashing(false);
            PlayerController.GainControl();

            elapsedTime = 0;
            while (elapsedTime < _dashFade)
            {
                elapsedTime += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / _dashFade);

                _speed = Mathf.Lerp(_dashSpeed, _originalMoveSpeed, t);

                yield return new WaitForFixedUpdate();
            }

            _speed = _originalMoveSpeed;
        }

        public void TakeStep()
        {
            var sound = FMODEvents.INSTANCE._playerFootstep;
            FMODEvents.INSTANCE.CreateEvent(sound, out var instance);

            instance.setParameterByName("FloorType", _playerEntity.AboveHole ? 1 : 0);
            instance.start();

            if (_playerEntity.AboveHole)
                _waterStepVFX.SendEvent("StartSplash");
            else
            {
                _grassStepVFX.Play();
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

            InputBuffer.Add(_dashAbilityTracker.TryUse, _dashInputBufferMargin);
        }
    }
}