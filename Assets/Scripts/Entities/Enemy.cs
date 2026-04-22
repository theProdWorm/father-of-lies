using System;
using System.Linq;
using Abilities.Attacks;
using Animation;
using Audio;
using Effects;
using Entities.Player;
using StatusEffects;
using StatusEffects.Effects;
using UI;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.VFX;

namespace Entities
{
    public class Enemy : Entity
    {
        private static readonly int MOVE_DIR = Animator.StringToHash("MoveDir");
        private static readonly int MOVE_SPEED = Animator.StringToHash("MoveSpeed");
        private static readonly int ATTACK_SPEED = Animator.StringToHash("AttackSpeed");

        private static int ENEMYAMOUNT = 0;
        private static GameObject PLAYER;

        private UIEnemyHealth _healthBar;
        private Animator animator;
        private Animator childAnimator;
        private Vector3 prevPos = Vector3.zero;
        private float prevDot = 0;
        private BehaviorGraphAgent AiAgent;
        private BlackboardVariable<ChargePrep> ChargePrepEventChannel;
        private NavMeshAgent navAgent;
        private bool ragdollActive;
        private float ragdollTimeLeft;
        private float dissolveTimeLeft;
        private SkinnedMeshRenderer _skinnedMeshRenderer;
        private Material[] materials;
        private float staggerTimeLeft;

        [SerializeField] private GameObject attackPrefab;
        [Tooltip("Where the attack will spawn")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] EncounterManager.EnemyTypes type;
        [SerializeField] private bool canBeStaggered;
        [SerializeField] private float staggerCooldown = 3f;
        public bool HasSpawned = true;

        [SerializeField] private float _aboveHoleSpeedMultiplier = 0.5f;

        [Header("Status Effects")]
        [SerializeField] private Image _statusEffectIcon;
        [SerializeField] private Image _statusEffectCountIcon;
        [SerializeField] private GameObject _iceBlockPrefab;
        [SerializeField] public bool ImmuneToStatusEffects;

        [Header("Death")]
        [Tooltip("How long the ragdoll lasts before starting to dissolve")]
        [SerializeField] private float ragdollDuration = 1f;
        [Tooltip("How long it takes for the ragdoll to dissolve")]
        [SerializeField] private float dissolveDuration = 1f;

        private StatusEffectList _statusEffects;

        private float _animationSpeed;

        protected override void Awake()
        {
            base.Awake();

            if (!ImmuneToStatusEffects)
                _statusEffects = new(this, _statusEffectIcon, _statusEffectCountIcon);

            InitializeBaseStats();
            AiAgent = GetComponent<BehaviorGraphAgent>();
            navAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            materials = _skinnedMeshRenderer.materials;
            if (!PLAYER)
                PLAYER = PlayerEntity.INSTANCE.gameObject;

            AiAgent.SetVariableValue("Target", PLAYER);
            AiAgent.SetVariableValue("Animator", GetComponent<Animator>());
            AiAgent.Start();
            navAgent.speed = _moveSpeed;

            _healthBar = GetComponentInChildren<UIEnemyHealth>();
            _healthBar.UpdateHealthUI(_currentHealth, _maxHealth);
            _animationSpeed = animator.GetFloat(MOVE_SPEED);

            ENEMYAMOUNT++;

            switch (type)
            {
                case EncounterManager.EnemyTypes.Wolf:
                    {
                        if (AiAgent.GetVariable("ChargePrep", out ChargePrepEventChannel))
                        {

                        }
                        else throw new NullReferenceException();

                        break;
                    }
                case EncounterManager.EnemyTypes.BirdOnBird:
                    {
                        childAnimator = GetComponentInChildren<Animator>();
                        break;
                    }
            }
        }

        public void ChargeReady()
        {
            ChargePrepEventChannel.Value.SendEventMessage();
        }
        public void AttackFinished()
        {
            AiAgent.SetVariableValue("Attacking", false);
        }

