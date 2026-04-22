using System;
using System.Collections;
using Abilities.Attacks;
using Audio;
using UnityEngine;

namespace Entities.Player
{
    public sealed class Fenrir : PlayableCharacter
    {
        private static readonly int ATTACK_FLIP = Animator.StringToHash("attackFlip");

        private static Fenrir INSTANCE;
        
        [SerializeField] private float _lungeForce;
        [SerializeField] private float _lungeDuration;

        [SerializeField] private int _shatterDamage;
        
        private Rigidbody _rigidbody;
        
        private Vector3 _lungeVector;

        public bool AttackFlip;
        private Coroutine _lungeCoroutine;

        public static int SHATTER_DAMAGE;
        public static bool FLIP_ATTACK => INSTANCE.AttackFlip;

        private void Awake()
        {
            SHATTER_DAMAGE = _shatterDamage;
        }
        
        protected override void OnEnable()
        {
            _playerEntity.OnDamageDealt.AddListener(_playerEntity.AddPotionCharges);

            // TODO: Use switch ability
        }

        protected override void OnDisable()
        {
            _playerEntity.OnDamageDealt.RemoveListener(_playerEntity.AddPotionCharges);
        }

        protected override void Start()
        {
            base.Start();
            
            _rigidbody = GetComponent<Rigidbody>();
        }

        protected override void Update()
        {
            base.Update();
        }
        
        private void LateUpdate()
        {
            if (_lungeCoroutine != null)
                _rigidbody.linearVelocity += _lungeVector;
        }

        protected override void PrepareAttack()
        {
            AttackFlip = !AttackFlip;
            _animator.SetBool(ATTACK_FLIP, AttackFlip);
            
            base.PrepareAttack();
            
            if (!_target)
                return;
        
            var targetPos = _target.transform.position;
            targetPos.y = transform.position.y;
            
            transform.LookAt(targetPos);
        }

        protected override void PerformAttack()
        {
            Attack.Create(_attackAbility.AttackPrefab, _playerEntity, _attackPoint);
            
            base.PerformAttack();
        }
        
        public void PerformAttackLunge()
        {
            var toTarget = _target.transform.position - transform.position;
            var distanceToTarget = toTarget.magnitude;
            var direction = toTarget.normalized;

            float projectedDistance = 0.75f * _lungeForce * _lungeDuration;
            float duration = _lungeDuration * Mathf.Clamp01(distanceToTarget / projectedDistance);

            if (_lungeCoroutine != null)
            {
                StopCoroutine(_lungeCoroutine);
                _lungeCoroutine = null;
            }

            _lungeCoroutine = StartCoroutine(LungeFadeCoroutine(direction, _lungeForce, duration));
        }
        
        private IEnumerator LungeFadeCoroutine(Vector3 direction, float originalForce, float duration)
        {
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);

                float force = originalForce * Mathf.Abs(Mathf.Pow(t, 3) - 1);
                _lungeVector = force * direction;

                yield return null;
            }

            _lungeVector = Vector3.zero;
            _lungeCoroutine = null;
        }
        
        protected override void PrepareSwitch()
        {
        }

        protected override void PerformSwitch()
        {
        }

        protected override Entity FindTarget()
        {
            return null;
        }
    }
}