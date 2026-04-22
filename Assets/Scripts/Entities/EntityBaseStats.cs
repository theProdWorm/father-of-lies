using UnityEngine;

namespace Entities
{
    [CreateAssetMenu(fileName = "New Entity Base Stats", menuName = "Stats/Entity Base Stats")]
    public class EntityBaseStats : ScriptableObject
    {
        public int MaxHealth;
        public float MoveSpeed;
    }
}