        public void Attack()
        {
            if (type == EncounterManager.EnemyTypes.BirdOnBird)
            {
                attackPrefab.GetComponent<HomingProjectileAttack>().target = PLAYER.transform;
                Abilities.Attacks.Attack.Create(attackPrefab, this, attackPoint.position, Quaternion.LookRotation(PLAYER.transform.position - transform.position));
            }
            else
                Abilities.Attacks.Attack.Create(attackPrefab, this, attackPoint.position, transform.rotation);

            var sound = type switch
            {
                EncounterManager.EnemyTypes.Draugr => FMODEvents.INSTANCE._draugrSwing,
                EncounterManager.EnemyTypes.BirdOnBird => FMODEvents.INSTANCE._draugrSwing,
                EncounterManager.EnemyTypes.Wolf => FMODEvents.INSTANCE._draugrSwing
            };

            FMODEvents.INSTANCE.PlayEvent(sound, transform.position);
        }

        private float dissolveValue;
        protected override void Update()
        {
            if (IsDead)
            {
                if (!ragdollActive) return;
                if (ragdollTimeLeft > 0)
                {
                    ragdollTimeLeft -= Time.deltaTime;
                }
                else
                {
                    dissolveTimeLeft -= Time.deltaTime;
                    materials[0].SetFloat("_Cutoff_Height", Mathf.InverseLerp(0, dissolveDuration, dissolveTimeLeft));
                    if (!(dissolveTimeLeft <= 0)) return;
                    ragdollActive = false;
                    Destroy(gameObject);
                }
                return;
            }

            base.Update();

            if (!ImmuneToStatusEffects)
                _statusEffects.Update();

            //navAgent.speed = _moveSpeed;

            if (staggerTimeLeft > 0) staggerTimeLeft -= Time.deltaTime;

            var pos = transform.position;
            // var rotation = Quaternion.LookRotation(PLAYER.transform.position - transform.position, Vector3.up);
            // var lerpRot = Quaternion.Lerp(transform.rotation,rotation , Time.deltaTime * rotationSpeed);
            // var rot = lerpRot.eulerAngles;
            // transform.eulerAngles = new Vector3(0, rot.y, 0);
            //transform.LookAt(_player.transform, Vector3.up);
            //var rot = transform.eulerAngles;
            //transform.eulerAngles = new Vector3(0, rot.y, 0);

            if (Vector3.Distance(pos, prevPos) < 0.1f) return;
            var between = (pos - prevPos);
            var distance = between.magnitude;
            var direction = between / distance;
            var dot = Vector3.Dot(transform.forward, direction);
            float velocity = .1f;

            float smoothed = 0;
            smoothed = Mathf.SmoothDamp(
                prevDot,
                dot,
                ref velocity,
                .05f
            );

            float speedMultiplier = AboveHole ? _aboveHoleSpeedMultiplier : 1;
            navAgent.speed = _moveSpeed * speedMultiplier;
            animator.SetFloat(MOVE_SPEED, _frozen ? 0 : _animationSpeed * speedMultiplier);

            prevDot = smoothed;
            prevPos = transform.position;

            if (type == EncounterManager.EnemyTypes.BirdOnBird) return;
            animator.SetFloat(MOVE_DIR, smoothed);
        }

        public void ApplyStatusEffect(StatusEffect effect)
        {
            if (!ImmuneToStatusEffects)
                _statusEffects.Add(effect);
        }

        public void RemoveAllStatusEffectsOfType<T>() where T : StatusEffect
        {
            if (!ImmuneToStatusEffects)
                _statusEffects.RemoveAll<T>();
        }
        public int CountStatusEffectsOfType<T>() where T : StatusEffect =>
            ImmuneToStatusEffects ? 0 : _statusEffects.GetCount<T>();
        public bool HasStatusEffectOfType<T>() where T : StatusEffect =>
            !ImmuneToStatusEffects && _statusEffects.HasEffect<T>();

