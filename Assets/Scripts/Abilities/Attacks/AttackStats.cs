using UnityEngine;

namespace Stats
{
    public class AttackStats
    {
        public readonly GameObject Prefab;

        public readonly int Damage;

        public AttackStats(GameObject prefab, int damage)
        {
            Prefab = prefab;
            Damage = damage;
        }
    }
}