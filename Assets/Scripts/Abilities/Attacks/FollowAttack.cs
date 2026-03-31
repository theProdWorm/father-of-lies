using UnityEngine;

namespace Abilities.Attacks
{
    public class FollowAttack : Attack
    {
        private void Start()
        {
            transform.parent = _owner.transform;
        }
        
        protected override void OnTriggerEnter(Collider otherCollider)
        {
            PerformAttack(otherCollider);
        }
    }
}