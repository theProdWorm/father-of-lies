using Stats;
using UnityEngine;

namespace Abilities.Attacks
{
    public class ExplodingProjectileAttack : ProjectileAttack
    {
        [SerializeField] private GameObject _explosionPrefab;
        
        private void Start()
        {
            OnAttackFinished.AddListener(CreateExplosion);
        }

        private void CreateExplosion()
        {
            var stats = new AttackStats(_explosionPrefab, _stats);
            Create(_owner, transform.position, transform.rotation, stats);
        }
    }
}