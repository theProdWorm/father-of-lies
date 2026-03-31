using UnityEngine;

namespace Abilities.Attacks
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileAttack : Attack
    {
        [SerializeField] protected float _speed;
        [SerializeField] private int _maxHits;

        [SerializeField] private float _range;
        
        [SerializeField] protected Rigidbody _rigidbody;
        
        private int _remainingHits;

        private float _distanceTraveled;
        
        private bool _isDead;

        private void Awake()
        {
            _remainingHits = _maxHits;
            _rigidbody.linearVelocity = transform.forward * _speed;
        }

        protected virtual void FixedUpdate()
        {
            _distanceTraveled += _rigidbody.linearVelocity.magnitude * Time.fixedDeltaTime;
            
            if (_distanceTraveled >= _range)
                Die();
        }
        
        
        private void Die()
        {
            if (_isDead)
                return;
            
            _isDead = true;
            OnAttackFinished?.Invoke();
        }
        
        protected override void OnTriggerEnter(Collider otherCollider)
        {
            if (_isDead || otherCollider.isTrigger)
                return;
            
            if (!otherCollider.CompareTag("Player") && !otherCollider.CompareTag("Hostile"))
            {
                Die();
                return;
            }

            if (!otherCollider.CompareTag(_hostileTag))
                return;
            
            PerformAttack(otherCollider);
            _remainingHits--;
            
            if (_remainingHits == 0)
                Die();
        }
    }
}