using UnityEngine;

namespace Abilities.Attacks
{
    public class AreaAttack : Attack
    {
        protected override void OnTriggerEnter(Collider otherCollider)
        {
            PerformAttack(otherCollider);
        }
    }
}