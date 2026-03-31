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

        private int _damage;

        protected AttackStats _stats;

        protected Entity _owner;
        protected string _hostileTag = "Player";

        public static void Create(Entity owner, Vector3 position, Quaternion rotation, AttackStats stats)
        {
            var attackInstance = Instantiate(stats.Prefab, position, rotation).GetComponent<Attack>();

            attackInstance.SetOwner(owner);
            attackInstance.SetStats(stats);
        }

        public static void Create(Entity owner, Transform parent, AttackStats stats)
        {
            var attackInstance = Instantiate(stats.Prefab, parent).GetComponent<Attack>();

            attackInstance.SetOwner(owner);
            attackInstance.SetStats(stats);
        }

        private void SetStats(AttackStats stats)
        {
            _stats = stats;
            _damage = Mathf.CeilToInt(_stats.Damage * _damageMultiplier);
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

            bool crit = Random.Range(0f, 100f) <= _stats.CritChance;
            int damage = Mathf.CeilToInt(_damage * (crit ? _stats.CritDamage : 1));

            int realDamage = entity.TakeDamage(damage, _owner);

            Vector3 knockbackDirection = (entity.transform.position - _owner.transform.position).normalized;
            entity.KnockBack(knockbackDirection, _knockbackForce, 0.1f);

            OnHitEntity?.Invoke(entity, realDamage);

            return entity;
        }

        protected abstract void OnTriggerEnter(Collider otherCollider);
    }
}