using Audio;
using GameManager;
using Gameplay.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Entities.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerEntity : Entity
    {
        public static PlayerEntity INSTANCE;

        public UnityEvent<int, int> OnHealthChanged;
        public UnityEvent<int, int> OnPotionChargesChanged;
        public UnityEvent OnPotionDrunk;

        [SerializeField] private float _invincibilityDuration;
        
        [SerializeField] private float _lowHealthThreshold;

        [Header("Healing")]
        [SerializeField] private int _potionHealAmount;
        [SerializeField] private int _potionCost;
        [SerializeField] private int _maxPotionCharges;
        [SerializeField] private float _healInputBufferMargin = 0.1f;
        
        private int _potionCharges;
        private bool PotionReady => _potionCharges >= _potionCost;

        private float _remainingInvincibilityDuration;

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
            
            OnHealthChanged.AddListener((current, max) =>
                FMODEvents.SetLowHealth((float)current / max <= _lowHealthThreshold));

            //Sync the health UI at the start
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnPotionChargesChanged?.Invoke(_potionCharges, _maxPotionCharges);
        }

        protected override void Update()
        {
            base.Update();
            
            if (_remainingInvincibilityDuration > 0)
                _remainingInvincibilityDuration -= Time.deltaTime;
        }

        private void LateUpdate()
        {
            if (_knockbackCoroutine != null)
                _rigidbody.linearVelocity += _knockbackForce;
        }
        
        public override int TakeDamage(int amount, Entity attacker)
        {
            if (_remainingInvincibilityDuration > 0)
                return 0;

            int realDamage = base.TakeDamage(amount, attacker);

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

        public void HealInput(InputAction.CallbackContext context)
        {
            if (!context.performed || !PotionReady || _currentHealth >= _maxHealth)
                return;

            InputBuffer.Add(() =>
            {
                Heal(_potionHealAmount);

                _potionCharges -= _potionCost;
                OnPotionChargesChanged?.Invoke(_potionCharges, _maxPotionCharges);

                OnPotionDrunk?.Invoke();

                var sound = FMODEvents.INSTANCE._potionConsume;
                FMODEvents.INSTANCE.PlayEvent(sound, transform.position);

                return true;
            }, _healInputBufferMargin);
        }

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

            var animators = GetComponentsInChildren<Animator>(true);
            
            foreach (var animator in animators)
            {
                Destroy(animator);
            }
            Destroy(GetComponent<Collider>(), 1);
            Destroy(this);
        }
    }
}