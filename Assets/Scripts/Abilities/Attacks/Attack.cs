using System;
using Entities;
using Stats;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Abilities.Attacks
{
    public abstract class Attack : MonoBehaviour
    {
        public UnityEvent<Entity, int> OnHitEntity;
        public UnityEvent OnAttackFinished;

        [SerializeField] private float _damageMultiplier;
        [SerializeField] private float _knockbackForce;

        protected int _damage;

        protected AttackStats _stats;

        protected Entity _owner;
        protected string _hostileTag = "Player";

        public static void Create(GameObject prefab, Entity owner, Vector3 position, Quaternion rotation)
        {
            var attackInstance = Instantiate(prefab, position, rotation).GetComponent<Attack>();
            attackInstance.SetOwner(owner);
        }

        public static void Create(GameObject prefab, Entity owner, Transform parent)
        {
            var attackInstance = Instantiate(prefab, parent).GetComponent<Attack>();
            attackInstance.SetOwner(owner);
        }

        private void SetOwner(Entity owner)
        {
            OnHitEntity.AddListener(owner.OnDamageDealt.Invoke);

            _owner = owner;
            tag = owner.tag;

            if (_owner.CompareTag("Player"))
                _hostileTag = "Hostile";
            else if (_owner.CompareTag("Hostile"))
                _hostileTag = "Player";
        }

        public virtual void DestroySelf() => Destroy(transform.gameObject);

        protected Entity PerformAttack(Collider otherCollider)
        {
            if (!otherCollider.CompareTag(_hostileTag))
                return null;

            bool isEntity = otherCollider.gameObject.TryGetComponent<Entity>(out var entity);
            if (!isEntity)
                return null;

            int realDamage = entity.TakeDamage(_damage, _owner);

            Vector3 knockbackDirection = (entity.transform.position - _owner.transform.position).normalized;
            entity.KnockBack(knockbackDirection, _knockbackForce, 0.1f);

            OnHitEntity?.Invoke(entity, realDamage);

            return entity;
        }

        protected abstract void OnTriggerEnter(Collider otherCollider);
    }
}