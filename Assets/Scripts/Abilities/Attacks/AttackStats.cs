using UnityEngine;

namespace Stats
{
    public class AttackStats
    {
        public readonly GameObject Prefab;

        public readonly int Damage;

        public readonly float CritChance;
        public readonly float CritDamage;

        public AttackStats(GameObject prefab, int damage, float critChance, float critDamage)
        {
            Prefab = prefab;
            Damage = damage;
            CritChance = critChance;
            CritDamage = critDamage;
        }

        public AttackStats(GameObject prefab, AttackStats original) :
            this(prefab,
                original.Damage, 
                original.CritChance, 
                original.CritDamage)
        {
        }
    }
}