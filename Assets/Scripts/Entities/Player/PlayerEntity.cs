using Abilities;
using Abilities.Attacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Entities.Stats;
using FMODUnity;
using GameManager;
using Gameplay;
using Gameplay.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace Entities
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

#if UNITY_EDITOR
        protected static bool HEL_UNLOCKED = true;
#else
        protected static bool HEL_UNLOCKED = false;
#endif

        public UnityEvent<int, int> OnHealthChanged;
        public UnityEvent<int, int> OnPotionChargesChanged;
        public UnityEvent OnPotionDrunk;
        public UnityEvent OnDashStarted;
        public UnityEvent OnDashFinished;

        [SerializeField] private Transform _characterContainer;
        [SerializeField] private PlayerInput _playerInput;

        [SerializeField] private float _invincibilityDuration;

        [Tooltip("Amount of time (in seconds) in advance the player can press an input for it to count.")]
        [SerializeField] private float _inputBufferMargin;

        [SerializeField] private float _lowHealthThreshold;

        [Header("Movement")]
        [SerializeField] private float _animationLockMoveSpeedFadeDuration;
        [SerializeField] private float _insideHoleSpeedMultiplier;

        [SerializeField] private ParticleSystem _grassStepVFX;
        [SerializeField] private VisualEffect _waterStepVFX;

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

        [Header("Dash")]
        [SerializeField] private Ability _dashAbility;

        [SerializeField] private Transform _dashPoint;

        [Range(0.02f, 0.5f)]
        [SerializeField] private float _dashDuration;

        [Range(0f, 1f), Tooltip("Fraction of dash duration to fade back to normal speed.")]
        [SerializeField] private float _dashFade;

        [Tooltip("The fraction cutoff for dashing OVER holes")]
        [Range(0.5f, 1f)]
        [SerializeField] private float _dashHoleSnapFraction;

        [SerializeField] private LayerMask _dashingPlayerLayer;

        [SerializeField] protected CharacterAbilitySet _abilities;
        [SerializeField] protected Animator _animator;
        [SerializeField] protected Transform _attackPoint;

        protected AbilityTracker _dashAbilityTracker;

        private Ability _currentAbility;
        private int _currentAbilityUseTimes;

        private PlayerBaseStats _playerBaseStats;

        private Camera _camera;

        private Vector2 _moveInput;

        private float _critChance;
        private float _critDamage;

        private float _damageReduction = 0f;

        private int _potionCharges;
        private bool PotionReady => _potionCharges >= _potionCost;

        private float _originalDashDistance;
        private float _originalMoveSpeed;
        private Coroutine _dashCoroutine;

        private Vector3 _targetPos;

        private bool _deltaIsMoving;
        private bool _deltaAboveHole;
        private bool _isDashing;
        private float _dashSpeed;

        private bool _isMoving;

        private bool _hasControl = true;
        private float _controlLossDuration;

        private float _remainingInvincibilityDuration;

        private Coroutine _lungeCoroutine;
        private Vector3 _lungeForce;

        public bool FlipFenrirAttack;

        private InputBuffer _inputBuffer;

        private List<IInteractable> _interactables = new();
        private IInteractable _currentInteractable;

        protected override void Awake()
        {
            base.Awake();

            INSTANCE = this;
            OnDeath.AddListener(_ => PlayerSpawner.DIED = true);

            _playerInput.SwitchCurrentActionMap("Dialogue");
            _playerInput.SwitchCurrentActionMap("UI");
            _playerInput.SwitchCurrentActionMap("Player");
        }

        protected override void Start()
        {
            base.Start();

            if (StatsPersistence.PlayerHealth > 0)
                _currentHealth = StatsPersistence.PlayerHealth;

            if (StatsPersistence.HealthItemAmount > 0)
                _potionCharges = StatsPersistence.HealthItemAmount;

            SceneManager.OnSceneLoaded.AddListener(() =>
            {
                StatsPersistence.PlayerHealth = _currentHealth;
                StatsPersistence.HealthItemAmount = _potionCharges;
            });

            _rigidbody.maxAngularVelocity = 0;

            _camera = Camera.main!;

            _inputBuffer = new(_inputBufferMargin);

            _originalMoveSpeed = _moveSpeed;
            _originalDashDistance = Vector3.Distance(transform.position, _dashPoint.position);

            _dashSpeed = _originalDashDistance / _dashDuration;

            InitializeAbilityTrackers();

            OnHealthChanged.AddListener((current, max) =>
                FMODEvents.SetLowHealth((float)current / max <= _lowHealthThreshold));

            //Sync the health UI at the start
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnPotionChargesChanged?.Invoke(_potionCharges, _maxPotionCharges);
            CharacterIndexChanged();
        }

        private void InitializeAbilityTrackers()
        {
            _attackAbilityTrackers = new AttackAbilityTracker[]
            {
                new(_fenrirAbilities.Attack, (ability, action) =>
                {
                    FlipFenrirAttack = !FlipFenrirAttack;
                    _fenrirAnimator.SetBool(ATTACK_FLIP, FlipFenrirAttack);

                    StartAttack(ability, action, ATTACK);
                }),
                new(_helAbilities.Attack, (ability, action) =>
                    StartAttack(ability, action, ATTACK))
            };

            _switchAbilityTrackers = new AttackAbilityTracker[]
            {
                new(_fenrirAbilities.Switch, (ability, action) =>
                {
                    ActiveCharacter = (Character)((int)++ActiveCharacter % 2);
                    CharacterIndexChanged();

                    StartAttack(ability, action, SWITCH);
                }),
                new(_helAbilities.Switch, (ability, action) =>
                {
                    ActiveCharacter = (Character)((int)++ActiveCharacter % 2);
                    CharacterIndexChanged();

                    StartAttack(ability, action, SWITCH);
                })
            };

            _dashAbilityTracker = new(_dashAbility, () => PerformDash(_dashPoint.position, true));
        }

        public void LoseControl() => _hasControl = false;
        public void GainControl() => _hasControl = true;

        public void SetDashing(bool isDashing) => _isDashing = isDashing;
        public void SetDashing() => _isDashing = true;

        public float GetSwitchCooldownPercent() => SwitchAbilityTracker.RemainingCooldownPercent;

        protected override void Update()
        {
            base.Update();
            CurrentAnimator.SetFloat(SPEED, _aboveHole ? _insideHoleSpeedMultiplier : 1f);

            _inputBuffer.Update();
            if (_hasControl)
                _inputBuffer.NextInput();

            foreach (var attackAbilityTracker in _attackAbilityTrackers)
                attackAbilityTracker.Update();
            foreach (var switchAbilityTracker in _switchAbilityTrackers)
                switchAbilityTracker.Update();

            _dashAbilityTracker.Update();

            if (!_hasControl && !_isDashing)
            {
                if (_animationLockMoveSpeedFadeDuration == 0)
                {
                    _moveSpeed = 0;
                }
                else
                {
                    float t = Mathf.Clamp01(_controlLossDuration / _animationLockMoveSpeedFadeDuration);
                    _moveSpeed = Mathf.Lerp(_originalMoveSpeed, 0, t);
                }
            }
            else if (!_isDashing)
            {
                _moveSpeed = _originalMoveSpeed;
            }

            MoveAndRotate();

            if (_interactables.Count > 0)
                FindMainInteractable();

            if (!_hasControl)
                _controlLossDuration += Time.deltaTime;
            else
                _controlLossDuration = 0;

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

                float speed = _moveSpeed * (_aboveHole ? _insideHoleSpeedMultiplier : 1);
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

            RumbleManager.PLAYER_MOVING_IN_WATER = _aboveHole && _isMoving;
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

        private Vector3 FindTarget()
        {
            var enemies = EncounterManager.ALIVE_ENEMIES;

            List<float> distances = new();
            List<float> angles = new();

            Vector3 forwardDirection;

            if (_useCameraDirection)
            {
                var cameraForward = _camera.transform.forward;
                var downProjection = Vector3.Project(cameraForward, Vector3.up);

                forwardDirection = (cameraForward - downProjection).normalized;
            }
            else
            {
                forwardDirection = transform.forward;
            }

            var validEnemies = enemies.Where(enemy =>
            {
                if (!enemy.HasSpawned || enemy.IsDead)
                    return false;

                float distance = Vector3.Distance(enemy.transform.position, transform.position);
                if (distance > _targetLockMaxDistance)
                    return false;

                Vector3 toVector = enemy.transform.position - transform.position;
                float angle = Mathf.Abs(Vector3.Angle(forwardDirection, toVector));

                if (angle > _targetLockAngle)
                    return false;

                distances.Add(distance);
                angles.Add(angle);

                return true;
            }).ToArray();

            if (validEnemies.Length == 0)
                return null;

            int targetIndex = 0;
            float maxWeight = 0;
            for (int i = 0; i < validEnemies.Length; i++)
            {
                float distanceWeight = _targetLockDistanceWeight *
                                       (1 - Mathf.Clamp01(distances[i] / _targetLockMaxDistance));
                float angleWeight = _targetLockAngleWeight *
                                    (1 - Mathf.Clamp01(angles[i] / _targetLockAngle));

                float weight = distanceWeight * angleWeight;

                if (weight <= maxWeight)
                    continue;

                maxWeight = weight;
                targetIndex = i;
            }

            var targetPos = validEnemies[targetIndex].transform.position;
            return targetPos;
        }

        public void PerformAttack(Transform attackPoint)
        {
            GainControl();

            if (!_currentAbility)
                return;

            var attackStats = new AttackStats(
                _currentAbility.AttackPrefab,
                _damage);

            var position = attackPoint.position;

            if (_currentAbility.Burst)
                StartCoroutine(AttackCoroutine(attackStats, _currentAbilityUseTimes,
                    _currentAbility.BurstDelay, _currentAbility.SpreadAngle, position));
            else
                Attack.Create(this, position, transform.rotation, attackStats);

            string attackName = attackStats.Prefab.name.ToLower();
            EventReference sound;

            if (attackName.Contains("switch"))
                sound = FMODEvents.INSTANCE._playerSwitchIn;
            else
                sound = FMODEvents.INSTANCE._playerAttack;

            FMODEvents.INSTANCE.PlayEvent(sound, transform.position);
        }

        public void PerformAttackParented(Transform attackPoint)
        {
            GainControl();

            if (!_currentAbility)
                return;

            var attackStats = new AttackStats(
                _currentAbility.AttackPrefab,
                _damage,
                _critChance,
                _critDamage);

            var position = attackPoint.position;

            if (_currentAbility.Burst)
                StartCoroutine(AttackCoroutine(attackStats, _currentAbilityUseTimes,
                    _currentAbility.BurstDelay, _currentAbility.SpreadAngle, position));
            else
                Attack.Create(this, attackPoint, attackStats);

            string attackName = attackStats.Prefab.name.ToLower();
            EventReference sound;

            if (attackName.Contains("switch"))
                sound = FMODEvents.INSTANCE._playerSwitchIn;
            else
                sound = FMODEvents.INSTANCE._playerAttack;

            FMODEvents.INSTANCE.PlayEvent(sound, transform.position);
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

            instance.setParameterByName("FloorType", _aboveHole ? 1 : 0);
            instance.start();

            if (_aboveHole)
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

        public void MoveInput(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

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

        public void DashInput(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            _inputBuffer.Add(_dashAbilityTracker.TryUse);
        }

        public abstract void SwitchInput(InputAction.CallbackContext context);

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

            // var sound = FMODEvents.INSTANCE._playerDeath;
            // FMODEvents.INSTANCE.PlayEvent(sound, transform.position);
        }
    }
}