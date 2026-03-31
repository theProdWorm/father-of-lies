using UnityEngine;

namespace Entities.Stats
{
    [CreateAssetMenu(fileName = "Player Base Stats", menuName = "Stats/Player Base Stats")]
    public class PlayerBaseStats : EntityBaseStats
    {
        public float CritChance;
        public float CritDamage;
    }
}