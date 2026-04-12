using Abilities.Attacks;
using Audio;
using UnityEngine;

namespace Entities.Player
{
    public class Hel : PlayableCharacter
    {
        protected override void PerformAttack()
        {
            GainControl();

            var pos = 
            Attack.Create(_attackAbility.AttackPrefab, _playerEntity);
            
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