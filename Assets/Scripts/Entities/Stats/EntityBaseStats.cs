using UnityEngine;

namespace Entities.Stats
{
    [CreateAssetMenu(fileName = "New Entity Base Stats", menuName = "Stats/Entity Base Stats")]
    public class EntityBaseStats : ScriptableObject
    {
        public int MaxHealth;
        public int Damage;
        
        public float MoveSpeed;
    }
}