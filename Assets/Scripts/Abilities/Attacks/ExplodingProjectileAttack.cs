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
            Create(_explosionPrefab, _owner, transform.position, transform.rotation);
        }
    }
}