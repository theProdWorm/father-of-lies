using Abilities.Attacks;
using Audio;
using UnityEngine;

namespace Entities.Player
{
    public sealed class Fenrir : PlayableCharacter
    {
        private static readonly int ATTACK_FLIP = Animator.StringToHash("attackFlip");
        
        [SerializeField] private float _lungeForce;
        [SerializeField] private float _lungeDuration;

        public int ShatterDamage { get; private set; }

        public bool AttackFlip;
        
        private void OnEnable()
        {
            _playerEntity.OnDamageDealt.AddListener(_playerEntity.AddPotionCharges);
            
            // TODO: Use switch ability
        }

        private void OnDisable()
        {
            _playerEntity.OnDamageDealt.RemoveListener(_playerEntity.AddPotionCharges);
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
            GainControl();

            Attack.Create(_attackAbility.AttackPrefab, _playerEntity, _attackPoint);
            
            FMODEvents.INSTANCE.PlayEvent(FMODEvents.INSTANCE._playerAttack, transform.position);
        }

        protected override void PrepareSwitch()
        {
        }

        protected override void PerformSwitch()
        {
        }

        protected override Entity FindTarget()
        {
            
        }
    }
}