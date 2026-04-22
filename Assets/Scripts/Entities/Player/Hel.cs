using System;
using Abilities.Attacks;
using Audio;
using UnityEngine;

namespace Entities.Player
{
    public class Hel : PlayableCharacter
    {
        [SerializeField] private float _freezeDamageMultiplier;

        public static float FREEZE_DAMAGE_MULTIPLIER;
        
        private void Awake()
        {
            FREEZE_DAMAGE_MULTIPLIER = _freezeDamageMultiplier;
        }
        
        protected override void PerformAttack()
        {
            PlayerController.GainControl();

            Attack.Create(_attackAbility.AttackPrefab, _playerEntity, _attackPoint.position, transform.rotation);
            
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
            return null;
        }
    }
}