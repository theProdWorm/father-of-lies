using System.Collections;
using System.Collections.Generic;
using Abilities;
using Audio;
using Entities.Stats;
using GameManager;
using Gameplay;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace Entities.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerEntity : Entity
    {
        public static PlayerEntity INSTANCE;

        protected static readonly int IS_MOVING = Animator.StringToHash("isMoving");
        protected static readonly int DASH = Animator.StringToHash("dash");
        protected static readonly int ATTACK = Animator.StringToHash("attack");
        protected static readonly int SWITCH = Animator.StringToHash("switch");
        protected static readonly int SPEED = Animator.StringToHash("speed");

        public UnityEvent<int, int> OnHealthChanged;
        public UnityEvent<int, int> OnPotionChargesChanged;
        public UnityEvent OnPotionDrunk;
        public UnityEvent OnDashStarted;
        public UnityEvent OnDashFinished;

        [SerializeField] private float _invincibilityDuration;

        [SerializeField] private float _lowHealthThreshold;

        [Header("Movement")]

        [Header("Target Lock")]
        [SerializeField] private float _targetLockAngle;

        [SerializeField] private float _targetLockMaxDistance;
        [SerializeField] private float _targetLockAngleWeight;
        [SerializeField] private float _targetLockDistanceWeight;

        [SerializeField] private bool _useCameraDirection = true;

        [Header("Collision")]
        [SerializeField] private CapsuleCollider _collider;

        [SerializeField] private LayerMask _wallLayer;

        [Header("Interaction")]
        [SerializeField] private float _lookWeight;

        [SerializeField] private float _distanceWeight;

        [Header("Healing")]
        [SerializeField] private int _potionHealAmount;
        [SerializeField] private int _potionCost;
        [SerializeField] private int _maxPotionCharges;

        [SerializeField] protected CharacterAbilitySet _abilities;
        [SerializeField] protected Animator _animator;
        [SerializeField] protected Transform _attackPoint;
        
        private Camera _camera;
        
        private int _potionCharges;
        private bool PotionReady => _potionCharges >= _potionCost;

        private Vector3 _targetPos;

        private float _remainingInvincibilityDuration;

        private Coroutine _lungeCoroutine;
        private Vector3 _lungeForce;

        public bool FlipFenrirAttack;

        private readonly List<IInteractable> _interactables = new();
        private IInteractable _currentInteractable;

        protected override void Awake()
        {
            base.Awake();

            INSTANCE = this;
            OnDeath.AddListener(_ => PlayerSpawner.DIED = true);
        }

        protected override void Start()
        {
            base.Start();

            if (StatsPersistence.PlayerHealth > 0)
                _currentHealth = StatsPersistence.PlayerHealth;

            if (StatsPersistence.HealthItemAmount > 0)
                _potionCharges = StatsPersistence.HealthItemAmount;

            SceneManager.OnSceneExit.AddListener(() =>
            {
                StatsPersistence.PlayerHealth = _currentHealth;
                StatsPersistence.HealthItemAmount = _potionCharges;
            });

            _rigidbody.maxAngularVelocity = 0;

            _camera = Camera.main!;
            
            OnHealthChanged.AddListener((current, max) =>
                FMODEvents.SetLowHealth((float)current / max <= _lowHealthThreshold));

            //Sync the health UI at the start
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnPotionChargesChanged?.Invoke(_potionCharges, _maxPotionCharges);
            CharacterIndexChanged();
        }

        protected override void Update()
        {
            base.Update();
            CurrentAnimator.SetFloat(SPEED, AboveHole ? _insideHoleSpeedMultiplier : 1f);

            

            MoveAndRotate();

            if (_interactables.Count > 0)
                FindMainInteractable();


            if (_remainingInvincibilityDuration > 0)
                _remainingInvincibilityDuration -= Time.deltaTime;
        }

        private void MoveAndRotate()
        {
            Vector3 movement = Vector3.zero;

            if (!_isDashing && _hasControl)
            {
                var cameraForward = _camera.transform.forward;
                var downProjection = Vector3.Project(cameraForward, Vector3.up);

                var forwardDirection = (cameraForward - downProjection).normalized;
                var rightDirection = _camera.transform.right.normalized;

                Vector3 movementX = _moveInput.x * rightDirection;
                Vector3 movementZ = _moveInput.y * forwardDirection;

                float speed = _moveSpeed * (AboveHole ? _insideHoleSpeedMultiplier : 1);
                movement = speed * (movementX + movementZ).normalized;
            }
            else if (_isDashing)
            {
                movement = _moveSpeed * transform.forward;
            }

            _rigidbody.linearVelocity = movement;

            if (_lungeCoroutine != null)
                _rigidbody.linearVelocity += _lungeForce;

            transform.LookAt(transform.position + _rigidbody.linearVelocity);

            if (_knockbackCoroutine != null)
                _rigidbody.linearVelocity += _knockbackForce;

            _isMoving = movement.magnitude > 0.01f;
            CurrentAnimator.SetBool(IS_MOVING, _isMoving);

            RumbleManager.PLAYER_MOVING_IN_WATER = AboveHole && _isMoving;
        }

        private IEnumerator LungeFadeCoroutine(Vector3 direction, float originalForce, float duration)
        {
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);

                float force = originalForce * Mathf.Abs(Mathf.Pow(t, 3) - 1);
                _lungeForce = force * direction;

                yield return null;
            }

            _lungeForce = Vector3.zero;
            _lungeCoroutine = null;
        }

        public void PerformAttackLunge()
        {
            var toTarget = _targetPos - transform.position;
            var distanceToTarget = toTarget.magnitude;
            var direction = toTarget.normalized;

            float projectedDistance = 0.75f * _fenrirLungeForce * _fenrirLungeDuration;
            float duration = _fenrirLungeDuration * Mathf.Clamp01(distanceToTarget / projectedDistance);

            if (_lungeCoroutine != null)
            {
                StopCoroutine(_lungeCoroutine);
                _lungeCoroutine = null;
            }

            _lungeCoroutine = StartCoroutine(LungeFadeCoroutine(direction, _fenrirLungeForce, duration));
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
                _moveSpeed = _originalMoveSpeed;
            }

            _dashCoroutine = StartCoroutine(DashCoroutine(dashPoint, animate));
        }

        private IEnumerator DashCoroutine(Vector3 dashPoint, bool animate)
        {
            if (animate)
                CurrentAnimator.SetTrigger(DASH);

            OnDashStarted?.Invoke();

            SetDashing(true);
            LoseControl();

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

            _moveSpeed = _dashSpeed;

            float elapsedTime = 0;
            while (elapsedTime < dashDuration)
            {
                Vector3 velocityVector = _rigidbody.linearVelocity * Time.fixedDeltaTime;
                float moveDistance = velocityVector.magnitude;
                float distanceToDashPoint = Vector3.Distance(_rigidbody.position, dashPoint);

                if (moveDistance > distanceToDashPoint)
                {
                    float fraction = distanceToDashPoint / moveDistance;
                    _moveSpeed *= fraction;
                    yield return new WaitForFixedUpdate();
                    break;
                }

                elapsedTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            //gameObject.layer = defaultPlayerLayer;

            OnDashFinished?.Invoke();

            SetDashing(false);
            GainControl();

            elapsedTime = 0;
            while (elapsedTime < _dashFade)
            {
                elapsedTime += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / _dashFade);

                _moveSpeed = Mathf.Lerp(_dashSpeed, _originalMoveSpeed, t);

                yield return new WaitForFixedUpdate();
            }

            _moveSpeed = _originalMoveSpeed;
        }

        public override int TakeDamage(int amount, Entity attacker)
        {
            if (_remainingInvincibilityDuration > 0)
                return 0;

            int reducedDamage = Mathf.CeilToInt(amount * (1 - _damageReduction));
            int realDamage = base.TakeDamage(reducedDamage, attacker);

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            var sound = FMODEvents.INSTANCE._playerHit;
            FMODEvents.INSTANCE.PlayEvent(sound, transform.position);

            _remainingInvincibilityDuration = _invincibilityDuration;

            return realDamage;
        }

        public override void Heal(int amount)
        {
            base.Heal(amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void AddPotionCharges(Entity _, int damage)
        {
            if (_potionCharges >= _maxPotionCharges)
                return;

            _potionCharges += damage;
            OnPotionChargesChanged?.Invoke(_potionCharges, _maxPotionCharges);
        }

        private void CharacterIndexChanged()
        {
            for (int i = 0; i < _characterContainer.childCount; i++)
            {
                bool activeState = i == (int)ActiveCharacter;

                var character = _characterContainer.GetChild(i);
                character.gameObject.SetActive(activeState);
            }

            FMODEvents.SetCharacter(ActiveCharacter == Character.Hel);
            StatsPersistence.IsFenrir = ActiveCharacter == Character.Fenrir;
        }

        public void UnlockHel() => HEL_UNLOCKED = true;

        public void TakeStep()
        {
            var sound = FMODEvents.INSTANCE._playerFootstep;
            FMODEvents.INSTANCE.CreateEvent(sound, out var instance);

            instance.setParameterByName("FloorType", AboveHole ? 1 : 0);
            instance.start();

            if (AboveHole)
                _waterStepVFX.SendEvent("StartSplash");
            else
            {
                _grassStepVFX.Play();
            }
        }

        #region Collision

        private void FindMainInteractable()
        {
            if (_currentInteractable != null)
                _currentInteractable.Highlighted = false;

            int lowestIndex = 0;
            float highestScore = 0;
            for (int i = 0; i < _interactables.Count; i++)
            {
                var between = (_interactables[i].Position - _rigidbody.position);
                var distance = between.magnitude;
                var direction = between / distance;

                float distScore = 1 - Mathf.Clamp01(distance / 10f);
                var dot = Vector3.Dot(transform.forward, direction);

                float score = dot * _lookWeight + distScore * _distanceWeight;
                if (score > highestScore)
                {
                    lowestIndex = i;
                    highestScore = score;
                }
            }

            _currentInteractable = _interactables[lowestIndex];
            _currentInteractable.Highlighted = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Interactable"))
            {
                _interactables.Add(other.GetComponent<IInteractable>());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Interactable"))
            {
                IInteractable interactable = other.GetComponent<IInteractable>();

                if (interactable == _currentInteractable)
                    _currentInteractable.Highlighted = false;

                _interactables.Remove(interactable);
            }
        }

        #endregion

        #region Input

        public void InteractInput(InputAction.CallbackContext context)
        {
            if (!_hasControl || !context.performed)
                return;

            if (_interactables.Count == 0 || _currentInteractable == null)
                return;

            _currentInteractable.Interacted();
        }

        public abstract void AttackInput(InputAction.CallbackContext context);

        public void HealInput(InputAction.CallbackContext context)
        {
            if (!context.performed || !PotionReady || _currentHealth >= _maxHealth)
                return;

            _inputBuffer.Add(() =>
            {
                Heal(_potionHealAmount);

                _potionCharges -= _potionCost;
                OnPotionChargesChanged?.Invoke(_potionCharges, _maxPotionCharges);

                OnPotionDrunk?.Invoke();

                var sound = FMODEvents.INSTANCE._potionConsume;
                FMODEvents.INSTANCE.PlayEvent(sound, transform.position);

                return true;
            });
        }

        #endregion

        protected override void Die()
        {
            base.Die();
            StatsPersistence.PlayerHealth = _maxHealth;
            StatsPersistence.HealthItemAmount = 0;

            foreach (Rigidbody rbC in GetComponentsInChildren<Rigidbody>(true))
            {
                rbC.gameObject.SetActive(true);
                rbC.isKinematic = false;
            }

            foreach (var animator in _animators)
            {
                Destroy(animator);
            }
            Destroy(_collider, 1);
            Destroy(this);
        }
    }
}