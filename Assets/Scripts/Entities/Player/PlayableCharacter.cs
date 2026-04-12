using Abilities;
using Abilities.Attacks;
using UnityEngine;

namespace Entities.Player
{
    public abstract class PlayableCharacter : MonoBehaviour
    {
        protected static readonly int ATTACK = Animator.StringToHash("attack");
        
        [SerializeField] protected Ability _attackAbility;
        [SerializeField] protected Ability _switchAbility;
        
        [SerializeField] protected Animator _animator;

        [SerializeField] protected Transform _attackPoint;
        
        protected PlayerEntity _playerEntity;
        
        protected AbilityTracker _attackAbilityTracker;
        protected AbilityTracker _switchAbilityTracker;

        protected bool _hasControl = true;
        
        protected Entity _target;
        
        protected virtual void Start()
        {
            _playerEntity = GetComponent<PlayerEntity>();
            
            _attackAbilityTracker = new(_attackAbility, PrepareAttack);
            _switchAbilityTracker = new(_switchAbility, PrepareSwitch);
        }

        public void LoseControl() => _hasControl = false;
        public void GainControl() => _hasControl = true;

        protected virtual void PrepareAttack()
        {
            _animator.SetTrigger(ATTACK);
            
            LoseControl();
            
            _target = FindTarget();
        }
        
        protected abstract void PerformAttack();
        
        protected abstract void PrepareSwitch();
        protected abstract void PerformSwitch();

        protected abstract Entity FindTarget();
    }
}