        public void Destroy()
        {
            ENEMYAMOUNT--;
            AiAgent.End();
            navAgent.enabled = false;
            tag = "Untagged";
            //TODO Destroy upon ragdoll deletion
            Destroy(AiAgent);
            Destroy(navAgent);
            if (childAnimator) Destroy(childAnimator);
            Destroy(animator);
            Destroy(GetComponent<Collider>());
            Destroy(_healthBar.gameObject);

            foreach (Rigidbody rbC in GetComponentsInChildren<Rigidbody>(true))
            {
                rbC.gameObject.SetActive(true);
                rbC.isKinematic = false;
            }

            ragdollActive = true;
            ragdollTimeLeft = ragdollDuration;
            dissolveTimeLeft = dissolveDuration;

            if (!ImmuneToStatusEffects)
                _statusEffects.Clear();

            Unfreeze();
        }

        public override int TakeDamage(int amount, Entity attacker)
        {
            if (!HasSpawned)
                return 0;

            if (HasStatusEffectOfType<StatusEffect_Frozen>() &&
                attacker is PlayerEntity player)
            {
                if (PlayerController.IS_FENRIR)
                {
                    amount += Fenrir.SHATTER_DAMAGE;
                    RemoveAllStatusEffectsOfType<StatusEffect_Frozen>();
                }
                else
                {
                    amount = Mathf.CeilToInt(amount * Hel.FREEZE_DAMAGE_MULTIPLIER);
                }
            }

            int realDamage = base.TakeDamage(amount, attacker);
            DamageNumbers.CreateDamageNumber(transform, realDamage);
            _healthBar.UpdateHealthUI(_currentHealth, _maxHealth);

            if (canBeStaggered && staggerTimeLeft <= 0)
            {
                staggerTimeLeft = staggerCooldown;
                //animator.StopPlayback();
                if (type != EncounterManager.EnemyTypes.BirdOnBird) animator.SetBool("Stagger", true);
                AiAgent.SetVariableValue("Staggered", true);
                AiAgent.SetVariableValue("Attacking", false);
            }

            return realDamage;
        }

        public override void Heal(int amount)
        {
            base.Heal(amount);
            _healthBar.UpdateHealthUI(_currentHealth, _maxHealth);
        }

        private IceBlock _iceBlockInstance;
        private bool _frozen;
        public void Freeze()
        {
            if (IsDead)
                return;

            AiAgent.SetVariableValue("Frozen", true);
            navAgent.enabled = false;

            _iceBlockInstance = Instantiate(_iceBlockPrefab, transform.position, transform.rotation)
                .GetComponent<IceBlock>();

            _frozen = true;
        }
        public void Unfreeze()
        {
            if (!_frozen)
                return;

            AiAgent.SetVariableValue("Frozen", false);
            navAgent.enabled = true;

            _iceBlockInstance.Shatter();
            _frozen = false;
        }

        public override void AddSpeedMultiplier(float amount)
        {
            base.AddSpeedMultiplier(amount);
            navAgent.speed = _moveSpeed;

            float animationSpeed = _animationSpeed * _speedMultiplier;
            if (type == EncounterManager.EnemyTypes.BirdOnBird)
            {
                childAnimator.SetFloat(ATTACK_SPEED, animationSpeed);
                return;
            }
            animator.SetFloat(ATTACK_SPEED, animationSpeed);
        }
        public override void RemoveSpeedMultiplier(float amount)
        {
            base.RemoveSpeedMultiplier(amount);
            navAgent.speed = _moveSpeed;

            float animationSpeed = _animationSpeed * _speedMultiplier;
            if (type == EncounterManager.EnemyTypes.BirdOnBird)
            {
                childAnimator.SetFloat(ATTACK_SPEED, animationSpeed);
                return;
            }
            animator.SetFloat(ATTACK_SPEED, animationSpeed);
        }
    }
}