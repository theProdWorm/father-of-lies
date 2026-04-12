using Abilities;
using Abilities.Attacks;
using Audio;
using UnityEngine;

namespace Entities.Player
{
    public abstract class PlayableCharacter : MonoBehaviour
    {
        protected static readonly int ATTACK = Animator.StringToHash("attack");
        
        [SerializeField] protected Ability _attackAbility;
        [SerializeField] protected Ability _switchAbility;

        [SerializeField] protected GameObject _model;
        [SerializeField] protected Animator _animator;

        [SerializeField] protected Transform _attackPoint;
        
        protected PlayerEntity _playerEntity;
        
        protected AbilityTracker _attackAbilityTracker;
        protected AbilityTracker _switchAbilityTracker;

        protected bool _hasControl = true;
        
        protected Entity _target;
        
        public bool IsSwitchReady => _switchAbilityTracker.RemainingCooldownPercent <= 0;
        public float SwitchCooldownPercent => _switchAbilityTracker.RemainingCooldownPercent;
        
        protected virtual void Start()
        {
            _playerEntity = PlayerEntity.INSTANCE;
            
            _attackAbilityTracker = new(_attackAbility, PrepareAttack);
            _switchAbilityTracker = new(_switchAbility, PrepareSwitch);
        }

        protected virtual void OnEnable()
        {
            _model.SetActive(true);
        }
        
        protected virtual void OnDisable()
        {
            _model.SetActive(false);
        }

        protected virtual void Update()
        {
            _attackAbilityTracker.Update();
            _switchAbilityTracker.Update();
        }
        
        protected virtual void PrepareAttack()
        {
            _animator.SetTrigger(ATTACK);
            
            PlayerController.LoseControl();
            
            _target = FindTarget();
        }

        protected virtual void PerformAttack()
        {
            PlayerController.GainControl();
            FMODEvents.INSTANCE.PlayEvent(FMODEvents.INSTANCE._playerAttack, transform.position);
        }
        
        protected virtual void PrepareSwitch()
        {
        }

        protected virtual void PerformSwitch()
        {
            
        }

        protected abstract Entity FindTarget();
    }
}